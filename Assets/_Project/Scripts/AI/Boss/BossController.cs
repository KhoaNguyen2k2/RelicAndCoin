using System;
using RelicAndCoin.AI.Enemies;
using RelicAndCoin.Combat.Weapons;
using RelicAndCoin.Core.Difficulty;
using RelicAndCoin.AI.Enemies.States;
using RelicAndCoin.Combat;
using RelicAndCoin.Combat.StatusEffects;
using RelicAndCoin.Core.StateMachine;
using RelicAndCoin.Core.Stats;
using UnityEngine;

namespace RelicAndCoin.AI.Boss
{
    /// <summary>
    /// MonoBehaviour mỏng: dieu khien Boss da giai doan.
    /// Theo doi ty lap HP hien tai de kich chuyen pha OnPhaseChanged
    /// va cuong hoa chi so qua StatModifier.
    /// </summary>
    public sealed class BossController : MonoBehaviour, IDamageable
    {
        [Header("Data")]
        [SerializeField] private EnemyDataSO _data;
        [SerializeField] private DifficultyConfigSO _difficulty;
        [SerializeField] private BossPhaseConfig[] _phases = Array.Empty<BossPhaseConfig>();

        [Header("References")]
        [SerializeField] private Transform _player;

        private CharacterStatsModel _stats;
        private StatusEffectSystem _statusEffects;
        private StateMachine _stateMachine;
        private EnemyContext _ctx;
        private int _currentPhaseIndex;

        public bool IsAlive => _stats != null && _stats.CurrentHP > 0f;
        public Vector3 Position => transform.position;
        public Action<int> OnPhaseChanged; // (chi so pha moi)

        private void Awake()
        {
            float healthMult = _difficulty != null ? _difficulty.EnemyHealthMultiplier : 1f;
            float damageMult = _difficulty != null ? _difficulty.EnemyDamageMultiplier : 1f;

            _stats = new CharacterStatsModel(
                baseMaxHP: _data.MaxHP * healthMult,
                baseMaxStamina: 100f,
                baseMaxMana: 0f);
            _stats.AddModifier(new StatModifier(
                StatType.AttackDamage, ModifierType.Flat, _data.AttackDamage * damageMult, this));

            _statusEffects = new StatusEffectSystem();
            _stateMachine = new StateMachine();

            _ctx = new EnemyContext(
                rigidbody: GetComponent<Rigidbody2D>(),
                stats: _stats,
                statusEffects: _statusEffects,
                data: _data,
                stateMachine: _stateMachine);

            _ctx.TargetPosition = transform.position;
            _stateMachine.ChangeState(new EnemyPatrolState(_ctx, transform.position));
        }

        private void Update()
        {
            _stats.Tick(UnityEngine.Time.deltaTime);
            _statusEffects.Tick(UnityEngine.Time.deltaTime, dmg =>
            {
                _stats.TakeDamage(dmg);
                return IsAlive;
            });
            _stateMachine.Tick(UnityEngine.Time.deltaTime);
            UpdateDistanceToPlayer();
            CheckPhaseTransition();
        }

        private void FixedUpdate()
        {
            _stateMachine.FixedTick(UnityEngine.Time.fixedDeltaTime);
        }

        private void UpdateDistanceToPlayer()
        {
            if (_player != null)
                _ctx.DistanceToPlayer = Vector2.Distance(transform.position, _player.position);
        }

        private void CheckPhaseTransition()
        {
            if (_currentPhaseIndex >= _phases.Length) return;

            float hpRatio = _stats.MaxHP > 0f ? _stats.CurrentHP / _stats.MaxHP : 0f;
            if (hpRatio <= _phases[_currentPhaseIndex].HealthThresholdPercent)
            {
                ActivatePhase(_currentPhaseIndex);
                OnPhaseChanged?.Invoke(_currentPhaseIndex);
                _currentPhaseIndex++;
            }
        }

        private void ActivatePhase(int index)
        {
            var phase = _phases[index];
            float speedPercent = phase.SpeedMultiplier - 1f;
            float damagePercent = phase.DamageMultiplier - 1f;

            if (Mathf.Approximately(speedPercent, 0f) == false)
                _stats.AddModifier(new StatModifier(StatType.MoveSpeed, ModifierType.Percent, speedPercent, this));
            if (Mathf.Approximately(damagePercent, 0f) == false)
                _stats.AddModifier(new StatModifier(StatType.AttackDamage, ModifierType.Percent, damagePercent, this));
        }

        public void TakeDamage(DamageResult damage, Vector2 hitDirection)
        {
            if (!IsAlive) return;
            _stats.TakeDamage(damage.FinalDamage);
        }
    }
}
