using System;
using RelicAndCoin.AI.Enemies;
using RelicAndCoin.Inventory;
using RelicAndCoin.Shop;
using UnityEngine;

namespace RelicAndCoin.AI.Shop
{
    /// <summary>
    /// MonoBehaviour mỏng gắn lên GameObject khách hàng: di chuyển tới vị trí quầy trưng bày,
    /// giữ CustomerAIModel làm biến thành viên để kiểm tra giá và thực hiện giao dịch qua ShopDisplaySlot,
    /// đồng thời cập nhật kết quả vào PriceBookModel.
    /// </summary>
    public sealed class CustomerController : MonoBehaviour
    {
        public event Action<bool, PriceReaction, ItemDataSO, float> OnTransactionCompleted;

        [Header("Cấu hình di chuyển & suy nghĩ")]
        [SerializeField] private float _moveSpeed = 2f;
        [SerializeField] private float _thinkDuration = 1.5f;
        [SerializeField] private float _arrivalDistance = 0.1f;

        [Header("Phản ứng với quái vật")]
        [SerializeField] private float _monsterFearRadius = 4f;
        [SerializeField] private float _fleeSpeed = 3.5f;

        private ShopDisplaySlot _targetSlot;
        private PriceBookModel _priceBook;
        private Vector3 _slotWorldPosition;
        private CustomerAIModel _ai;

        // Bộ đệm cố định để quét va chạm quái vật trong bán kính nguy hiểm mà không tạo rác GC.
        private readonly Collider2D[] _overlapBuffer = new Collider2D[8];
        private float _lastFleeLogTime = -10f;

        public ShopDisplaySlot TargetSlot => _targetSlot;
        public PriceBookModel PriceBook => _priceBook;
        public CustomerAIModel AI => _ai;
        public Vector3 SlotWorldPosition => _slotWorldPosition;

        private void Awake()
        {
            ConfigurePhysics();
        }

        private void ConfigurePhysics()
        {
            var rb = GetComponent<Rigidbody2D>();
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody2D>();
            }

            // Rigidbody Kinematic + full contact giúp khách hàng / dân thường hoạt động như bức tường:
            // hoàn toàn không bị đẩy trôi bởi Player hay quái vật va chạm vào.
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.useFullKinematicContacts = true;
            rb.freezeRotation = true;

            var col = GetComponent<Collider2D>();
            if (col == null)
            {
                var circleCol = gameObject.AddComponent<CircleCollider2D>();
                circleCol.radius = 0.5f;
                col = circleCol;
            }

            // Collider không phải Trigger để va chạm vật lý cứng chắn người chơi và quái vật.
            col.isTrigger = false;
        }

        /// <summary>
        /// Khởi tạo dữ liệu và dependencies cho khách hàng từ bootstrapper hoặc spawner.
        /// </summary>
        public void Initialize(
            CustomerAIModel ai,
            ShopDisplaySlot targetSlot,
            PriceBookModel priceBook,
            Vector3 slotWorldPosition)
        {
            _ai = ai;
            _targetSlot = targetSlot;
            _priceBook = priceBook;
            _slotWorldPosition = slotWorldPosition;
        }

        public void SetTarget(ShopDisplaySlot slot, Vector3 slotWorldPosition)
        {
            _targetSlot = slot;
            _slotWorldPosition = slotWorldPosition;
        }

        public void SetAI(CustomerAIModel ai) => _ai = ai;
        public void SetPriceBook(PriceBookModel priceBook) => _priceBook = priceBook;
        public void SetSlotPosition(Vector3 slotWorldPosition) => _slotWorldPosition = slotWorldPosition;

