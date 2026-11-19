using System;
using System.Collections.Generic;
using UnityEngine;

namespace RelicAndCoin.Inventory
{
    /// <summary>
    /// Quản lý lưới túi đồ (mặc định 5x4 ô): kiểm tra hợp lệ, đặt/xóa vật phẩm,
    /// và xử lý hiệu ứng lời nguyền khi rời hầm ngục.
    /// Pure C# class — không phụ thuộc Unity lifecycle ngoài kiểu dữ liệu Vector2Int.
    /// </summary>
    public sealed class GridInventoryModel
    {
        public const int DefaultWidth = 5;
        public const int DefaultHeight = 4;

        private readonly int _width;
        private readonly int _height;
        // Ô (x, y) -> vật phẩm đang chiếm ô đó. null = ô trống.
        private readonly InventorySlotItem[,] _grid;
        private readonly List<InventorySlotItem> _items = new();

        public int Width => _width;
        public int Height => _height;
        public IReadOnlyList<InventorySlotItem> Items => _items;

        public GridInventoryModel(int width = DefaultWidth, int height = DefaultHeight)
        {
            _width = width;
            _height = height;
            _grid = new InventorySlotItem[width, height];
        }

        /// <summary>
        /// Kiểm tra xem vật phẩm có thể đặt tại vị trí góc trên bên trái hay không.
        /// </summary>
        public bool CanPlaceItem(ItemDataSO itemData, Vector2Int position, bool isRotated = false)
        {
            if (itemData == null) return false;

            var size = isRotated
                ? new Vector2Int(itemData.GridSize.y, itemData.GridSize.x)
                : itemData.GridSize;

            // Trong biên lưới?
            if (position.x < 0 || position.y < 0) return false;
            if (position.x + size.x > _width || position.y + size.y > _height) return false;

            // EdgeOnly chỉ cho phép đặt ở viền ba-lô.
            if (itemData.CurseType == ItemCurseType.EdgeOnly && !IsOnEdge(position, size))
                return false;

            // Tất cả ô đích phải trống.
            for (int y = position.y; y < position.y + size.y; y++)
                for (int x = position.x; x < position.x + size.x; x++)
                    if (_grid[x, y] != null) return false;

            return true;
        }

        /// <summary>Đặt vật phẩm tại vị trí nếu hợp lệ. Trả về true nếu thành công.</summary>
        public bool TryPlaceAt(ItemDataSO itemData, Vector2Int position, bool isRotated = false)
        {
            if (!CanPlaceItem(itemData, position, isRotated)) return false;

            var slot = new InventorySlotItem(itemData, 1, position, isRotated);
            var size = slot.EffectiveSize;
            for (int y = position.y; y < position.y + size.y; y++)
                for (int x = position.x; x < position.x + size.x; x++)
                    _grid[x, y] = slot;
            _items.Add(slot);
            return true;
        }

        /// <summary>
        /// Tìm ô trống đầu tiên mà vật phẩm vừa vào và đặt nó vào đó.
        /// Trả về true nếu thành công.
        /// </summary>
        public bool TryAddItem(ItemDataSO itemData, bool isRotated = false)
        {
            if (itemData == null) return false;

            var size = isRotated
                ? new Vector2Int(itemData.GridSize.y, itemData.GridSize.x)
                : itemData.GridSize;

            for (int y = 0; y <= _height - size.y; y++)
                for (int x = 0; x <= _width - size.x; x++)
                    if (CanPlaceItem(itemData, new Vector2Int(x, y), isRotated))
                        return TryPlaceAt(itemData, new Vector2Int(x, y), isRotated);

            return false;
        }

        /// <summary>Xóa vật phẩm khỏi lưới (trả về tất cả ô nó chiếm về trống).</summary>
        public bool RemoveItem(InventorySlotItem item)
        {
            if (item == null) return false;

            var size = item.EffectiveSize;
            bool removed = false;
            for (int y = item.GridPosition.y; y < item.GridPosition.y + size.y; y++)
                for (int x = item.GridPosition.x; x < item.GridPosition.x + size.x; x++)
                    if (_grid[x, y] == item)
                    {
                        _grid[x, y] = null;
                        removed = true;
                    }

            _items.Remove(item);
            return removed;
        }

        /// <summary>
        /// Xử lý hiệu ứng lời nguyền của mọi vật phẩm khi rời hầm ngục:
        /// DestroyAdjacent / SendHome -> xóa vật phẩm liền kề khỏi lưới;
        /// EdgeOnly -> không có hiệu ứng khi rời (chỉ ràng buộc khi đặt).
        /// </summary>
        public void ResolveDungeonExitCurses()
        {
            var snapshot = _items.ToArray();
            foreach (var item in snapshot)
            {
                if (item.ItemData == null) continue;

                switch (item.ItemData.CurseType)
                {
                    case ItemCurseType.DestroyAdjacent:
                        RemoveAdjacentItems(item);
                        break;
                    case ItemCurseType.SendHome:
                        SendAdjacentHome(item);
                        break;
                    case ItemCurseType.EdgeOnly:
                    case ItemCurseType.None:
                        // Không có hiệu ứng khi rời hầm ngục.
                        break;
                }
            }
        }

        private bool IsOnEdge(Vector2Int position, Vector2Int size) =>
            position.x == 0 || position.y == 0 ||
            position.x + size.x == _width || position.y + size.y == _height;

        private void RemoveAdjacentItems(InventorySlotItem item)
        {
            var size = item.EffectiveSize;
            var affected = new HashSet<InventorySlotItem>();

            for (int y = item.GridPosition.y - 1; y <= item.GridPosition.y + size.y; y++)
                for (int x = item.GridPosition.x - 1; x <= item.GridPosition.x + size.x; x++)
                    if (IsInBounds(x, y))
                    {
                        var neighbor = _grid[x, y];
                        if (neighbor != null && neighbor != item)
                            affected.Add(neighbor);
                    }

            foreach (var neighbor in affected)
                RemoveItem(neighbor);
        }

        private void SendAdjacentHome(InventorySlotItem item)
        {
            // Từ góc nhìn của mô hình túi đồ, vật phẩm bên cạnh bị rời khỏi lưới
            // (chuyển về rương làng được xử lý bởi hệ thống cửa hàng / base).
            RemoveAdjacentItems(item);
        }

        private bool IsInBounds(int x, int y) =>
            x >= 0 && x < _width && y >= 0 && y < _height;
    }
}
