using RelicAndCoin.Inventory;
using RelicAndCoin.Shop;
using UnityEngine;
using UnityEngine.UI;

namespace RelicAndCoin.UI
{
    /// <summary>
    /// View quầy hàng: cho phép người chơi điều chỉnh giá niêm yết ListedPrice
    /// và hiển thị biểu tượng cảm xúc PriceReaction khi khách hàng xem hàng.
    /// MonoBehaviour mỏng — business logic ở ShopDisplaySlot + DynamicEconomyService.
    /// </summary>
    public sealed class ShopCounterView : MonoBehaviour
    {
        [Header("Thao tác giá")]
        [SerializeField] private InputField _priceInput;
        [SerializeField] private Button _decreaseButton;
        [SerializeField] private Button _increaseButton;
        [SerializeField] private Text _priceText;

        [Header("Biểu tượng cảm xúc theo phản ứng giá")]
        [SerializeField] private Image _reactionIcon;
        [SerializeField] private Text _reactionText;
        [SerializeField] private Sprite _iconCheap;
        [SerializeField] private Sprite _iconFair;
        [SerializeField] private Sprite _iconExpensive;
        [SerializeField] private Sprite _iconOverpriced;

        private ShopDisplaySlot _slot;
        private DynamicEconomyService _economy;
        private ItemPopularity _popularity = ItemPopularity.Normal;
        private float _listedPrice;

        /// <summary>Gắn ô trưng bày, dịch vụ giá và mức phổ biến của món hàng.</summary>
        public void Bind(ShopDisplaySlot slot, DynamicEconomyService economy, ItemPopularity popularity)
        {
            _slot = slot;
            _economy = economy;
            _popularity = popularity;
            if (_slot != null) _listedPrice = _slot.ListedPrice;
            Refresh();
        }

        public void OnDecreasePrice() => AdjustPrice(-1f);
        public void OnIncreasePrice() => AdjustPrice(+1f);

        public void OnPriceEdited(string text)
        {
            if (float.TryParse(text, out var price)) _listedPrice = Mathf.Max(0f, price);
            Refresh();
        }

        private void AdjustPrice(float delta)
        {
            if (_slot == null || !_slot.IsOccupied) return;
            _listedPrice = Mathf.Max(0f, _listedPrice + delta);
            // ShopDisplaySlot.ListedPrice chỉ có private set → cập nhật bằng cách nộp lại hàng.
            _slot.StockItem(_slot.Item, _listedPrice);
            Refresh();
        }

        private void Refresh()
        {
            if (_slot == null) return;

            if (_priceText != null) _priceText.text = _listedPrice.ToString("F0");
            if (_priceInput != null) _priceInput.text = _listedPrice.ToString("F0");

            bool showReaction = _slot.IsOccupied && _economy != null && _slot.Item != null;
            if (_reactionIcon != null) _reactionIcon.gameObject.SetActive(showReaction);
            if (_reactionText != null) _reactionText.gameObject.SetActive(showReaction);

            if (!showReaction) return;

            var reaction = _economy.EvaluatePrice(_slot.Item.ItemData, _listedPrice, _popularity);
            SetReaction(reaction);
        }

        private void SetReaction(PriceReaction reaction)
        {
            if (_reactionIcon != null)
            {
                _reactionIcon.sprite = reaction switch
                {
                    PriceReaction.Cheap      => _iconCheap,
                    PriceReaction.Fair       => _iconFair,
                    PriceReaction.Expensive  => _iconExpensive,
                    PriceReaction.Overpriced => _iconOverpriced,
                    _                        => _iconFair
                };
            }

            if (_reactionText != null)
            {
                _reactionText.text = reaction switch
                {
                    PriceReaction.Cheap      => "Rẻ",
                    PriceReaction.Fair       => "Ổn",
                    PriceReaction.Expensive  => "Đắt",
                    PriceReaction.Overpriced => "Quá đắt",
                    _                        => string.Empty
                };
            }
        }
    }
}
