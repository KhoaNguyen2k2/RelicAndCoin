using UnityEngine;

namespace RelicAndCoin.Inventory
{
    /// <summary>
    /// ScriptableObject lưu dữ liệu thuần túy của một loại vật phẩm:
    /// ID, tên, mô tả, biểu tượng, phân loại, kích thước chiếm ô, số lượng xếp chồng,
    /// giá trị gốc và lời nguyền.
    /// Không chứa business logic.
    /// </summary>
    [CreateAssetMenu(
        menuName = "RelicAndCoin/Inventory/Item Data",
        fileName = "ItemData")]
    public sealed class ItemDataSO : ScriptableObject
    {
        [Header("Thông tin cơ bản")]
        [Tooltip("Mã định danh duy nhất (dùng lưu tải, craft, drop table).")]
        public string Id = string.Empty;

        [Tooltip("Tên hiển thị của vật phẩm.")]
        public string DisplayName = string.Empty;

        [Tooltip("Mô tả ngắn gọn hiển thị trong tooltip / sổ tay vật phẩm.")]
        [TextArea(2, 4)]
        public string Description = string.Empty;

        [Tooltip("Biểu tượng của vật phẩm.")]
        public Sprite Icon;

        [Header("Túi đồ")]
        [Tooltip("Phân loại vật phẩm.")]
        public ItemCategory Category;

        [Tooltip("Kích thước chiếm ô trên lưới ba-lô (ví dụ: 1x1, 1x2, 2x2).")]
        public Vector2Int GridSize = Vector2Int.one;

        [Tooltip("Số lượng xếp chồng tối đa trong một ô.")]
        [Min(1)]
        public int MaxStack = 1;

        [Tooltip("Giá trị gốc khi mua/bán tại cửa hàng.")]
        [Min(0f)]
        public float BaseValue = 0f;

        [Header("Lời nguyền")]
        [Tooltip("Loại lời nguyền gắn với vật phẩm (mặc định không có).")]
        public ItemCurseType CurseType = ItemCurseType.None;
    }
}
