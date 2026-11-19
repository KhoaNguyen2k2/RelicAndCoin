using System;
using System.Collections.Generic;
using RelicAndCoin.Inventory;
using UnityEngine;

namespace RelicAndCoin.Shop.Base
{
    /// <summary>
    /// Loại cơ sở có thể nâng cấp trong hệ thống Base (Cửa hàng, Lò rèn, Giả kim).
    /// </summary>
    public enum BaseFacilityType
    {
        Shop,
        Blacksmith,
        Alchemy
    }

    /// <summary>
    /// Một yêu cầu nguyên liệu cho việc nâng cấp hoặc chế tạo.
    /// </summary>
    [Serializable]
    public struct MaterialRequirement
    {
        public ItemDataSO Item;
        public int Quantity;
    }

    /// <summary>
    /// ScriptableObject lưu dữ liệu từng cấp nâng cấp của một cơ sở:
    /// loại cơ sở, cấp độ mục tiêu, chi phí vàng, danh sách nguyên liệu yêu cầu,
    /// và mô tả hiệu ứng mở khóa.
    /// Pure data — không chứa business logic.
    /// </summary>
    [CreateAssetMenu(
        menuName = "RelicAndCoin/Shop/Facility Upgrade Data",
        fileName = "FacilityUpgradeData")]
    public sealed class FacilityUpgradeDataSO : ScriptableObject
    {
        [Header("Cơ sở")]
        [Tooltip("Loại cơ sở mà bản nâng cấp này thuộc về.")]
        public BaseFacilityType FacilityType;

        [Tooltip("Cấp độ mục tiêu sau khi nâng cấp (ví dụ: 1 → 2).")]
        [Min(1)] public int TargetLevel = 1;

        [Header("Chi phí")]
        [Tooltip("Chi phí vàng để nâng cấp lên cấp này.")]
        [Min(0)] public int GoldCost = 0;

        [Tooltip("Danh sách nguyên liệu yêu cầu trong túi đồ để thực hiện nâng cấp.")]
        public List<MaterialRequirement> RequiredMaterials = new();

        [Header("Hiệu ứng")]
        [Tooltip("Mô tả hiệu ứng mở khóa sau nâng cấp (tăng số quầy hàng, mở công thức bậc cao, ...).")]
        [TextArea(2, 4)]
        public string UnlockDescription = string.Empty;
    }
}
