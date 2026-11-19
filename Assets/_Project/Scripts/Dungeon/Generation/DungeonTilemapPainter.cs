using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace RelicAndCoin.Dungeon.Generation
{
    /// <summary>
    /// MonoBehaviour mỏng: vẽ dữ liệu DungeonGenerator lên Unity Tilemap.
    /// Lớp 1: FloorTilemap (sàn), Lớp 2: WallTilemap (tường + cửa, có va chạm).
    /// </summary>
    public sealed class DungeonTilemapPainter : MonoBehaviour
    {
        [Header("Tilemaps")]
        [SerializeField] private Tilemap _floorTilemap;
        [SerializeField] private Tilemap _wallTilemap;

        [Header("Tiles")]
        [SerializeField] private TileBase _floorTile;
        [SerializeField] private TileBase _wallTile;
        [SerializeField] private TileBase _doorTile;

        public Tilemap FloorTilemap => _floorTilemap;
        public Tilemap WallTilemap => _wallTilemap;

        public TileBase FloorTile
        {
            get => _floorTile;
            set => _floorTile = value;
        }

        public TileBase WallTile
        {
            get => _wallTile;
            set => _wallTile = value;
        }

        public TileBase DoorTile
        {
            get => _doorTile;
            set => _doorTile = value;
        }

        private void Awake()
        {
            // Tự động tìm Tilemap con nếu chưa được gán thủ công trong Inspector
            if (_floorTilemap == null || _wallTilemap == null)
            {
                var tilemaps = GetComponentsInChildren<Tilemap>(true);
                foreach (var tm in tilemaps)
                {
                    if (_floorTilemap == null && tm.gameObject.name.Contains("Floor"))
                        _floorTilemap = tm;
                    else if (_wallTilemap == null && (tm.gameObject.name.Contains("Wall") || tm.gameObject.name.Contains("Obstacle")))
                        _wallTilemap = tm;
                }

                // Nếu vẫn chưa tìm thấy theo tên, gán theo thứ tự
                if (_floorTilemap == null && tilemaps.Length > 0) _floorTilemap = tilemaps[0];
                if (_wallTilemap == null && tilemaps.Length > 1) _wallTilemap = tilemaps[1];
            }
        }

        public void Paint(IReadOnlyList<DungeonRoomData> rooms, Vector2Int gridSize)
        {
            if (_floorTilemap != null) _floorTilemap.ClearAllTiles();
            if (_wallTilemap != null) _wallTilemap.ClearAllTiles();

            var roomCells = new HashSet<Vector2Int>();
            foreach (var room in rooms)
                foreach (var cell in room.Cells())
                    roomCells.Add(cell);

            // Vẽ sàn cho mọi ô thuộc phòng
            if (_floorTilemap != null && _floorTile != null)
                foreach (var cell in roomCells)
                    _floorTilemap.SetTile(new Vector3Int(cell.x, cell.y, 0), _floorTile);

            // Vẽ tường & cửa bao quanh mỗi phòng
            foreach (var room in rooms)
                PaintRoomWalls(room, roomCells);
        }

        private void PaintRoomWalls(DungeonRoomData room, HashSet<Vector2Int> roomCells)
        {
            if (_wallTilemap == null || _wallTile == null) return;

            var bounds = room.Bounds;

            // Cạnh trên & dưới
            for (int x = bounds.xMin; x <= bounds.xMax; x++)
            {
                PaintWallCell(new Vector2Int(x, bounds.yMin - 1), room, Direction.Down, roomCells);
                PaintWallCell(new Vector2Int(x, bounds.yMax + 1), room, Direction.Up, roomCells);
            }
            // Cạnh trái & phải
            for (int y = bounds.yMin; y <= bounds.yMax; y++)
            {
                PaintWallCell(new Vector2Int(bounds.xMin - 1, y), room, Direction.Left, roomCells);
                PaintWallCell(new Vector2Int(bounds.xMax + 1, y), room, Direction.Right, roomCells);
            }
        }

        private void PaintWallCell(Vector2Int cell, DungeonRoomData room, Direction toward,
            HashSet<Vector2Int> roomCells)
        {
            // Ô này là sàn của phòng khác → không vẽ tường đè vào
            if (roomCells.Contains(cell))
                return;

            bool doorOpen = room.IsDoorOpen(toward);
            if (doorOpen && _doorTile != null)
                _wallTilemap.SetTile(new Vector3Int(cell.x, cell.y, 0), _doorTile);
            else
                _wallTilemap.SetTile(new Vector3Int(cell.x, cell.y, 0), _wallTile);
        }
    }
}
