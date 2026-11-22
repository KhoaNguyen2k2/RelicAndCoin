using System.Collections;
using System.Collections.Generic;
using RelicAndCoin.Combat.Weapons;
using RelicAndCoin.Core.Stats;
using RelicAndCoin.Inventory;
using RelicAndCoin.Shop;
using UnityEngine;

namespace RelicAndCoin.Combat.Loot
{
    /// <summary>
    /// Thực thể vật phẩm rơi trên mặt đất (sau khi mở rương hoặc tiêu diệt quái).
    /// Hiển thị Icon và cho phép người chơi bấm phím 'E' để nhặt, trang bị vũ khí hoặc hồi máu.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class DroppedItemPickup : MonoBehaviour
    {
        [Header("Dữ liệu vật phẩm")]
        [SerializeField] private LootEntry _lootEntry;
        [SerializeField] private ItemDataSO _itemData;
        [SerializeField] private WeaponDataSO _weaponData;
        [SerializeField] private int _quantity = 1;

        [Header("Tương tác & Hiển thị")]
        [SerializeField] private float _pickupRadius = 1.8f;
        [SerializeField] private SpriteRenderer _spriteRenderer;

        private Transform _playerTransform;
        private Vector3 _basePosition;
        private float _bobTimer;
        private bool _isPickedUp = false;

        public LootEntry Entry => _lootEntry;
        public ItemDataSO ItemData => _itemData;
        public WeaponDataSO WeaponData => _weaponData;
        public int Quantity => _quantity;

        private void Awake()
        {
            if (_spriteRenderer == null)
                _spriteRenderer = GetComponent<SpriteRenderer>();

            _basePosition = transform.position;
        }

        private void Start()
        {
            // Tự động tìm Player nếu có trong Scene
            var playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
                _playerTransform = playerObj.transform;

            UpdateVisual();
        }

        private void Update()
        {
            if (_isPickedUp) return;

            // Hiệu ứng nhấp nhô nhẹ trên mặt đất
            _bobTimer += Time.deltaTime * 3f;
            transform.position = _basePosition + new Vector3(0f, Mathf.Sin(_bobTimer) * 0.08f, 0f);

            // Kiểm tra người chơi ở gần
            if (_playerTransform == null)
            {
                var playerObj = GameObject.FindWithTag("Player");
                if (playerObj != null)
                    _playerTransform = playerObj.transform;
                return;
            }

            float dist = Vector2.Distance(transform.position, _playerTransform.position);
            if (dist <= _pickupRadius)
            {
                // Khi đứng gần và bấm E -> Nhặt vật phẩm
                if (Input.GetKeyDown(KeyCode.E))
                {
                    TryPickup(_playerTransform.gameObject);
                }
            }
        }

        /// <summary>
        /// Khởi tạo dữ liệu từ LootEntry.
        /// </summary>
        public void Initialize(LootEntry entry)
        {
            _lootEntry = entry;
            if (entry == null) return;

            _quantity = Mathf.Max(1, entry.MinQuantity);
            if (entry.RewardType == LootRewardType.Weapon && entry.Weapon != null)
            {
                _weaponData = entry.Weapon;
            }
            else if (entry.RewardType == LootRewardType.Item && entry.Item != null)
            {
                _itemData = entry.Item;
            }

            UpdateVisual();
        }

        /// <summary>
        /// Khởi tạo dữ liệu từ ItemDataSO.
        /// </summary>
        public void Initialize(ItemDataSO item, int quantity = 1)
        {
            _itemData = item;
            _quantity = Mathf.Max(1, quantity);
            UpdateVisual();
        }

        /// <summary>
        /// Khởi tạo dữ liệu từ WeaponDataSO.
        /// </summary>
        public void Initialize(WeaponDataSO weapon)
        {
            _weaponData = weapon;
            _quantity = 1;
            UpdateVisual();
        }

        /// <summary>
        /// Tự động cập nhật hình ảnh đại diện (sử dụng Pixel Art sắc nét 16x16 có viền tối).
        /// </summary>
        public void UpdateVisual()
        {
            if (_spriteRenderer == null)
                _spriteRenderer = GetComponent<SpriteRenderer>();

            Sprite targetSprite = null;

            if (_itemData != null && _itemData.Icon != null)
            {
                targetSprite = _itemData.Icon;
            }
            else if (_lootEntry != null && _lootEntry.Icon != null)
            {
                targetSprite = _lootEntry.Icon;
            }

            if (targetSprite != null)
            {
                _spriteRenderer.sprite = targetSprite;
                _spriteRenderer.color = Color.white;
            }
            else
            {
                if (_weaponData != null || (_lootEntry != null && _lootEntry.RewardType == LootRewardType.Weapon))
                {
                    _spriteRenderer.sprite = GetOrCreateWeaponSprite();
                }
                else if (_itemData != null && _itemData.Category == ItemCategory.Potion)
                {
                    _spriteRenderer.sprite = GetOrCreatePotionSprite();
                }
                else
                {
                    _spriteRenderer.sprite = GetOrCreateCoinSprite();
                }

                _spriteRenderer.color = Color.white;
            }

            _spriteRenderer.sortingOrder = 5;
        }

        /// <summary>
        /// Điểm vào tương tác (có thể gọi từ Player hoặc phím E).
        /// </summary>
        public bool Interact(GameObject player) => TryPickup(player);

        private bool TryPickup(GameObject player)
        {
            if (_isPickedUp || player == null) return false;
            _isPickedUp = true;

            // 1. Trường hợp là Vũ khí -> Tự động trang bị
            if (_weaponData != null)
            {
                var weaponController = player.GetComponent<WeaponController>();
                if (weaponController != null)
                {
                    weaponController.EquipWeapon(_weaponData);
                    Debug.Log($"<color=cyan>[NHẶT ĐỒ]</color> Đã nhặt và trang bị vũ khí: <b>{_weaponData.DisplayName}</b> (Sát thương gốc: {_weaponData.BaseDamage})!");
                }
            }
            // 2. Trường hợp là Thuốc hồi phục (Potion) -> Hồi máu cho nhân vật
            else if (_itemData != null && _itemData.Category == ItemCategory.Potion)
            {
                var stats = player.GetComponent<CharacterStatsController>();
                if (stats != null)
                {
                    float healAmount = _itemData.BaseValue > 0f ? _itemData.BaseValue : 35f;
                    stats.Heal(healAmount);
                    Debug.Log($"<color=green>[HỒI MÁU]</color> Đã sử dụng bình thuốc <b>{_itemData.DisplayName}</b>: Hồi phục {healAmount} HP! (HP hiện tại: {stats.CurrentHP}/{stats.MaxHP})");
                }
            }
            // 3. Trường hợp là Vật phẩm khác -> Thêm vào túi đồ
            else if (_itemData != null)
            {
                var bootstrapper = Object.FindAnyObjectByType<ShopAndInventoryBootstrapper>();
                if (bootstrapper != null && bootstrapper.InventoryModel != null)
                {
                    bootstrapper.InventoryModel.TryAddItem(_itemData);
                }
                Debug.Log($"<color=yellow>[TÚI ĐỒ]</color> Đã nhặt vật phẩm <b>{_itemData.DisplayName}</b> x{_quantity} vào túi đồ!");
            }
            else
            {
                Debug.Log($"<color=white>[NHẶT ĐỒ]</color> Đã nhặt vật phẩm thành công!");
            }

            Destroy(gameObject);
            return true;
        }

        /// <summary>
        /// Sinh đối tượng DroppedItemPickup trong thế giới và tạo hiệu ứng văng nhẹ.
        /// </summary>
        public static DroppedItemPickup Create(Vector3 spawnPos, LootEntry lootEntry, Vector2 popDirection)
        {
            var go = new GameObject($"DroppedItem_{lootEntry?.DisplayName ?? "Item"}");
            go.transform.position = spawnPos;

            var pickup = go.AddComponent<DroppedItemPickup>();
            pickup.Initialize(lootEntry);

            // Gắn collider trigger để tiện phát hiện
            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.5f;

            pickup.StartCoroutine(pickup.PopOutAnimation(spawnPos, spawnPos + (Vector3)popDirection));
            return pickup;
        }

        private IEnumerator PopOutAnimation(Vector3 start, Vector3 target)
        {
            float duration = 0.25f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                // Đường cong nảy parabolic
                float arc = Mathf.Sin(t * Mathf.PI) * 0.4f;
                Vector3 current = Vector3.Lerp(start, target, t);
                current.y += arc;
                transform.position = current;
                yield return null;
            }

            transform.position = target;
            _basePosition = target;
        }

