using System;
using RelicAndCoin.Core.Stats;

namespace RelicAndCoin.Combat.Weapons
{
    /// <summary>Kết quả tính toán sát thương một đòn.</summary>
    public readonly struct DamageResult
    {
        /// <summary>Sát thương cuối cùng đã trừ phòng thủ (không dưới 0).</summary>
        public readonly float FinalDamage;
        /// <summary>Sát thương gốc trước khi trừ phòng thủ.</summary>
        public readonly float RawDamage;
        /// <summary>Đòn này có trúng chí mạng hay không.</summary>
        public readonly bool IsCritical;
        /// <summary>Nguyên tố của đòn đánh.</summary>
        public readonly ElementType Element;

        public DamageResult(float finalDamage, float rawDamage, bool isCritical, ElementType element)
        {
            FinalDamage = finalDamage;
            RawDamage = rawDamage;
            IsCritical = isCritical;
            Element = element;
        }
    }

    /// <summary>
    /// Pure C# — tính toán sát thương đầu ra dựa trên chỉ số người đánh, chỉ số vũ khí
    /// và phòng thủ của mục tiêu. Không phụ thuộc UnityEngine.
    /// </summary>
    public sealed class DamageCalculator
    {
        private const float CritMultiplier = 2f;
        private const float MinFinalDamage = 0f;

        /// <summary>
        /// Tính toán sát thương của một đòn đánh.
        /// </summary>
        /// <param name="rng">Generator ngẫu nhiên (để kiểm định deterministik). Truyền null để dùng Random.Shared.</param>
        /// <param name="attackerDamage">Chỉ số AttackDamage của người đánh.</param>
        /// <param name="critChance">Xác suất chí mạng (0–1) của người đánh.</param>
        /// <param name="weaponBaseDamage">Sát thương cơ bản của vũ khí.</param>
        /// <param name="element">Nguyên tố của vũ khí.</param>
        /// <param name="targetDefense">Chỉ số Defense của mục tiêu.</param>
        public DamageResult Calculate(
            Random rng,
            float attackerDamage,
            float critChance,
            float weaponBaseDamage,
            ElementType element,
            float targetDefense)
        {
            rng ??= new System.Random();

            // Sát thương gốc = base của vũ khí + statAttackDamage của người đánh (cộng Flat).
            float rawDamage = weaponBaseDamage + attackerDamage;

            // Xác định chí mạng.
            bool isCritical = rng.NextDouble() < critChance;
            if (isCritical) rawDamage *= CritMultiplier;

            // Trừ phòng thủ (giảm trực tiếp), không dưới 0.
            float finalDamage = Math.Max(rawDamage - targetDefense, MinFinalDamage);

            return new DamageResult(finalDamage, rawDamage, isCritical, element);
        }
    }
}
