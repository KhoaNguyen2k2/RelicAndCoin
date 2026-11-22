using System;
using System.Collections.Generic;
using RelicAndCoin.Core.Difficulty;
using UnityEngine;

namespace RelicAndCoin.Combat.Loot
{
    /// <summary>
    /// MonoBehaviour mỏng gắn lên Hòm (Crate) / Rương thường (NormalChest) / Rương kho báu (TreasureChest).
    /// Nhận tương tác mở rương, gọi LootDropService để sinh phần thưởng và ngăn mở lại lần hai.
    /// </summary>
    [SelectionBase]
    public sealed class ChestController : MonoBehaviour
    {
        [Header("Cấu hình Rương")]
        [SerializeField] private LootSourceType _chestType = LootSourceType.NormalChest;
        [SerializeField] private LootTableSO _lootTable;
        [SerializeField] private LootDropConfigSO _lootConfig;
        [SerializeField] private DifficultyConfigSO _difficulty;

        [Header("Khoảng cách tương tác")]
        [SerializeField] private float _interactRange = 2.0f;

        [Header("Hình ảnh & Phản hồi trực quan")]
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Sprite _openedSprite;
        [SerializeField] private Color _openedColor = new Color(0.7f, 0.7f, 0.7f, 1f);

        private bool _isOpened = false;
        private Transform _playerTransform;

        public bool IsOpened => _isOpened;
        public LootSourceType ChestType => _chestType;
        public float InteractRange => _interactRange;
        public IReadOnlyList<LootEntry> DroppedLoot { get; private set; }

        public event Action<IReadOnlyList<LootEntry>> OnChestOpened;

        private void Awake()
        {
            if (_spriteRenderer == null)
                _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        private void Start()
        {
            var playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
                _playerTransform = playerObj.transform;
        }

        private void Update()
        {
            if (_isOpened) return;

            if (_playerTransform == null)
            {
                var playerObj = GameObject.FindWithTag("Player");
                if (playerObj != null)
                    _playerTransform = playerObj.transform;
                return;
            }

            float distance = Vector2.Distance(transform.position, _playerTransform.position);
            if (distance <= _interactRange && Input.GetKeyDown(KeyCode.E))
            {
                OpenChest();
            }
        }

        /// <summary>
        /// Thực hiện mở rương / đập vỡ hòm. Trả về true nếu mở thành công, false nếu rương đã được mở trước đó.
        /// </summary>
        public bool OpenChest()
        {
            if (_isOpened)
                return false;

            _isOpened = true;

            var mode = _difficulty != null ? _difficulty.Mode : DifficultyMode.Normal;
            var drops = LootDropService.CalculateLoot(_chestType, mode, _lootTable, _lootConfig);

            // Đảm bảo sinh ít nhất 1-2 vật phẩm từ bảng nếu kết quả rỗng
            if ((drops == null || drops.Count == 0) && _lootTable != null && _lootTable.Entries.Count > 0)
            {
                drops = new List<LootEntry>();
                int count = Mathf.Min(2, _lootTable.Entries.Count);
                for (int i = 0; i < count; i++)
                    drops.Add(_lootTable.Entries[i]);
            }

            DroppedLoot = drops;

            // Đổi hình ảnh hoặc màu sắc sang trạng thái đã mở
            if (_spriteRenderer != null)
            {
                if (_openedSprite != null)
                    _spriteRenderer.sprite = _openedSprite;
                else
                    _spriteRenderer.color = _openedColor;
            }

            // Sinh các đối tượng DroppedItemPickup văng nhẹ ra cạnh rương
            if (drops != null && drops.Count > 0)
            {
                for (int i = 0; i < drops.Count; i++)
                {
                    float angle = (i * (360f / Mathf.Max(1, drops.Count)) + UnityEngine.Random.Range(-25f, 25f)) * Mathf.Deg2Rad;
                    Vector2 popDir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * UnityEngine.Random.Range(0.9f, 1.3f);
                    DroppedItemPickup.Create(transform.position, drops[i], popDir);
                }
            }

            OnChestOpened?.Invoke(drops);

            // Ghi nhận phần thưởng ra log phục vụ kiểm tra
            string lootNames = drops != null && drops.Count > 0
                ? string.Join(", ", drops.ConvertAll(d => $"[{d.Rarity}] {d.DisplayName}"))
                : "Không rơi vật phẩm nào";
            Debug.Log($"<color=orange>[RƯƠNG]</color> [{gameObject.name} - {_chestType}] Đã mở thành công! Sinh ({drops?.Count ?? 0}) vật phẩm rơi: {lootNames}");

            return true;
        }

        /// <summary>
        /// Điểm vào tương tác (có thể gọi từ PlayerInteractionSystem).
        /// </summary>
        public void Interact() => OpenChest();

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, _interactRange);
        }
    }
}
