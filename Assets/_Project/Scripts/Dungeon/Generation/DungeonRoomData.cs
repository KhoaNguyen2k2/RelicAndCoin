using System;
using System.Collections.Generic;
using UnityEngine;

namespace RelicAndCoin.Dungeon
{
    [Flags]
    public enum Direction : byte
    {
        None = 0,
        Up = 1 << 0,
        Right = 1 << 1,
        Down = 1 << 2,
        Left = 1 << 3,
    }

    public sealed class DungeonRoomData
    {
        public Vector2Int GridPosition { get; }
        public Vector2Int Size { get; }
        public RoomType RoomType { get; set; }
        public bool IsCleared { get; set; }
        public Direction OpenDirections { get; private set; }

        public DungeonRoomData(Vector2Int gridPosition, Vector2Int size, RoomType roomType)
        {
            GridPosition = gridPosition;
            Size = size;
            RoomType = roomType;
        }

        public RectInt Bounds => new RectInt(GridPosition.x, GridPosition.y, Size.x, Size.y);

        public void OpenDoor(Direction direction) => OpenDirections |= direction;

        public void CloseDoor(Direction direction) => OpenDirections &= ~direction;

        public bool IsDoorOpen(Direction direction) => (OpenDirections & direction) != 0;

        public IEnumerable<Vector2Int> Cells()
        {
            for (int x = 0; x < Size.x; x++)
                for (int y = 0; y < Size.y; y++)
                    yield return new Vector2Int(GridPosition.x + x, GridPosition.y + y);
        }
    }
}
