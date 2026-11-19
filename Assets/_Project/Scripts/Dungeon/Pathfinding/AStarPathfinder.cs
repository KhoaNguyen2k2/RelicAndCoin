using System.Collections.Generic;
using UnityEngine;

namespace RelicAndCoin.Dungeon.Pathfinding
{
    /// <summary>
    /// Thuật toán A* tìm đường ngắn nhất từ A đến B trên DungeonGridMap.
    /// OpenSet / ClosedSet được tái sử dụng giữa các lần gọi để giảm cấp phát rác GC.
    /// </summary>
    public sealed class AStarPathfinder
    {
        private readonly List<GridNode> _openSet = new List<GridNode>(64);
        private readonly HashSet<GridNode> _closedSet = new HashSet<GridNode>(64);

        public List<Vector2Int> FindPath(DungeonGridMap map, Vector2Int start, Vector2Int end)
        {
            map.ResetCosts();
            _openSet.Clear();
            _closedSet.Clear();

            var startNode = map.GetNode(start);
            var endNode = map.GetNode(end);
            if (startNode == null || endNode == null ||
                !startNode.IsWalkable || !endNode.IsWalkable)
                return null;

            _openSet.Add(startNode);
            startNode.GCost = 0;
            startNode.HCost = Distance(start, end);
            startNode.Parent = null;

            while (_openSet.Count > 0)
            {
                var current = GetLowestFCost(_openSet);
                if (current == endNode)
                    return RetracePath(startNode, endNode);

                _openSet.Remove(current);
                _closedSet.Add(current);

                foreach (var neighbor in map.GetNeighbors(current))
                {
                    if (!neighbor.IsWalkable || _closedSet.Contains(neighbor))
                        continue;

                    int newGCost = current.GCost + Distance(current.Position, neighbor.Position);
                    if (newGCost < neighbor.GCost || !_openSet.Contains(neighbor))
                    {
                        neighbor.GCost = newGCost;
                        neighbor.HCost = Distance(neighbor.Position, end);
                        neighbor.Parent = current;

                        if (!_openSet.Contains(neighbor))
                            _openSet.Add(neighbor);
                    }
                }
            }

            return null; // Không tìm được đường
        }

        private static GridNode GetLowestFCost(List<GridNode> openSet)
        {
            var best = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                var node = openSet[i];
                if (node.FCost < best.FCost ||
                    (node.FCost == best.FCost && node.HCost < best.HCost))
                    best = node;
            }
            return best;
        }

        private static int Distance(Vector2Int a, Vector2Int b) =>
            Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);

        private static List<Vector2Int> RetracePath(GridNode start, GridNode end)
        {
            var path = new List<Vector2Int>();
            var current = end;
            while (current != null)
            {
                path.Add(current.Position);
                current = current.Parent;
            }
            path.Reverse();
            return path;
        }
    }
}
