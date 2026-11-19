using System;
using RelicAndCoin.Combat.Runes;
using RelicAndCoin.Core.Difficulty;
using UnityEngine;

namespace RelicAndCoin.Combat.Loot
{
    /// <summary>
    /// Cấu trúc lưu trữ tỷ lệ % xuất hiện cho từng bậc độ hiếm và dải trang bị của quái vật.
    /// </summary>
    [Serializable]
    public sealed class DifficultyLootTier
    {
        [Range(0f, 100f)] public float NormalChance = 80f;
        [Range(0f, 100f)] public float RareChance = 50f;
        [Range(0f, 100f)] public float EpicChance = 15f;
        [Range(0f, 100f)] public float LegendaryChance = 5f;
        [Range(0f, 100f)] public float MythicalChance = 2.5f;

        [Header("Dải trang bị mặc sẵn cho quái vật")]
        public RuneRarity MinEnemyRarity = RuneRarity.Normal;
        public RuneRarity MaxEnemyRarity = RuneRarity.Epic;
    }

    /// <summary>
    /// ScriptableObject cấu hình tỷ lệ rơi đồ gốc (90% - 95%) cùng bảng tỷ lệ 5 bậc độ hiếm theo 3 độ khó.
    /// </summary>
    [CreateAssetMenu(menuName = "RelicAndCoin/Combat/Loot Drop Config", fileName = "LootDropConfig")]
    public sealed class LootDropConfigSO : ScriptableObject
    {
        [Header("Tỷ lệ rơi đồ gốc")]
        [Tooltip("Xác suất rơi ít nhất 1 món đồ khi hạ quái hoặc mở hòm (90% - 95%).")]
        [Range(0.8f, 1f)] public float BaseDropRate = 0.92f;

        [Header("Bảng tỷ lệ theo độ khó")]
        public DifficultyLootTier NormalTier = new()
        {
            NormalChance = 80f,
            RareChance = 50f,
            EpicChance = 15f,
            LegendaryChance = 5f,
            MythicalChance = 2.5f,
            MinEnemyRarity = RuneRarity.Normal,
            MaxEnemyRarity = RuneRarity.Epic
        };

        public DifficultyLootTier HardTier = new()
        {
            NormalChance = 70f,
            RareChance = 55f,
            EpicChance = 17f,
            LegendaryChance = 8f,
            MythicalChance = 4f,
            MinEnemyRarity = RuneRarity.Normal,
            MaxEnemyRarity = RuneRarity.Legendary
        };

        public DifficultyLootTier NightmareTier = new()
        {
            NormalChance = 50f,
            RareChance = 60f,
            EpicChance = 20f,
            LegendaryChance = 10f,
            MythicalChance = 5f,
            MinEnemyRarity = RuneRarity.Epic,
            MaxEnemyRarity = RuneRarity.Mythical
        };

        [Header("Ưu tiên Rương Kho báu (TreasureChest)")]
        [Tooltip("Tỷ lệ % cộng thêm cho bậc Rare khi mở rương kho báu.")]
        [Range(0f, 50f)] public float TreasureRareBonus = 25f;

        [Tooltip("Tỷ lệ % cộng thêm cho bậc Epic khi mở rương kho báu.")]
        [Range(0f, 50f)] public float TreasureEpicBonus = 20f;

        [Tooltip("Tỷ lệ % cộng thêm cho bậc Legendary khi mở rương kho báu.")]
        [Range(0f, 50f)] public float TreasureLegendaryBonus = 15f;

        [Tooltip("Tỷ lệ % cộng thêm cho bậc Mythical khi mở rương kho báu.")]
        [Range(0f, 50f)] public float TreasureMythicalBonus = 8f;

        public DifficultyLootTier GetTier(DifficultyMode mode) => mode switch
        {
            DifficultyMode.Hard      => HardTier,
            DifficultyMode.Nightmare => NightmareTier,
            _                        => NormalTier
        };
    }
}
