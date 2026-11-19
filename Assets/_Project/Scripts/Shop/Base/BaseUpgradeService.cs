using System;
using System.Collections.Generic;
using RelicAndCoin.Inventory;

namespace RelicAndCoin.Shop.Base
{
    /// <summary>
    /// Quản lý cấp độ hiện tại của 3 cơ sở (Cửa hàng, Lò rèn, Giả kim),
    /// kiểm tra điều kiện đủ vàng và nguyên liệu trong GridInventoryModel
    /// để thực hiện nâng cấp.
    /// Pure C# class — không phụ thuộc Unity lifecycle.
    /// </summary>
    public sealed class BaseUpgradeService
    {
        // Cấp độ hiện tại của mỗi cơ sở, bắt đầu từ 1.
        private readonly Dictionary<BaseFacilityType, int> _levels = new();

        public BaseUpgradeService()
        {
            foreach (BaseFacilityType type in (BaseFacilityType[])Enum.GetValues(typeof(BaseFacilityType)))
                _levels[type] = 1;
        }

        /// <summary>Trả về cấp độ hiện tại của một cơ sở.</summary>
        public int GetLevel(BaseFacilityType type) =>
            _levels.TryGetValue(type, out var level) ? level : 1;

        /// <summary>
        /// Kiểm tra xem có thể nâng cấp cơ sở hay không.
        /// Trả về (thành công, lý do nếu không thể).
        /// </summary>
        public bool CanUpgrade(
            BaseFacilityType type,
            FacilityUpgradeDataSO data,
            int currentGold,
            GridInventoryModel inventory,
            out string reason)
        {
            if (data == null)
            {
                reason = "Dữ liệu nâng cấp không được null.";
                return false;
            }
            if (data.FacilityType != type)
            {
                reason = "Loại cơ sở của dữ liệu nâng cấp không khớp.";
                return false;
            }

            int currentLevel = GetLevel(type);
            if (data.TargetLevel <= currentLevel)
            {
                reason = $"Cơ sở {type} đã ở cấp {currentLevel}, không thể nâng cấp xuống/{{giữ nguyên}} cấp {data.TargetLevel}.";
                return false;
            }

            if (currentGold < data.GoldCost)
            {
                reason = $"Không đủ vàng (cần {data.GoldCost}, có {currentGold}).";
                return false;
            }

            if (inventory == null)
            {
                reason = "Túi đồ không được null.";
                return false;
            }

            // Kiểm tra từng nguyên liệu yêu cầu có đủ số lượng trong túi đồ hay không.
            foreach (var req in data.RequiredMaterials)
            {
                int owned = CountItemInInventory(inventory, req.Item);
                if (owned < req.Quantity)
                {
                    reason = $"Thiếu nguyên liệu {req.Item.DisplayName}: có {owned}, cần {req.Quantity}.";
                    return false;
                }
            }

            reason = string.Empty;
            return true;
        }

        /// <summary>
        /// Thực hiện nâng cấp: trừ vàng, tiêu hao nguyên liệu từ túi đồ, tăng cấp độ.
        /// Trả về true nếu thành công.
        /// </summary>
        public bool TryUpgrade(
            BaseFacilityType type,
            FacilityUpgradeDataSO data,
            ref int currentGold,
            GridInventoryModel inventory)
        {
            if (!CanUpgrade(type, data, currentGold, inventory, out _)) return false;

            currentGold -= data.GoldCost;

            foreach (var req in data.RequiredMaterials)
                ConsumeItemFromInventory(inventory, req.Item, req.Quantity);

            _levels[type] = data.TargetLevel;
            return true;
        }

        private static int CountItemInInventory(GridInventoryModel inventory, ItemDataSO item)
        {
            if (item == null) return 0;
            int total = 0;
            foreach (var slot in inventory.Items)
                if (slot.ItemData == item) total += slot.Quantity;
            return total;
        }

        private static void ConsumeItemFromInventory(GridInventoryModel inventory, ItemDataSO item, int quantity)
        {
            // Tiêu hao từ các slot chứa item này, xóa slot khi hết số lượng.
            var slotsToConsume = new List<InventorySlotItem>();
            foreach (var slot in inventory.Items)
                if (slot.ItemData == item) slotsToConsume.Add(slot);

            int remaining = quantity;
            foreach (var slot in slotsToConsume)
            {
                if (remaining <= 0) break;
                int take = Math.Min(slot.Quantity, remaining);
                slot.Quantity -= take;
                remaining -= take;
                if (slot.Quantity <= 0)
                    inventory.RemoveItem(slot);
            }
        }
    }
}
