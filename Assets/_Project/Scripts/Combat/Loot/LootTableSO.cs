using System;
using System.Collections.Generic;
using RelicAndCoin.Combat.Runes;
using RelicAndCoin.Core.Difficulty;
using UnityEngine;

namespace RelicAndCoin.Combat.Loot
{
    /// <summary>
    /// ScriptableObject lưu trữ danh mục các vật phẩm, vũ khí, rune có thể xuất hiện trong hầm ngục.
    /// </summary>
    [CreateAssetMenu(menuName = "RelicAndCoin/Combat/Loot Table", fileName = "LootTable")]
    public sealed class LootTableSO : ScriptableObject
    {
        [Tooltip("Danh sách các phần tử rơi đồ.")]
        public List<LootEntry> Entries = new();

        /// <summary>
        /// Lọc danh sách phần tử theo một bậc độ hiếm cụ thể.
        /// </summary>
        public List<LootEntry> GetEntriesByRarity(RuneRarity rarity)
        {
            var list = new List<LootEntry>();
            for (int i = 0; i < Entries.Count; i++)
            {
                if (Entries[i].Rarity == rarity)
                    list.Add(Entries[i]);
            }
            return list;
        }

        /// <summary>
        /// Lọc danh sách phần tử trong một khoảng độ hiếm từ minRarity đến maxRarity.
        /// </summary>
        public List<LootEntry> GetEntriesInRange(RuneRarity minRarity, RuneRarity maxRarity)
        {
            var list = new List<LootEntry>();
            for (int i = 0; i < Entries.Count; i++)
            {
                if (Entries[i].Rarity >= minRarity && Entries[i].Rarity <= maxRarity)
                    list.Add(Entries[i]);
            }
            return list;
        }
    }
}
