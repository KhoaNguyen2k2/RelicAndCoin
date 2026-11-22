using UnityEngine;
using UnityEngine.Tilemaps;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RelicAndCoin.Dungeon.Generation
{
    /// <summary>
    /// MonoBehaviour Bootstrapper gắn cùng DungeonGrid bên cạnh DungeonTilemapPainter.
    /// Tự động nạp Tile Pixel Art từ Assets/_Project/Art/ (hoặc sinh fallback nếu trống),
    /// và gọi DungeonGenerator sinh hầm ngục ngay trong Start().
    /// </summary>
    [RequireComponent(typeof(DungeonTilemapPainter))]
    public sealed class DungeonRoomBootstrapper : MonoBehaviour
    {
        [Header("Cấu hình Hầm ngục")]
        [SerializeField] private DungeonConfigSO _config;
        [SerializeField] private bool _autoGenerateOnStart = true;
        [SerializeField] private bool _centerDungeonAtOrigin = true;
        [SerializeField] private bool _useRandomSeed = true;
        [SerializeField] private int _seed = 12345;

        private DungeonTilemapPainter _painter;
        private DungeonGenerator _generator;

        public DungeonGenerator Generator => _generator;
        public DungeonConfigSO Config => _config;

        private void Awake()
        {
            _painter = GetComponent<DungeonTilemapPainter>();

            // Fallback cấu hình mặc định nếu chưa gán ScriptableObject trong Inspector
            if (_config == null)
            {
                _config = ScriptableObject.CreateInstance<DungeonConfigSO>();
            }

            EnsureDefaultTiles();
        }

        private void Start()
        {
            if (_autoGenerateOnStart)
            {
                GenerateAndPaintDungeon();
            }
        }

        /// <summary>
        /// Tạo và vẽ bản đồ hầm ngục. Có thể gọi lại để tái tạo (regenerate) màn chơi.
        /// </summary>
        public void GenerateAndPaintDungeon(int? customSeed = null)
        {
            int actualSeed = customSeed ?? (_useRandomSeed ? Random.Range(1, 1000000) : _seed);
            _generator = new DungeonGenerator(_config, actualSeed);
            _generator.Generate();

            if (_painter != null)
            {
                // Xóa sạch sàn & tường cũ: không vẽ khối gạch hộp lơ lửng giữa màn hình nữa
                _painter.FloorTilemap?.ClearAllTiles();
                _painter.WallTilemap?.ClearAllTiles();

                // Thay vào đó, đặt Cổng Hầm Ngục ở góc trên bên phải lưới
                SpawnEntrancePortal();

                if (_centerDungeonAtOrigin)
                {
                    CenterAtOrigin();
                }

                Debug.Log($"[DungeonRoomBootstrapper] Đã sinh thành công {_generator.Rooms.Count} phòng với Seed: {actualSeed}");
            }
        }

        /// <summary>
        /// Dịch chuyển Grid để căn giữa phòng bắt đầu (hoặc toàn bộ lưới) quanh tọa độ (0,0),
        /// giúp camera mặc định nhìn thẳng vào phòng bắt đầu mà không bị lệch hay che khuất.
        /// </summary>
        public void CenterAtOrigin()
        {
            if (_config != null)
            {
                transform.position = new Vector3(-_config.GridWidth / 2f, -_config.GridHeight / 2f, 0f);
            }
        }

        /// <summary>
        /// Tạo đối tượng Cổng Hầm Ngục ở góc trên bên phải lưới (5.5, 2.8),
        /// để người chơi có không gian rộng rãi chiến đấu, mở rương và bán hàng
        /// mà không bị tường gạch vẽ đè lên nhân vật hay quầy hàng.
        /// </summary>
        private void SpawnEntrancePortal()
        {
            // Đối tượng gốc (không con của DungeonGrid) để vị trí thế giới (5.5, 2.8)
            // không bị dịch khi CenterAtOrigin di chuyển cha Grid sau này.
            var portalGO = new GameObject("DungeonEntrancePortal");
            portalGO.AddComponent<DungeonEntrancePortal>();
        }

        private void EnsureDefaultTiles()
        {
            if (_painter == null) return;

            // 1. Sàn hầm ngục: Ưu tiên nạp Sprite DungeonFloor_Pixel
            if (_painter.FloorTile == null)
            {
                Sprite floorSprite = LoadPixelSprite("Assets/_Project/Art/DungeonFloor_Pixel.png");
                if (floorSprite != null)
                {
                    _painter.FloorTile = CreateSpriteTile(floorSprite, "FloorTile_Pixel");
                }
                else
                {
                    _painter.FloorTile = CreateColorTile(new Color(0.42f, 0.44f, 0.47f, 1f), "DefaultFloorTile");
                }
            }

            // 2. Tường hầm ngục: Ưu tiên nạp Sprite DungeonWall_Pixel
            if (_painter.WallTile == null)
            {
                Sprite wallSprite = LoadPixelSprite("Assets/_Project/Art/DungeonWall_Pixel.png");
                if (wallSprite != null)
                {
                    _painter.WallTile = CreateSpriteTile(wallSprite, "WallTile_Pixel");
                }
                else
                {
                    _painter.WallTile = CreateColorTile(new Color(0.20f, 0.14f, 0.10f, 1f), "DefaultWallTile");
                }
            }

            // 3. Cửa gỗ cổ điển
            if (_painter.DoorTile == null)
            {
                _painter.DoorTile = CreateColorTile(new Color(0.55f, 0.35f, 0.15f, 1f), "DefaultDoorTile", 16f);
            }
        }

        private static Sprite LoadPixelSprite(string assetPath)
        {
#if UNITY_EDITOR
            return AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
#else
            return null;
#endif
        }

        private static Tile CreateSpriteTile(Sprite sprite, string tileName)
        {
            var tile = ScriptableObject.CreateInstance<Tile>();
            tile.name = tileName;
            tile.sprite = sprite;
            tile.color = Color.white;
            return tile;
        }

        private static Tile CreateColorTile(Color color, string tileName, float pixelsPerUnit = 16f)
        {
            var texture = new Texture2D(16, 16, TextureFormat.RGBA32, false);
            for (int y = 0; y < 16; y++)
                for (int x = 0; x < 16; x++)
                    texture.SetPixel(x, y, color);
            texture.Apply();
            texture.filterMode = FilterMode.Point;

            // Viền tối khớp kích thước với DungeonFloor_Pixel / DungeonWall_Pixel (16x16)
            for (int i = 0; i < 16; i++)
            {
                texture.SetPixel(i, 0, Color.black);
                texture.SetPixel(i, 15, Color.black);
                texture.SetPixel(0, i, Color.black);
                texture.SetPixel(15, i, Color.black);
            }
            texture.Apply();

            var sprite = Sprite.Create(
                texture,
                new Rect(0, 0, 16, 16),
                new Vector2(0.5f, 0.5f),
                pixelsPerUnit);
            sprite.name = tileName + "_Sprite";

            var tile = ScriptableObject.CreateInstance<Tile>();
            tile.name = tileName;
            tile.sprite = sprite;
            tile.color = Color.white;
            return tile;
        }
    }
}
