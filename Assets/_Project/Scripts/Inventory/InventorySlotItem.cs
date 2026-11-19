using UnityEngine;

namespace RelicAndCoin.Inventory
{
    /// <summary>
    /// Đại diện cho một vật phẩm đang nằm trong túi đồ: tham chiếu dữ liệu,
    /// số lượng, tọa độ góc trên bên trái và trạng thái xoay.
    /// Pure C# class — không chứa business logic hay phụ thuộc Unity lifecycle.
    /// </summary>
    [System.Serializable]
    public sealed class InventorySlotItem
    {
        public ItemDataSO ItemData { get; }
        public int Quantity { get; set; }
        public Vector2Int GridPosition { get; set; }
        public bool IsRotated { get; set; }

        public InventorySlotItem(
            ItemDataSO itemData,
            int quantity = 1,
            Vector2Int gridPosition = default,
            bool isRotated = false)
        {
            ItemData = itemData;
            Quantity = quantity;
            GridPosition = gridPosition;
            IsRotated = isRotated;
        }

        /// <summary>
        /// Kích thước thực tế chiếm trên lưới, tính cả việc xoay
        /// (xoay sẽ hoán đổi chiều rộng và chiều cao).
        /// </summary>
        public Vector2Int EffectiveSize =>
            IsRotated ? new Vector2Int(ItemData.GridSize.y, ItemData.GridSize.x) : ItemData.GridSize;
    }
}
