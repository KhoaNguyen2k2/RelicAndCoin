using RelicAndCoin.AI.Shop;
using RelicAndCoin.Core.Difficulty;
using RelicAndCoin.Inventory;
using RelicAndCoin.UI;
using UnityEngine;

namespace RelicAndCoin.Shop
{
    /// <summary>
    /// Bootstrapper khởi tạo hệ thống Cửa hàng và Túi đồ (Shop & Inventory):
    /// - Khởi tạo GridInventoryModel (5x4 ô), PriceBookModel, DynamicEconomyService và ShopDisplaySlot.
    /// - Nạp sẵn 1 vật phẩm mẫu lên ô trưng bày và ba-lô, liên kết với ShopCounterView và GridInventoryView.
    /// - Kích hoạt CustomerController đi tới quầy để kiểm tra giá và thực hiện mua bán ngay khi bắt đầu.
    /// Gắn lên GameObject "GameSystems" trong ShopScene.
    /// </summary>
    public sealed class ShopAndInventoryBootstrapper : MonoBehaviour
    {
        [Header("Dữ liệu & Cấu hình")]
        [SerializeField] private DifficultyConfigSO _difficultyConfig;
        [SerializeField] private ItemDataSO _sampleItem;
        [SerializeField] private float _sampleListedPrice = 25f;
        [SerializeField] private int _customerBudget = 100;

        [Header("Tham chiếu Views")]
        [SerializeField] private ShopCounterView _counterView;
        [SerializeField] private GridInventoryView _inventoryView;

        [Header("Khách hàng")]
        [SerializeField] private CustomerController _customerController;
        [SerializeField] private Vector3 _fallbackCounterPosition = Vector3.zero;

        // Pure C# models & services
        private GridInventoryModel _inventoryModel;
        private PriceBookModel _priceBook;
        private DynamicEconomyService _economyService;
        private ShopDisplaySlot _displaySlot;

        public GridInventoryModel InventoryModel => _inventoryModel;
        public PriceBookModel PriceBook => _priceBook;
        public DynamicEconomyService EconomyService => _economyService;
        public ShopDisplaySlot DisplaySlot => _displaySlot;

        private void Awake()
        {
            InitializeCoreModels();
            SetupSampleItemAndStock();
            BindViews();
        }

        private void Start()
        {
            ActivateCustomer();
        }

        private void OnDestroy()
        {
            if (_customerController != null)
            {
                _customerController.OnTransactionCompleted -= HandleTransactionCompleted;
            }
        }

        private void InitializeCoreModels()
        {
            // 1. Khởi tạo túi đồ dạng lưới 5x4
            _inventoryModel = new GridInventoryModel(
                GridInventoryModel.DefaultWidth,
                GridInventoryModel.DefaultHeight);

            // 2. Sổ tay thương nhân lưu lịch sử định giá
            _priceBook = new PriceBookModel();

            // 3. Cấu hình độ khó kinh tế
            if (_difficultyConfig == null)
            {
                _difficultyConfig = ScriptableObject.CreateInstance<DifficultyConfigSO>();
            }
            _economyService = new DynamicEconomyService(_difficultyConfig);

            // 4. Ô trưng bày tại quầy
            _displaySlot = new ShopDisplaySlot();
        }

        private void SetupSampleItemAndStock()
        {
            if (_sampleItem == null)
            {
                Debug.LogWarning("[ShopAndInventoryBootstrapper] Chưa gán _sampleItem trong Inspector. Quầy hàng sẽ trống.");
                return;
            }

            // Nạp vật phẩm vào túi đồ
            _inventoryModel.TryAddItem(_sampleItem);

            // Tạo ô vật phẩm trưng bày và đặt lên quầy
            float initialPrice = _sampleListedPrice > 0f ? _sampleListedPrice : _sampleItem.BaseValue;
            var displayItem = new InventorySlotItem(_sampleItem, 1, Vector2Int.zero, false);
            _displaySlot.StockItem(displayItem, initialPrice);

            Debug.Log($"[ShopAndInventoryBootstrapper] Đã nạp mẫu: '{_sampleItem.DisplayName}' (Base: {_sampleItem.BaseValue}) với giá niêm yết: {initialPrice} vàng.");
        }

        private void BindViews()
        {
            // Liên kết quầy hàng UI nếu có
            if (_counterView != null)
            {
                _counterView.Bind(_displaySlot, _economyService, ItemPopularity.Normal);
            }

            // Liên kết lưới túi đồ UI nếu có
            if (_inventoryView != null)
            {
                _inventoryView.Bind(_inventoryModel);
            }
        }

        private void ActivateCustomer()
        {
            if (_customerController == null)
            {
                Debug.LogWarning("[ShopAndInventoryBootstrapper] Chưa gán _customerController trong Inspector. Không kích hoạt khách hàng.");
                return;
            }

            // Xác định vị trí quầy trưng bày: ưu tiên vị trí CounterView, fallback nếu không có
            Vector3 counterPos = _counterView != null
                ? _counterView.transform.position
                : _fallbackCounterPosition;

            // Khởi tạo CustomerAIModel thuần C#
            var customerAI = new CustomerAIModel(_economyService, _customerBudget);

            // Đăng ký sự kiện giao dịch chi tiết từ CustomerController
            _customerController.OnTransactionCompleted -= HandleTransactionCompleted;
            _customerController.OnTransactionCompleted += HandleTransactionCompleted;

            // Nạp dependencies cho CustomerController
            _customerController.Initialize(
                customerAI,
                _displaySlot,
                _priceBook,
                counterPos);

            Debug.Log($"[ShopAndInventoryBootstrapper] Kích hoạt khách hàng '{_customerController.name}' di chuyển tới quầy tại tọa độ {counterPos} (Ngân sách: {_customerBudget} vàng).");
        }

        private void HandleItemBought(ShopDisplaySlot slot)
        {
            Debug.Log($"<color=green>[GIAO DỊCH THÀNH CÔNG]</color> Khách hàng đã mua vật phẩm tại quầy với giá niêm yết!");
        }

        private void HandleCustomerWalkedAway(ShopDisplaySlot slot)
        {
            Debug.Log($"<color=red>[GIAO DỊCH THẤT BẠI]</color> Khách hàng chê giá quá đắt hoặc không đủ tiền và đã bỏ đi!");
        }

        private void HandleTransactionCompleted(bool success, PriceReaction reaction, ItemDataSO item, float price)
        {
            string reactionStr = reaction switch
            {
                PriceReaction.Cheap      => "Rẻ (Cheap)",
                PriceReaction.Fair       => "Hợp lý (Fair)",
                PriceReaction.Expensive  => "Đắt (Expensive)",
                PriceReaction.Overpriced => "Quá đắt (Overpriced)",
                _                        => reaction.ToString()
            };

            if (success)
            {
                Debug.Log($"[ShopBootstrapper] Kết quả: KHÁCH ĐÃ MUA '{item.DisplayName}'! Giá niêm yết: {price} vàng | Đánh giá: {reactionStr}.");
            }
            else
            {
                Debug.LogWarning($"[ShopBootstrapper] Kết quả: KHÁCH TỪ CHỐI MUA '{item.DisplayName}'! Giá niêm yết: {price} vàng | Đánh giá: {reactionStr}.");
            }
        }
    }
}
