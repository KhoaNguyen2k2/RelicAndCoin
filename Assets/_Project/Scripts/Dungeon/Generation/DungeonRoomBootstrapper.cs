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
                _painter.Paint(_generator.Rooms, _config.GridSize);

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

            // 3. Cửa vàng
            if (_painter.DoorTile == null)
            {
                _painter.DoorTile = CreateColorTile(new Color(0.95f, 0.78f, 0.20f, 1f), "DefaultDoorTile");
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

        private static Tile CreateColorTile(Color color, string tileName)
        {
            var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            texture.SetPixel(0, 0, color);
            texture.Apply();
            texture.filterMode = FilterMode.Point;

            var sprite = Sprite.Create(
                texture,
                new Rect(0, 0, 1, 1),
                new Vector2(0.5f, 0.5f),
                1f);
            sprite.name = tileName + "_Sprite";

            var tile = ScriptableObject.CreateInstance<Tile>();
            tile.name = tileName;
            tile.sprite = sprite;
            tile.color = Color.white;
            return tile;
        }
    }
}
