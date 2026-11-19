using System;
using System.Collections.Generic;
using RelicAndCoin.Combat.Runes;
using RelicAndCoin.Core.Difficulty;

namespace RelicAndCoin.Combat.Loot
{
    /// <summary>
    /// Lớp Pure C# xử lý logic sinh trang bị mặc sẵn cho quái vật theo độ khó
    /// và tính toán phần thưởng rớt ra khi quái bị hạ gục hoặc khi mở Hòm / Rương.
    /// </summary>
    public static class LootDropService
    {
        /// <summary>
        /// Tự động chọn 1 trang bị từ LootTable phù hợp với dải độ hiếm của độ khó hiện tại để trang bị cho quái vật.
        /// </summary>
        public static LootEntry EquipEnemy(
            LootTableSO table,
            LootDropConfigSO config,
            DifficultyMode difficulty,
            Random rng = null)
        {
            if (table == null || table.Entries == null || table.Entries.Count == 0)
                return null;

            rng ??= new Random();
            var tier = config != null ? config.GetTier(difficulty) : new DifficultyLootTier();

            // Lọc danh sách trang bị trong dải độ hiếm của quái
            var validEntries = table.GetEntriesInRange(tier.MinEnemyRarity, tier.MaxEnemyRarity);
            if (validEntries.Count == 0)
            {
                // Fallback nếu không có món nào nằm đúng dải: lấy ngẫu nhiên 1 phần tử trong bảng
                return table.Entries[rng.Next(table.Entries.Count)];
            }

            return PickWeightedEntry(validEntries, rng);
        }

        /// <summary>
        /// Tính toán danh sách vật phẩm rơi ra dựa theo nguồn rớt, độ khó, và bảng cấu hình.
        /// </summary>
        public static List<LootEntry> CalculateLoot(
            LootSourceType sourceType,
            DifficultyMode difficulty,
            LootTableSO table,
            LootDropConfigSO config,
            LootEntry equippedItem = null,
            Random rng = null)
        {
            var drops = new List<LootEntry>();
            if (table == null || table.Entries == null || table.Entries.Count == 0)
                return drops;

            rng ??= new Random();
            var tier = config != null ? config.GetTier(difficulty) : new DifficultyLootTier();
            float baseDropRate = config != null ? config.BaseDropRate : 0.92f;

            switch (sourceType)
            {
                case LootSourceType.Enemy:
                    // 1. Quái vật chắc chắn rớt trang bị đang mang (nếu có)
                    if (equippedItem != null)
                    {
                        drops.Add(equippedItem);
                    }

                    // 2. Thử xác suất rớt thêm 1 món phụ (thuốc/nguyên liệu/vật phẩm tiêu hao)
                    if (rng.NextDouble() <= baseDropRate)
                    {
                        var rarity = RollRarity(tier, false, config, rng);
                        var extraEntry = PickEntryByRarity(table, rarity, rng);
                        if (extraEntry != null)
                            drops.Add(extraEntry);
                    }
                    break;

                case LootSourceType.Crate:
                    // Hòm gỗ: tỷ lệ theo BaseDropRate, ưu tiên rơi 1 vật phẩm cấp Normal/Rare
                    if (rng.NextDouble() <= baseDropRate)
                    {
                        var rarity = RollRarity(tier, false, config, rng);
                        // Giới hạn tối đa là Rare đối với hòm gỗ
                        if (rarity > RuneRarity.Rare) rarity = RuneRarity.Rare;

                        var crateEntry = PickEntryByRarity(table, rarity, rng);
                        if (crateEntry != null)
                            drops.Add(crateEntry);
                    }
                    break;

                case LootSourceType.NormalChest:
                    // Rương thường: rơi 1 - 2 món đồ
                    int normalChestCount = rng.Next(1, 3);
                    for (int i = 0; i < normalChestCount; i++)
                    {
                        var rarity = RollRarity(tier, false, config, rng);
                        var entry = PickEntryByRarity(table, rarity, rng);
                        if (entry != null)
                            drops.Add(entry);
                    }
                    break;

                case LootSourceType.TreasureChest:
                    // Rương kho báu: luôn rơi 100%, từ 2 - 3 món, có hệ số cộng thêm độ hiếm cao
                    int treasureCount = rng.Next(2, 4);
                    for (int i = 0; i < treasureCount; i++)
                    {
                        var rarity = RollRarity(tier, true, config, rng);
                        // Đảm bảo rương kho báu tối thiểu phải đạt bậc Rare trở lên
                        if (rarity < RuneRarity.Rare)
                            rarity = RuneRarity.Rare;

                        var entry = PickEntryByRarity(table, rarity, rng);
                        if (entry != null)
                            drops.Add(entry);
                    }
                    break;
            }

            return drops;
        }

        /// <summary>
        /// Roll bậc độ hiếm từ cao xuống thấp dựa trên bảng % của độ khó.
        /// </summary>
        private static RuneRarity RollRarity(
            DifficultyLootTier tier,
            bool isTreasureChest,
            LootDropConfigSO config,
            Random rng)
        {
            float bonusRare = (isTreasureChest && config != null) ? config.TreasureRareBonus : 0f;
            float bonusEpic = (isTreasureChest && config != null) ? config.TreasureEpicBonus : 0f;
            float bonusLeg  = (isTreasureChest && config != null) ? config.TreasureLegendaryBonus : 0f;
            float bonusMyth = (isTreasureChest && config != null) ? config.TreasureMythicalBonus : 0f;

            // Roll từ bậc cao nhất (Mythical) xuống thấp nhất
            double rollMyth = rng.NextDouble() * 100.0;
            if (rollMyth <= (tier.MythicalChance + bonusMyth))
                return RuneRarity.Mythical;

            double rollLeg = rng.NextDouble() * 100.0;
            if (rollLeg <= (tier.LegendaryChance + bonusLeg))
                return RuneRarity.Legendary;

            double rollEpic = rng.NextDouble() * 100.0;
            if (rollEpic <= (tier.EpicChance + bonusEpic))
                return RuneRarity.Epic;

            double rollRare = rng.NextDouble() * 100.0;
            if (rollRare <= (tier.RareChance + bonusRare))
                return RuneRarity.Rare;

            return RuneRarity.Normal;
        }

        private static LootEntry PickEntryByRarity(LootTableSO table, RuneRarity targetRarity, Random rng)
        {
            var matching = table.GetEntriesByRarity(targetRarity);
            if (matching.Count > 0)
                return PickWeightedEntry(matching, rng);

            // Nếu không có phần tử khớp chính xác bậc targetRarity, lấy món gần nhất
            return PickWeightedEntry(table.Entries, rng);
        }

        private static LootEntry PickWeightedEntry(IReadOnlyList<LootEntry> entries, Random rng)
        {
            if (entries == null || entries.Count == 0) return null;

            int totalWeight = 0;
            for (int i = 0; i < entries.Count; i++)
            {
                totalWeight += Math.Max(1, entries[i].Weight);
            }

            int roll = rng.Next(0, totalWeight);
            int current = 0;
            for (int i = 0; i < entries.Count; i++)
            {
                current += Math.Max(1, entries[i].Weight);
                if (roll < current)
                    return entries[i];
            }

            return entries[0];
        }
    }
}
