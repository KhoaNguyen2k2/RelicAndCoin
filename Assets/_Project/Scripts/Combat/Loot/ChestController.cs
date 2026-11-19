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

        [Header("Hình ảnh & Phản hồi trực quan")]
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Sprite _openedSprite;

        private bool _isOpened = false;

        public bool IsOpened => _isOpened;
        public LootSourceType ChestType => _chestType;
        public IReadOnlyList<LootEntry> DroppedLoot { get; private set; }

        public event Action<IReadOnlyList<LootEntry>> OnChestOpened;

        private void Awake()
        {
            if (_spriteRenderer == null)
                _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
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
            DroppedLoot = drops;

            // Đổi hình ảnh sang trạng thái đã mở
            if (_spriteRenderer != null && _openedSprite != null)
            {
                _spriteRenderer.sprite = _openedSprite;
            }

            OnChestOpened?.Invoke(drops);

            // Ghi nhận phần thưởng ra log phục vụ kiểm tra
            string lootNames = drops.Count > 0
                ? string.Join(", ", drops.ConvertAll(d => $"[{d.Rarity}] {d.DisplayName}"))
                : "Không rơi vật phẩm nào";
            Debug.Log($"[{gameObject.name} - {_chestType}] Đã mở thành công! Phần thưởng ({drops.Count}): {lootNames}");

            return true;
        }

        /// <summary>
        /// Điểm vào tương tác (có thể gọi từ PlayerInteractionSystem).
        /// </summary>
        public void Interact() => OpenChest();
    }
}
