using System.Collections.Generic;
using UnityEngine;

namespace RelicAndCoin.Dungeon.Pathfinding
{
    /// <summary>
    /// Biểu diễn toàn bộ mặt bằng hầm ngục: mảng 2 chiều GridNode, biết ô nào đi được, ô nào bị tường chặn.
    /// </summary>
    public sealed class DungeonGridMap
    {
        private readonly GridNode[,] _grid;
        public int Width { get; }
        public int Height { get; }

        public DungeonGridMap(Vector2Int gridSize, IEnumerable<Vector2Int> walkableCells)
        {
            Width = gridSize.x;
            Height = gridSize.y;
            _grid = new GridNode[Width, Height];

            var walkable = new HashSet<Vector2Int>(walkableCells);
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    var pos = new Vector2Int(x, y);
                    _grid[x, y] = new GridNode(pos, walkable.Contains(pos));
                }
            }
        }

        public GridNode GetNode(Vector2Int position)
        {
            return IsInBounds(position) ? _grid[position.x, position.y] : null;
        }

        public bool IsInBounds(Vector2Int position) =>
            position.x >= 0 && position.x < Width && position.y >= 0 && position.y < Height;

        public bool IsWalkable(Vector2Int position) =>
            IsInBounds(position) && _grid[position.x, position.y].IsWalkable;

        /// <summary>Reset chi phí trước mỗi lần tìm đường để tránh rác lingering từ lần trước.</summary>
        public void ResetCosts()
        {
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                {
                    var node = _grid[x, y];
                    node.GCost = int.MaxValue;
                    node.HCost = 0;
                    node.Parent = null;
                }
        }

        public IEnumerable<GridNode> GetNeighbors(GridNode node)
        {
            var dirs = new[] { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };
            foreach (var dir in dirs)
            {
                var pos = node.Position + dir;
                if (IsInBounds(pos))
                    yield return _grid[pos.x, pos.y];
            }
        }
    }
}