        private static Sprite _cachedWeaponSprite;
        private static Sprite _cachedPotionSprite;
        private static Sprite _cachedCoinSprite;

        /// <summary>
        /// Tạo Sprite Pixel Art 16x16: Thanh kiếm bạc lưỡi bén có viền tối và chuôi vàng.
        /// </summary>
        private static Sprite GetOrCreateWeaponSprite()
        {
            if (_cachedWeaponSprite != null) return _cachedWeaponSprite;

            string[] pattern = new string[]
            {
                "...............B",
                "..............BW",
                ".............BWS",
                "............BWSD",
                "...........BWSD.",
                "..........BWSD..",
                ".........BWSD...",
                "..B.....BWSD....",
                ".BGB...BWSD.....",
                "..BGBBBGGB......",
                "...BGGGBB.......",
                "....BHB.........",
                "...BHB..........",
                "..BGB...........",
                ".BBB............",
                "................"
            };

            var palette = new Dictionary<char, Color>
            {
                { '.', new Color(0, 0, 0, 0) },
                { 'B', new Color(0.10f, 0.12f, 0.18f, 1f) }, // Viền tối
                { 'W', new Color(0.98f, 0.98f, 1.00f, 1f) }, // Lưỡi kiếm sáng
                { 'S', new Color(0.72f, 0.80f, 0.90f, 1f) }, // Bạc trung tính
                { 'D', new Color(0.45f, 0.52f, 0.65f, 1f) }, // Bóng lưỡi kiếm
                { 'G', new Color(0.98f, 0.78f, 0.18f, 1f) }, // Chuôi kiếm vàng
                { 'H', new Color(0.48f, 0.28f, 0.12f, 1f) }  // Tay cầm da nâu
            };

            _cachedWeaponSprite = GeneratePixelArtSprite(pattern, palette);
            return _cachedWeaponSprite;
        }