        private void Update()
        {
            if (_ai == null) return;

            // 1. Quét tìm quái vật trong bán kính nguy hiểm
            if (CheckAndHandleThreats())
            {
                return;
            }

            if (_ai.IsThinking)
            {
                if (_ai.TickThinkTime(Time.deltaTime))
                {
                    HandleThinkFinished();
                }
                return;
            }

            if (_ai.State is CustomerState.Entering or CustomerState.Browsing)
            {
                if (_targetSlot != null && _targetSlot.IsOccupied)
                {
                    MoveToSlot(_slotWorldPosition);

                    if (Vector2.Distance(transform.position, _slotWorldPosition) <= _arrivalDistance)
                    {
                        BeginInspecting();
                    }
                }
            }
        }

        /// <summary>Khách hàng bắt đầu xem món hàng tại quầy và suy nghĩ trước khi mua.</summary>
        public void BeginInspecting()
        {
            if (_ai == null) return;
            _ai.SetState(CustomerState.InspectingItem);
            _ai.StartThinking(_thinkDuration);
        }

        /// <summary>Di chuyển mượt mà tới vị trí quầy trưng bày.</summary>
        public void MoveToSlot(Vector3 slotWorldPosition)
        {
            transform.position = Vector2.MoveTowards(
                transform.position,
                slotWorldPosition,
                _moveSpeed * Time.deltaTime);
        }

        /// <summary>
        /// Quét tìm quái vật xung quanh bằng Unity 6 API: nếu có, chuyển sang trạng thái Fleeing và chạy ra xa.
        /// Trả về true nếu phát hiện quái vật và đang hoảng loạn bỏ chạy.
        /// </summary>
        private bool CheckAndHandleThreats()
        {
            // Unity 6: Dùng OverlapCircle với ContactFilter2D.noFilter để không bị lỗi obsolete và không cấp phát GC
            int count = Physics2D.OverlapCircle(transform.position, _monsterFearRadius, ContactFilter2D.noFilter, _overlapBuffer);

            Transform nearestThreat = null;
            float nearestDistSqr = float.MaxValue;

            for (int i = 0; i < count; i++)
            {
                var col = _overlapBuffer[i];
                if (col == null) continue;

                var enemy = col.GetComponent<EnemyController>();
                if (enemy == null || !enemy.IsAlive) continue;

                float distSqr = (transform.position - enemy.transform.position).sqrMagnitude;
                if (distSqr < nearestDistSqr)
                {
                    nearestDistSqr = distSqr;
                    nearestThreat = enemy.transform;
                }
            }

            if (nearestThreat != null)
            {
                if (!_ai.IsFleeing)
                {
                    _ai.SetFleeing();
                }

                if (Time.time - _lastFleeLogTime >= 2f)
                {
                    _lastFleeLogTime = Time.time;
                    Debug.LogWarning($"[CustomerController] {name}: Cứu tôi với! Có quái vật gần kề! Đang bỏ chạy!");
                }

                Vector2 fleeDirection = ((Vector2)transform.position - (Vector2)nearestThreat.position).normalized;
                transform.position += (Vector3)(fleeDirection * _fleeSpeed * Time.deltaTime);
                return true;
            }

            return false;
        }

        private void HandleThinkFinished()
        {
            if (_targetSlot == null || !_targetSlot.IsOccupied)
            {
                _ai.SetAngry();
                return;
            }

            var item = _targetSlot.Item;
            if (item == null || item.ItemData == null)
            {
                _ai.SetAngry();
                return;
            }

            float listedPrice = _targetSlot.ListedPrice;
            var (willBuy, reaction) = _ai.DecidePurchase(
                item.ItemData,
                listedPrice,
                ItemPopularity.Normal);

            _priceBook?.RecordPriceAttempt(item.ItemData.Id, listedPrice, reaction);

            if (willBuy)
            {
                _targetSlot.OnCustomerBuy();
                _ai.SetHappy();
            }
            else
            {
                _targetSlot.NotifyCustomerWalkedAway();
                _ai.SetAngry();
            }

            OnTransactionCompleted?.Invoke(willBuy, reaction, item.ItemData, listedPrice);
        }
    }
}
