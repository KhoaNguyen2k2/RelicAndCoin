using System;
using System.Collections.Generic;
using RelicAndCoin.Combat.Weapons;
using RelicAndCoin.Inventory;
using RelicAndCoin.Shop.Base;
using UnityEngine;

namespace RelicAndCoin.Shop.Crafting
{
    /// <summary>
    /// Một nguyên liệu đầu vào cho công thức chế tạo.
    /// </summary>
    [Serializable]
    public struct CraftMaterial
    {
        public ItemDataSO Item;
        public int Quantity;
    }

    /// <summary>
    /// ScriptableObject lưu công thức chế tạo: cơ sở yêu cầu, cấp độ cơ sở tối thiểu,
    /// danh sách nguyên liệu đầu vào, chi phí vàng, và vật phẩm kết quả
    /// (ItemDataSO hoặc WeaponDataSO).
    /// Pure data — không chứa business logic.
    /// </summary>
    [CreateAssetMenu(
        menuName = "RelicAndCoin/Shop/Crafting Recipe",
        fileName = "CraftingRecipe")]
    public sealed class CraftingRecipeSO : ScriptableObject
    {
        [Header("Yêu cầu cơ sở")]
        [Tooltip("Loại cơ sở cần có để chế tạo công thức này.")]
        public BaseFacilityType RequiredFacility;

        [Tooltip("Cấp độ tối thiểu của cơ sở yêu cầu.")]
        [Min(1)] public int MinFacilityLevel = 1;

        [Header("Nguyên liệu đầu vào")]
        [Tooltip("Danh sách nguyên liệu tiêu hao khi chế tạo.")]
        public List<CraftMaterial> InputMaterials = new();

        [Tooltip("Chi phí vàng để chế tạo.")]
        [Min(0)] public int GoldCost = 0;

        [Header("Kết quả")]
        [Tooltip("Vật phẩm kết quả (dùng khi CraftsWeapon = false).")]
        public ItemDataSO ResultItem;

        [Tooltip("Vũ khí kết quả (dùng khi CraftsWeapon = true).")]
        public WeaponDataSO ResultWeapon;

        [Tooltip("Bật nếu kết quả là vũ khí (ResultWeapon), tắt nếu là vật phẩm thông thường (ResultItem).")]
        public bool CraftsWeapon;
    }
}
