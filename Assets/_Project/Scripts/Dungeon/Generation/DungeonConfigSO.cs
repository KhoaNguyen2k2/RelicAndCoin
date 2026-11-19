using UnityEngine;

namespace RelicAndCoin.Dungeon
{
    [CreateAssetMenu(menuName = "RelicAndCoin/Dungeon/Dungeon Config", fileName = "DungeonConfig")]
    public sealed class DungeonConfigSO : ScriptableObject
    {
        [Header("Lưới hầm ngục")]
        [Min(1)] public int GridWidth = 20;
        [Min(1)] public int GridHeight = 15;

        [Header("Số lượng phòng")]
        [Min(1)] public int MinRoomCount = 6;
        [Min(1)] public int MaxRoomCount = 12;

        [Header("Kích thước phòng (ô lưới)")]
        [Min(1)] public Vector2Int MinRoomSize = new Vector2Int(4, 4);
        [Min(1)] public Vector2Int MaxRoomSize = new Vector2Int(8, 6);

        [Header("Tỷ lệ xuất hiện phòng (0 = không bao giờ, 1 = luôn)")]
        [Range(0f, 1f)] public float TreasureRoomChance = 0.15f;
        [Range(0f, 1f)] public float ShopRoomChance = 0.10f;

        public Vector2Int GridSize => new Vector2Int(GridWidth, GridHeight);
    }
}
