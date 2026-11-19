using RelicAndCoin.Core.Stats;
using UnityEngine;

namespace RelicAndCoin.Combat.Weapons
{
    /// <summary>
    /// MonoBehaviour mỏng gắn lên Player: trang bị WeaponDataSO, kiểm tra thời gian hồi đòn,
    /// tiêu hao Stamina/Mana qua CharacterStatsController, tính sát thương bằng DamageCalculator
    /// và kích hoạt WeaponHitbox cho mỗi mục tiêu mới va chạm.
    /// </summary>
    [RequireComponent(typeof(CharacterStatsController))]
    public sealed class WeaponController : MonoBehaviour
    {
        [Header("Vũ khí")]
        [SerializeField] private WeaponDataSO _weaponData;

        [Header("Tham chiếu")]
        [SerializeField] private WeaponHitbox _hitbox;
        [SerializeField] private Transform    _hitOrigin; // điểm lấy hướng đòn (mặc định transform của chính weapon)

        private CharacterStatsController _stats;
        private readonly DamageCalculator _calculator = new();
        private float _nextAttackTime;

        private void Awake()
        {
            _stats = GetComponent<CharacterStatsController>();
            if (_hitOrigin == null) _hitOrigin = transform;

            _hitbox.OnNewHit += OnNewHit;
        }

        private void OnDestroy() => _hitbox.OnNewHit -= OnNewHit;

        /// <summary>Gọi từ Player FSM (AttackState) khi người chơi nhấn tấn công.</summary>
        public bool TryAttack()
        {
            if (Time.time < _nextAttackTime) return false;
            if (_weaponData == null) return false;
            if (!_stats.TryUseStamina(_weaponData.StaminaCost)) return false;
            if (!_stats.TryUseMana(_weaponData.ManaCost)) return false;

            _nextAttackTime = Time.time + 1f / _weaponData.AttackSpeed;
            _hitbox.BeginSwing();
            return true;
        }

        private void OnNewHit(IDamageable target)
        {
            if (!target.IsAlive) return;

            float targetDefense = GetTargetDefense(target);
            var damage = _calculator.Calculate(
                rng: null,
                attackerDamage: _stats.AttackDamage,
                critChance:   _stats.CritChance,
                weaponBaseDamage: _weaponData.BaseDamage,
                element:   _weaponData.Element,
                targetDefense: targetDefense);

            Vector2 hitDirection = ((Vector2)target.Position - (Vector2)_hitOrigin.position).normalized;
            target.TakeDamage(damage, hitDirection);
        }

        private static float GetTargetDefense(IDamageable target)
        {
            var stats = GetStats(target);
            return stats != null ? stats.Defense : 0f;
        }

        private static CharacterStatsController GetStats(IDamageable target)
        {
            var mono = target as MonoBehaviour;
            return mono != null ? mono.GetComponent<CharacterStatsController>() : null;
        }

        // Expose cho các hệ thống khác cần biết weapon hiện tại (UI, tooltip, rune)
        public WeaponDataSO WeaponData => _weaponData;
        public float AttackRange => _weaponData != null ? _weaponData.AttackRange : 0f;
    }
}
