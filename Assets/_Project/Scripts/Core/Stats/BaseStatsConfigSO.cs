using UnityEngine;

namespace RelicAndCoin.Core.Stats
{
    [CreateAssetMenu(menuName = "RelicAndCoin/Config/Base Stats Config", fileName = "BaseStatsConfig")]
    public sealed class BaseStatsConfigSO : ScriptableObject
    {
        [Header("Chỉ số tối đa gốc")]
        [Min(1f)] public float MaxHP      = 100f;
        [Min(1f)] public float MaxStamina = 100f;
        [Min(1f)] public float MaxMana    = 100f;

        [Header("Hồi phục tự nhiên (mỗi giây)")]
        [Min(0f)] public float StaminaRegenPerSecond = 10f;
        [Min(0f)] public float ManaRegenPerSecond    = 5f;
        [Min(0f)] public float HpRegenPerSecond      = 20f;

        [Header("Fatigue")]
        [Min(0.1f)] public float FatigueDuration  = 3f;
        [Min(0f)]   public float StaminaRegenDelay = 1f;
        [Min(0f)]   public float HpRegenDelay      = 2f;

        [Header("Chỉ số chiến đấu gốc")]
        [Min(0f)] public float AttackDamage = 10f;
        [Min(0f)] public float Defense      = 0f;
        [Min(0f)] public float MoveSpeed    = 5f;
        [Range(0f, 1f)] public float CritChance = 0.05f;

        public CharacterStatsModel CreateModel() => new(
            MaxHP,
            MaxStamina,
            MaxMana,
            FatigueDuration,
            StaminaRegenPerSecond,
            ManaRegenPerSecond,
            StaminaRegenDelay,
            HpRegenPerSecond,
            HpRegenDelay
        );
    }
}
