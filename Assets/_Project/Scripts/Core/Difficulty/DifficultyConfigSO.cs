using UnityEngine;

namespace RelicAndCoin.Core.Difficulty
{
    [CreateAssetMenu(menuName = "RelicAndCoin/Config/Difficulty Config", fileName = "DifficultyConfig")]
    public sealed class DifficultyConfigSO : ScriptableObject
    {
        public DifficultyMode Mode = DifficultyMode.Normal;

        [Header("Kẻ địch — Chỉ số cơ bản")]
        [Min(0.1f)] public float EnemyHealthMultiplier  = 1f;
        [Min(0.1f)] public float EnemyDamageMultiplier  = 1f;

        [Header("Kẻ địch — AI & Hành vi")]
        [Min(0.1f)] public float PatrolSpeedMultiplier    = 1f;
        [Min(0.1f)] public float DetectRangeMultiplier    = 1f;
        [Min(0.1f)] public float AttackCooldownMultiplier = 1f;
        public bool CanDodgeAndPunish = false;

        [Header("Cửa hàng")]
        [Min(0.1f)] public float ShopPriceMultiplier    = 1f;

        [Header("Lời nguyền")]
        [Range(0f, 1f)] public float CurseChanceMultiplier = 1f;
    }
}
