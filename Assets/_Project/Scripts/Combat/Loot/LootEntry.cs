using System;
using RelicAndCoin.Combat.Runes;
using RelicAndCoin.Combat.Weapons;
using RelicAndCoin.Inventory;
using UnityEngine;

namespace RelicAndCoin.Combat.Loot
{
    /// <summary>
    /// Định danh nguồn rơi đồ trong hầm ngục.
    /// </summary>
    public enum LootSourceType
    {
        Enemy,          // Quái vật mang trang bị
        Crate,          // Hòm gỗ (chứa tiền, thuốc, vật phẩm tiêu hao)
        NormalChest,    // Rương thường
        TreasureChest   // Rương kho báu (rơi trang bị/runes cao cấp hơn)
    }

    /// <summary>
    /// Phân loại phần thưởng trong một ô loot.
    /// </summary>
    public enum LootRewardType
    {
        Item,
        Weapon,
        Rune
    }

    /// <summary>
    /// Cấu trúc lưu trữ một phần tử rơi đồ trong LootTable.
    /// Có thể là ItemDataSO, WeaponDataSO hoặc RuneDataSO kèm bậc độ hiếm.
    /// </summary>
    [Serializable]
    public sealed class LootEntry
    {
        [Tooltip("Phân loại phần thưởng: Vật phẩm, Vũ khí, hoặc Rune.")]
        public LootRewardType RewardType = LootRewardType.Item;

        [Tooltip("Dữ liệu vật phẩm (nếu RewardType = Item).")]
        public ItemDataSO Item;

        [Tooltip("Dữ liệu vũ khí (nếu RewardType = Weapon).")]
        public WeaponDataSO Weapon;

        [Tooltip("Dữ liệu rune (nếu RewardType = Rune).")]
        public RuneDataSO Rune;

        [Tooltip("Bậc độ hiếm của phần tử này.")]
        public RuneRarity Rarity = RuneRarity.Normal;

        [Tooltip("Số lượng tối thiểu khi rơi.")]
        [Min(1)] public int MinQuantity = 1;

        [Tooltip("Số lượng tối đa khi rơi.")]
        [Min(1)] public int MaxQuantity = 1;

        [Tooltip("Trọng số xác suất xuất hiện.")]
        [Min(1)] public int Weight = 10;

        public string DisplayName => RewardType switch
        {
            LootRewardType.Item   => Item != null ? Item.DisplayName : "Vật phẩm chưa gán",
            LootRewardType.Weapon => Weapon != null ? Weapon.DisplayName : "Vũ khí chưa gán",
            LootRewardType.Rune   => Rune != null ? Rune.DisplayName : "Rune chưa gán",
            _                     => "Không xác định"
        };

        public Sprite Icon => RewardType switch
        {
            LootRewardType.Item   => Item != null ? Item.Icon : null,
            _                     => null
        };
    }
}
