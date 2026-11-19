using System;
using System.Collections.Generic;
using RelicAndCoin.Inventory;
using RelicAndCoin.Shop.Base;

namespace RelicAndCoin.Shop.Crafting
{
    /// <summary>
    /// Kiểm tra điều kiện và tiêu hao nguyên liệu/vàng để chế tạo vật phẩm mới.
    /// Pure C# class — không phụ thuộc Unity lifecycle.
    /// </summary>
    public sealed class CraftingService
    {
        /// <summary>
        /// Kiểm tra xem có thể chế tạo công thức hay không.
        /// Trả về (thành công, lý do nếu không thể).
        /// </summary>
        public bool CanCraft(
            CraftingRecipeSO recipe,
            BaseUpgradeService baseUpgradeService,
            GridInventoryModel inventory,
            int currentGold,
            out string reason)
        {
            if (recipe == null)
            {
                reason = "Công thức chế tạo không được null.";
                return false;
            }
            if (baseUpgradeService == null)
            {
                reason = "Dịch vụ nâng cấp cơ sở không được null.";
                return false;
            }
            if (inventory == null)
            {
                reason = "Túi đồ không được null.";
                return false;
            }

            int facilityLevel = baseUpgradeService.GetLevel(recipe.RequiredFacility);
            if (facilityLevel < recipe.MinFacilityLevel)
            {
                reason = $"{recipe.RequiredFacility} cần đạt cấp {recipe.MinFacilityLevel}, hiện chỉ còn cấp {facilityLevel}.";
                return false;
            }

            if (currentGold < recipe.GoldCost)
            {
                reason = $"Không đủ vàng (cần {recipe.GoldCost}, có {currentGold}).";
                return false;
            }

            foreach (var mat in recipe.InputMaterials)
            {
                int owned = CountItemInInventory(inventory, mat.Item);
                if (owned < mat.Quantity)
                {
                    reason = $"Thiếu nguyên liệu {mat.Item.DisplayName}: có {owned}, cần {mat.Quantity}.";
                    return false;
                }
            }

            reason = string.Empty;
            return true;
        }

        /// <summary>
        /// Thực hiện chế tạo: tiêu hao nguyên liệu và vàng, trả về vật phẩm kết quả.
        /// Trả về true nếu thành công.
        /// </summary>
        public bool Craft(
            CraftingRecipeSO recipe,
            BaseUpgradeService baseUpgradeService,
            GridInventoryModel inventory,
            ref int currentGold,
            out InventorySlotItem resultItem)
        {
            resultItem = null;

            if (!CanCraft(recipe, baseUpgradeService, inventory, currentGold, out _)) return false;

            currentGold -= recipe.GoldCost;

            foreach (var mat in recipe.InputMaterials)
                ConsumeItemFromInventory(inventory, mat.Item, mat.Quantity);

            var resultData = recipe.CraftsWeapon ? null : recipe.ResultItem;
            if (resultData == null && !recipe.CraftsWeapon)
            {
                // Không có kết quả ItemData hợp lệ.
                return false;
            }

            resultItem = new InventorySlotItem(resultData, 1);
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
