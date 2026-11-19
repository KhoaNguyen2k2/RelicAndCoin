using RelicAndCoin.Inventory;
using RelicAndCoin.Shop;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RelicAndCoin.UI
{
    /// <summary>
    /// View sổ tay giá cả: đọc từ PriceBookModel để hiển thị mức giá Fair cao nhất từng bán
    /// và mức độ phổ biếnItemPopularity của từng món hàng.
    /// MonoBehaviour mỏng — business logic ở PriceBookModel.
    /// </summary>
    public sealed class PriceBookView : MonoBehaviour
    {
        [Header("Hàng và container")]
        [SerializeField] private GameObject _rowPrefab;       // prefab hàng: icon, tên, mức giá Fair, mức phổ biến
        [SerializeField] private RectTransform _rowsContainer;

        private PriceBookModel _model;
        private readonly List<ItemDataSO> _items = new();
        private readonly List<GameObject> _rows = new();

        /// <summary>Gắn model và danh sách món hàng cần hiển thị.</summary>
        public void Bind(PriceBookModel model, IEnumerable<ItemDataSO> items)
        {
            _model = model;
            _items.Clear();
            foreach (var item in items)
                if (item != null) _items.Add(item);
            Refresh();
        }

        /// <summary>Cập nhật lại danh sách món hàng và vẽ lại.</summary>
        public void SetItems(IEnumerable<ItemDataSO> items)
        {
            _items.Clear();
            foreach (var item in items)
                if (item != null) _items.Add(item);
            Refresh();
        }

        /// <summary>Vẽ lại toàn bộ hàng từ dữ liệu hiện tại của model.</summary>
        public void Refresh()
        {
            if (_model == null || _rowsContainer == null) return;

            ClearRows();
            foreach (var item in _items)
                CreateRow(item);
        }

        private void ClearRows()
        {
            foreach (var go in _rows)
                if (go != null) Destroy(go);
            _rows.Clear();
            while (_rowsContainer.childCount > 0)
                Destroy(_rowsContainer.GetChild(0).gameObject);
        }

        private void CreateRow(ItemDataSO item)
        {
            if (_rowPrefab == null) return;

            var go = Instantiate(_rowPrefab, _rowsContainer);
            var icon = go.GetComponentInChildren<Image>();
            var nameText = FindText(go, "Name");
            var priceText = FindText(go, "Price");
            var popularityText = FindText(go, "Popularity");

            if (icon != null) icon.sprite = item.Icon;
            if (nameText != null) nameText.text = item.DisplayName;

            float highestFair = _model.GetHighestFairSoldPrice(item.Id);
            if (priceText != null)
                priceText.text = highestFair > 0f ? highestFair.ToString("F0") + " vàng" : "Chưa bán";

            var popularity = _model.GetPopularity(item.Id);
            if (popularityText != null)
                popularityText.text = popularity switch
                {
                    ItemPopularity.LowDemand  => "Thấp",
                    ItemPopularity.Normal     => "Bình thường",
                    ItemPopularity.HighDemand => "Cao",
                    _                         => "Bình thường"
                };

            _rows.Add(go);
        }

        private static Text FindText(GameObject go, string childName)
        {
            if (go == null) return null;
            var child = go.transform.Find(childName);
            return child != null ? child.GetComponent<Text>() : null;
        }
    }
}