        /// <summary>
        /// Tạo Sprite Pixel Art 16x16: Bình thủy tinh chứa thuốc hồi máu đỏ rực có bọt sáng.
        /// </summary>
        private static Sprite GetOrCreatePotionSprite()
        {
            if (_cachedPotionSprite != null) return _cachedPotionSprite;

            string[] pattern = new string[]
            {
                "................",
                ".....BBBBB......",
                ".....BCCCB......",
                "......BBB.......",
                ".....BGGGB......",
                "....BG...GB.....",
                "...BG.....GB....",
                "..BG.W.....GB...",
                "..B.WRRRRRR.B...",
                "..B.WRRRRRR.B...",
                "..B.WRRRRRR.B...",
                "..B.RRRRDRR.B...",
                "..B.RRRRRRR.B...",
                "..B.DDDDDDD.B...",
                "...BBBBBBBBB....",
                "................"
            };

            var palette = new Dictionary<char, Color>
            {
                { '.', new Color(0, 0, 0, 0) },
                { 'B', new Color(0.12f, 0.10f, 0.16f, 1f) }, // Viền tối
                { 'C', new Color(0.60f, 0.40f, 0.20f, 1f) }, // Nút bần gỗ
                { 'G', new Color(0.78f, 0.92f, 0.98f, 0.9f) },// Thủy tinh trong
                { 'W', new Color(1.00f, 0.65f, 0.75f, 1f) }, // Vệt sáng thuốc
                { 'R', new Color(0.92f, 0.16f, 0.25f, 1f) }, // Thuốc đỏ
                { 'D', new Color(0.55f, 0.06f, 0.14f, 1f) }  // Bóng thuốc đáy
            };

            _cachedPotionSprite = GeneratePixelArtSprite(pattern, palette);
            return _cachedPotionSprite;
        }

        /// <summary>
        /// Tạo Sprite Pixel Art 16x16: Đồng tiền vàng chạm khắc biểu tượng lấp lánh.
        /// </summary>
        private static Sprite GetOrCreateCoinSprite()
        {
            if (_cachedCoinSprite != null) return _cachedCoinSprite;

            string[] pattern = new string[]
            {
                "................",
                ".....BBBBBB.....",
                "...BBGGGGGGBB...",
                "..BGGYYYYYYGGB..",
                "..BGYYYWWYYYGB..",
                "..BGYYWWWWYYGB..",
                ".BGYYWWDDWWYYGB.",
                ".BGYYWDDDDDWYGB.",
                ".BGYYWDDDDDWYGB.",
                ".BGYYWWDDWWYYGB.",
                ".BGYYWWWWYYYGB..",
                "..BGYYYWWYYYGB..",
                "..BGYYYYYYYYGB..",
                "...BBGGGGGGBB...",
                ".....BBBBBB.....",
                "................"
            };

            var palette = new Dictionary<char, Color>
            {
                { '.', new Color(0, 0, 0, 0) },
                { 'B', new Color(0.22f, 0.16f, 0.05f, 1f) }, // Viền tối
                { 'G', new Color(0.74f, 0.54f, 0.12f, 1f) }, // Vành vàng đậm
                { 'Y', new Color(0.98f, 0.82f, 0.20f, 1f) }, // Vàng tươi
                { 'W', new Color(1.00f, 0.96f, 0.60f, 1f) }, // Ánh kim lấp lánh
                { 'D', new Color(0.50f, 0.35f, 0.06f, 1f) }  // Vết khắc chạm sâu
            };

            _cachedCoinSprite = GeneratePixelArtSprite(pattern, palette);
            return _cachedCoinSprite;
        }

        private static Sprite GeneratePixelArtSprite(string[] rows, Dictionary<char, Color> palette)
        {
            int size = 16;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode   = TextureWrapMode.Clamp
            };

            Color[] pixels = new Color[size * size];

            // Trong mảng chuỗi, rows[0] là hàng trên cùng (y = 15)
            for (int y = 0; y < size; y++)
            {
                int rowIdx = (size - 1) - y;
                string row = rows[rowIdx];
                for (int x = 0; x < size; x++)
                {
                    char c = x < row.Length ? row[x] : '.';
                    pixels[y * size + x] = palette.TryGetValue(c, out var color) ? color : Color.clear;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();

            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 16f);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _pickupRadius);
        }
    }
}
