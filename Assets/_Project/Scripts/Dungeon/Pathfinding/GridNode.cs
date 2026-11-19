using UnityEngine;

namespace RelicAndCoin.Dungeon.Pathfinding
{
    /// <summary>
    /// Một ô lưới trong bản đồ hầm ngục. Pure C#, không phụ thuộc UnityEngine ngoài Vector2Int.
    /// </summary>
    public sealed class GridNode
    {
        public Vector2Int Position { get; }
        public bool IsWalkable { get; set; }

        public int GCost { get; set; }
        public int HCost { get; set; }
        public int FCost => GCost + HCost;

        public GridNode Parent { get; set; }

        public GridNode(Vector2Int position, bool isWalkable = true)
        {
            Position = position;
            IsWalkable = isWalkable;
        }
    }
}
