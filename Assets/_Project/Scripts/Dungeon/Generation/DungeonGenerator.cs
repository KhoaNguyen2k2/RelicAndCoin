using System;
using System.Collections.Generic;
using UnityEngine;

namespace RelicAndCoin.Dungeon
{
    public sealed class DungeonGenerator
    {
        private readonly DungeonConfigSO _config;
        private readonly System.Random _rng;

        private readonly List<DungeonRoomData> _rooms = new();

        public IReadOnlyList<DungeonRoomData> Rooms => _rooms;
        public DungeonRoomData StartRoom { get; private set; }
        public DungeonRoomData BossRoom { get; private set; }

        public DungeonGenerator(DungeonConfigSO config, int seed)
        {
            _config = config;
            _rng = new System.Random(seed);
        }

        public void Generate()
        {
            _rooms.Clear();

            // Đặt phòng Start gần trung tâm lưới
            var center = new Vector2Int(_config.GridWidth / 2, _config.GridHeight / 2);
            var startSize = RandomSize();
            var startPosition = center - startSize / 2;
            StartRoom = new DungeonRoomData(startPosition, startSize, RoomType.Start);
            _rooms.Add(StartRoom);

            int roomCount = _rng.Next(_config.MinRoomCount, _config.MaxRoomCount + 1);
            while (_rooms.Count < roomCount)
            {
                if (!TryPlaceRandomRoom())
                    break;
            }

            AssignRoomTypes();
            ConnectRooms();
            PickBossRoom();
        }

        private bool TryPlaceRandomRoom()
        {
            var size = RandomSize();
            var position = new Vector2Int(
                _rng.Next(0, _config.GridWidth - size.x + 1),
                _rng.Next(0, _config.GridHeight - size.y + 1));

            var candidate = new DungeonRoomData(position, size, RoomType.Combat);

            if (OverlapsAnyRoom(candidate, buffer: 1))
                return false;

            _rooms.Add(candidate);
            return true;
        }

        private bool OverlapsAnyRoom(DungeonRoomData room, int buffer)
        {
            foreach (var existing in _rooms)
            {
                var r1 = room.Bounds;
                var r2 = existing.Bounds;
                r1.xMin -= buffer; r1.yMin -= buffer;
                r1.xMax += buffer; r1.yMax += buffer;

                if (r1.Overlaps(r2))
                    return true;
            }
            return false;
        }

        private Vector2Int RandomSize()
        {
            return new Vector2Int(
                _rng.Next(_config.MinRoomSize.x, _config.MaxRoomSize.x + 1),
                _rng.Next(_config.MinRoomSize.y, _config.MaxRoomSize.y + 1));
        }

        private void AssignRoomTypes()
        {
            foreach (var room in _rooms)
            {
                if (room == StartRoom)
                    continue;

                float roll = (float)_rng.NextDouble();
                if (roll < _config.TreasureRoomChance)
                    room.RoomType = RoomType.Treasure;
                else if (roll < _config.TreasureRoomChance + _config.ShopRoomChance)
                    room.RoomType = RoomType.Shop;
                else
                    room.RoomType = RoomType.Combat;
            }
        }

        private void ConnectRooms()
        {
            var connected = new HashSet<DungeonRoomData> { StartRoom };
            var unconnected = new List<DungeonRoomData>(_rooms);
            unconnected.Remove(StartRoom);

            while (unconnected.Count > 0)
            {
                DungeonRoomData bestFrom = null;
                DungeonRoomData bestTo = null;
                int bestDistance = int.MaxValue;

                foreach (var from in connected)
                {
                    foreach (var to in unconnected)
                    {
                        int d = DistanceBetween(from, to);
                        if (d < bestDistance)
                        {
                            bestDistance = d;
                            bestFrom = from;
                            bestTo = to;
                        }
                    }
                }

                if (bestFrom == null || bestTo == null)
                    break;

                CarveCorridor(bestFrom, bestTo);
                connected.Add(bestTo);
                unconnected.Remove(bestTo);
            }
        }

        private int DistanceBetween(DungeonRoomData a, DungeonRoomData b)
        {
            var ca = a.GridPosition + a.Size / 2;
            var cb = b.GridPosition + b.Size / 2;
            return Mathf.Abs(ca.x - cb.x) + Mathf.Abs(ca.y - cb.y);
        }

        private void CarveCorridor(DungeonRoomData a, DungeonRoomData b)
        {
            var ca = a.GridPosition + a.Size / 2;
            var cb = b.GridPosition + b.Size / 2;

            bool horizontalFirst = _rng.Next(2) == 0;
            var corner = horizontalFirst
                ? new Vector2Int(cb.x, ca.y)
                : new Vector2Int(ca.x, cb.y);

            OpenDoorToward(a, corner - ca);
            OpenDoorToward(b, corner - cb);
        }

        private void OpenDoorToward(DungeonRoomData room, Vector2Int toward)
        {
            if (Mathf.Abs(toward.x) > Mathf.Abs(toward.y))
                room.OpenDoor(toward.x > 0 ? Direction.Right : Direction.Left);
            else
                room.OpenDoor(toward.y > 0 ? Direction.Up : Direction.Down);
        }

        private void PickBossRoom()
        {
            DungeonRoomData farthest = StartRoom;
            int maxDistance = 0;
            foreach (var room in _rooms)
            {
                if (room == StartRoom)
                    continue;
                int d = DistanceBetween(StartRoom, room);
                if (d > maxDistance)
                {
                    maxDistance = d;
                    farthest = room;
                }
            }
            BossRoom = farthest;
            BossRoom.RoomType = RoomType.Boss;
        }
    }
}
