using System;
using RelicAndCoin.Inventory;

namespace RelicAndCoin.Shop
{
    /// <summary>
    /// Đại diện cho một ô trưng bày trên quầy bán hàng: chứa vật phẩm, giá niêm yết
    /// và xử lý kết quả khi khách hàng quyết định mua hoặc bỏ đi.
    /// Pure C# class — không phụ thuộc Unity lifecycle ngoài kiểu dữ liệu tham chiếu.
    /// </summary>
    public sealed class ShopDisplaySlot
    {
        public event Action<ShopDisplaySlot> OnItemBought;
        public event Action<ShopDisplaySlot> OnCustomerWalkedAway;

        public InventorySlotItem Item { get; private set; }
        public float ListedPrice { get; private set; }
        public bool IsOccupied => Item != null;

        public ShopDisplaySlot() { }

        /// <summary>Đặt một vật phẩm lên ô trưng bày với giá niêm yết.</summary>
        public void StockItem(InventorySlotItem item, float listedPrice)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            if (listedPrice < 0f) throw new ArgumentOutOfRangeException(nameof(listedPrice), "Giá niêm yết không được âm.");

            Item = item;
            ListedPrice = listedPrice;
        }

        /// <summary>Khách hàng quyết định mua vật phẩm: xoá ô và thông báo sự kiện.</summary>
        public bool OnCustomerBuy()
        {
            if (!IsOccupied) return false;

            var item = Item;
            Item = null;
            ListedPrice = 0f;
            OnItemBought?.Invoke(this);
            return true;
        }

        /// <summary>Khách hàng bỏ đi không mua: giữ nguyên vật phẩm, có thể đánh dấu ô đã bị từ chối.</summary>
        public void NotifyCustomerWalkedAway()
        {
            OnCustomerWalkedAway?.Invoke(this);
        }

        /// <summary>Dọn trống ô trưng bày (ví dụ khi thu hồi hàng về kho).</summary>
        public void Clear()
        {
            Item = null;
            ListedPrice = 0f;
        }
    }
}
