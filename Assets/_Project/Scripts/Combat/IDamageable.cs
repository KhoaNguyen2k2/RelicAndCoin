using RelicAndCoin.Combat.Weapons;
using UnityEngine;

namespace RelicAndCoin.Combat
{
    /// <summary>Đối tượng có thể nhận sát thương (kẻ địch, NPC,-Parasitic object…)</summary>
    public interface IDamageable
    {
        /// <summary>Đối tượng còn sống và có thể nhận damage hay không.</summary>
        bool IsAlive { get; }

        /// <summary>Vị trí thế giới của đối tượng (dùng tính hướng đòn / knockback).</summary>
        Vector3 Position { get; }

        /// <summary>Nhận một đòn đã được tính toán sẵn;</param>
        /// <param name="damage">Kết quả tính toán từ DamageCalculator.</param>
        /// <param name="hitDirection">Hướng va chạm (dùng knockdown / knockback).</param>
        void TakeDamage(DamageResult damage, Vector2 hitDirection);
    }
}
