using RelicAndCoin.AI.Enemies.States;
using RelicAndCoin.Combat;
using RelicAndCoin.Combat.Loot;
using RelicAndCoin.Combat.StatusEffects;
using RelicAndCoin.Combat.Weapons;
using RelicAndCoin.Core.Difficulty;
using RelicAndCoin.Core.StateMachine;
using RelicAndCoin.Core.Stats;
using RelicAndCoin.Player;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace RelicAndCoin.AI.Enemies
{
    /// <summary>
    /// MonoBehaviour mỏng: cầu nối giữa Unity lifecycle và hệ thống AI quái vật.
    /// Kế thừa IDamageable để nhận sát thương từ WeaponHitbox.
    /// Hỗ trợ tự động mặc trang bị theo độ khó và rớt đồ khi bị tiêu diệt.
    /// </summary>
    public sealed class EnemyController : MonoBehaviour, IDamageable
    {
        [Header("Data")]
        [SerializeField] private EnemyDataSO _data;
        [SerializeField] private DifficultyConfigSO _difficulty;

        [Header("Loot Configuration")]
        [SerializeField] private LootTableSO _lootTable;
        [SerializeField] private LootDropConfigSO _lootConfig;

        [Header("References")]
        [SerializeField] private Transform _player;

        private CharacterStatsModel _stats;
        private StatusEffectSystem _statusEffects;
        private StateMachine _stateMachine;
        private EnemyContext _ctx;
        private bool _isDead = false;

        private PlayerController _playerController;
        private CharacterStatsController _playerStats;

        // Các instance State được tạo sẵn một lần duy nhất
        private EnemyPatrolState _patrolState;
        private EnemyChaseState  _chaseState;
        private EnemyAttackState _attackState;

        public bool IsAlive => !_isDead && _stats != null && _stats.CurrentHP > 0f;
        public Vector3 Position => transform.position;
        public LootEntry EquippedLoot { get; private set; }
        public IReadOnlyList<LootEntry> DroppedLoot { get; private set; }

        public event Action<IReadOnlyList<LootEntry>> OnEnemyDefeated;

        private void Awake()
        {
            var diffMode      = _difficulty != null ? _difficulty.Mode : DifficultyMode.Normal;
            float healthMult  = _difficulty != null ? _difficulty.EnemyHealthMultiplier : 1f;
            float damageMult  = _difficulty != null ? _difficulty.EnemyDamageMultiplier : 1f;
            float patrolMult  = _difficulty != null ? _difficulty.PatrolSpeedMultiplier : 1f;
            float detectMult  = _difficulty != null ? _difficulty.DetectRangeMultiplier : 1f;
            float cdMult      = _difficulty != null ? _difficulty.AttackCooldownMultiplier : 1f;
            bool canDodge     = _difficulty != null && _difficulty.CanDodgeAndPunish;

            _stats = new CharacterStatsModel(
                baseMaxHP: _data.MaxHP * healthMult,
                baseMaxStamina: 100f,
                baseMaxMana: 0f);
            _stats.AddModifier(new StatModifier(
                StatType.AttackDamage, ModifierType.Flat, _data.AttackDamage * damageMult, this));

            // Sinh trang bị mặc sẵn theo đúng dải độ hiếm của độ khó
            EquippedLoot = LootDropService.EquipEnemy(_lootTable, _lootConfig, diffMode);
            if (EquippedLoot != null)
            {
                // Nếu quái cầm vũ khí, cộng thêm chỉ số sát thương vũ khí
                if (EquippedLoot.Weapon != null)
                {
                    _stats.AddModifier(new StatModifier(
                        StatType.AttackDamage, ModifierType.Flat, EquippedLoot.Weapon.BaseDamage, this));
                }
                // Nếu quái mang Rune, cộng các thuộc tính của Rune
                else if (EquippedLoot.Rune != null && EquippedLoot.Rune.StatModifiers != null)
                {
                    foreach (var mod in EquippedLoot.Rune.StatModifiers)
                        _stats.AddModifier(mod);
                }

                Debug.Log($"[{_data.EnemyName}] trang bị [{EquippedLoot.Rarity}] {EquippedLoot.DisplayName}");
            }

            _statusEffects = new StatusEffectSystem();
            _stateMachine = new StateMachine();

            _ctx = new EnemyContext(
                rigidbody: GetComponent<Rigidbody2D>(),
                stats: _stats,
                statusEffects: _statusEffects,
                data: _data,
                stateMachine: _stateMachine);

            _ctx.TargetPosition     = transform.position;
            _ctx.PatrolSpeed        = _data.MoveSpeed * patrolMult;
            _ctx.ChaseSpeed         = _data.MoveSpeed * (patrolMult > 1f ? patrolMult : 1f);
            _ctx.DetectRange        = _data.DetectRange * detectMult;
            _ctx.AttackCooldown     = _data.AttackCooldown * cdMult;
            _ctx.CanDodgeAndPunish  = canDodge;

            // Tự tìm Player nếu chưa được gán trong Inspector
            if (_player == null)
            {
                var foundPlayer = GameObject.FindWithTag("Player");
                if (foundPlayer != null)
                    _player = foundPlayer.transform;
            }

            if (_player != null)
            {
                _ctx.PlayerTransform = _player;
                _playerController    = _player.GetComponent<PlayerController>();
                _playerStats         = _player.GetComponent<CharacterStatsController>();
            }

            // Khởi tạo các State một lần duy nhất (Zero GC Allocation)
            _patrolState = new EnemyPatrolState(_ctx, transform.position);
            _chaseState  = new EnemyChaseState(_ctx);
            _attackState = new EnemyAttackState(_ctx);

            // Wiring các chuyển đổi giữa các state
            _patrolState.SetTransitions(_chaseState);
            _chaseState.SetTransitions(_patrolState, _attackState);
            _attackState.SetTransitions(_chaseState);

            _stateMachine.ChangeState(_patrolState);
        }

        private void Update()
        {
            if (_isDead) return;

            _stats.Tick(UnityEngine.Time.deltaTime);
            _statusEffects.Tick(UnityEngine.Time.deltaTime, dmg =>
            {
                _stats.TakeDamage(dmg);
                if (!IsAlive)
                {
                    Die();
                    return false;
                }
                return true;
            });

            if (_isDead) return;

            UpdatePlayerStatus();
            _stateMachine.Tick(UnityEngine.Time.deltaTime);
            UpdateDistanceToPlayer();
        }

        private void FixedUpdate()
        {
            if (_isDead) return;
            _stateMachine.FixedTick(UnityEngine.Time.fixedDeltaTime);
        }

        private void UpdatePlayerStatus()
        {
            if (_player == null) return;

            // Kiểm tra trạng thái tấn công của Player
            _ctx.IsPlayerAttacking = Input.GetKeyDown(KeyCode.J) || Input.GetKey(KeyCode.J);

            // Kiểm tra trạng thái Fatigue của Player
            if (_playerStats != null)
                _ctx.IsPlayerFatigued = _playerStats.IsFatigued;
        }

        private void UpdateDistanceToPlayer()
        {
            if (_player != null)
                _ctx.DistanceToPlayer = Vector2.Distance(transform.position, _player.position);
        }

        public void TakeDamage(DamageResult damage, Vector2 hitDirection)
        {
            if (!IsAlive) return;

            _stats.TakeDamage(damage.FinalDamage);
            if (!IsAlive)
            {
                Die();
            }
        }

        private void Die()
        {
            if (_isDead) return;
            _isDead = true;

            var mode = _difficulty != null ? _difficulty.Mode : DifficultyMode.Normal;
            var drops = LootDropService.CalculateLoot(LootSourceType.Enemy, mode, _lootTable, _lootConfig, EquippedLoot);
            DroppedLoot = drops;

            OnEnemyDefeated?.Invoke(drops);

            string lootStr = drops.Count > 0
                ? string.Join(", ", drops.ConvertAll(d => $"[{d.Rarity}] {d.DisplayName}"))
                : "Không rơi đồ";
            Debug.Log($"[{_data.EnemyName}] đã bị hạ gục! Phần thưởng rớt ({drops.Count}): {lootStr}");

            // Ngắt va chạm vật lý để người chơi không bị cản
            var col = GetComponent<Collider2D>();
            if (col != null) col.enabled = false;
            var rb = GetComponent<Rigidbody2D>();
            if (rb != null) rb.simulated = false;

            Destroy(gameObject, 0.25f);
        }
    }
}
