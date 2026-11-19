using RelicAndCoin.Inventory;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RelicAndCoin.UI
{
    /// <summary>
    /// View lưới túi đồ: vẽ ô, icon, số lượng, viền màu cảnh báo theo lời nguyền;
    /// nhận thao tác đặt/xoay và giao tiếp với GridInventoryModel.
    /// MonoBehaviour mỏng — business logic ở GridInventoryModel.
    /// </summary>
    public sealed class GridInventoryView : MonoBehaviour
    {
        [Header("Prefabs & Container")]
        [SerializeField] private GameObject _slotPrefab;       // prefab ô: Image (icon) + Text (số lượng) + child "Border" (viền)
        [SerializeField] private RectTransform _gridContainer; // GridLayoutGroup chứa các ô

        [Header("Màu viền cảnh báo theo loại lời nguyền")]
        [SerializeField] private Color _curseNone         = Color.white;
        [SerializeField] private Color _curseDestroyAdjacent = Color.red;
        [SerializeField] private Color _curseSendHome      = Color.magenta;
        [SerializeField] private Color _curseEdgeOnly      = Color.yellow;

        private GridInventoryModel _model;
        private readonly Dictionary<InventorySlotItem, GameObject> _itemViews = new();

        /// <summary>Gắn model vào view và vẽ lại lưới.</summary>
        public void Bind(GridInventoryModel model)
        {
            _model = model;
            Refresh();
        }

        /// <summary>Vẽ lại toàn bộ lưới từ trạng thái hiện tại của model.</summary>
        public void Refresh()
        {
            if (_model == null || _gridContainer == null) return;

            ClearViews();
            foreach (var item in _model.Items)
                CreateItemView(item);
        }

        private void ClearViews()
        {
            var views = new List<GameObject>(_itemViews.Values);
            _itemViews.Clear();
            foreach (var go in views)
                if (go != null) Destroy(go);

            while (_gridContainer.childCount > 0)
                Destroy(_gridContainer.GetChild(0).gameObject);
        }

        private void CreateItemView(InventorySlotItem item)
        {
            if (_slotPrefab == null) return;

            var go = Instantiate(_slotPrefab, _gridContainer);
            var icon = go.GetComponentInChildren<Image>();
            var qtyText = go.GetComponentInChildren<Text>();

            if (item.ItemData != null && icon != null)
                icon.sprite = item.ItemData.Icon;

            if (qtyText != null)
                qtyText.text = item.Quantity > 1 ? item.Quantity.ToString() : string.Empty;

            ApplyCurseBorder(go, item.ItemData);
            _itemViews[item] = go;
        }

        private void ApplyCurseBorder(GameObject go, ItemDataSO data)
        {
            var border = FindBorder(go);
            if (border == null) return;

            Color color = data == null ? _curseNone : data.CurseType switch
            {
                ItemCurseType.DestroyAdjacent => _curseDestroyAdjacent,
                ItemCurseType.SendHome        => _curseSendHome,
                ItemCurseType.EdgeOnly        => _curseEdgeOnly,
                _                             => _curseNone
            };
            border.color = color;
            border.gameObject.SetActive(data != null && data.CurseType != ItemCurseType.None);
        }

        private static Image FindBorder(GameObject go)
        {
            if (go == null) return null;
            var borderTransform = go.transform.Find("Border");
            return borderTransform != null ? borderTransform.GetComponent<Image>() : null;
        }

        // ── Công khai để nút bấm / điều khiển input gọi ──
        public void OnPlaceItem(ItemDataSO itemData, Vector2Int position, bool isRotated)
        {
            if (_model == null) return;
            if (_model.TryPlaceAt(itemData, position, isRotated)) Refresh();
        }

        public void OnRotateItem(InventorySlotItem item)
        {
            if (_model == null || item == null) return;

            var pos = item.GridPosition;
            _model.RemoveItem(item);

            var rotated = new InventorySlotItem(item.ItemData, item.Quantity, pos, !item.IsRotated);
            if (!_model.TryPlaceAt(rotated.ItemData, pos, rotated.IsRotated))
            {
                // Không xoay được thì khôi phục trạng thái cũ.
                _model.TryPlaceAt(item.ItemData, pos, item.IsRotated);
            }
            Refresh();
        }
    }
}
