using RelicAndCoin.Combat;
using UnityEngine;

namespace RelicAndCoin.Combat.Weapons
{
    /// <summary>
    /// ScriptableObject lưu dữ liệu một loại vũ khí: ID, tên, phân loại, nguyên tố,
    /// sát thương cơ bản, tầm đánh, tốc độ đánh, chi phí Stamina/Mana mỗi đòn và mức giá cơ bản.
    /// Pure data — không chứa business logic.
    /// </summary>
    [CreateAssetMenu(
        menuName = "RelicAndCoin/Combat/Weapon Data",
        fileName = "WeaponData")]
    public sealed class WeaponDataSO : ScriptableObject
    {
        [Header("Thông tin cơ bản")]
        [Tooltip("Mã định danh duy nhất của vũ khí (dùng trong lưu tải, craft, drop table).")]
        public string Id = string.Empty;

        [Tooltip("Tên hiển thị của vũ khí.")]
        public string DisplayName = string.Empty;

        [Tooltip("Mô tả ngắn gọn hiển thị trong tooltip / sổ tay vật phẩm.")]
        [TextArea(2, 4)]
        public string Description = string.Empty;

        [Header("Phân loại & Nguyên tố")]
        public WeaponCategory Category;
        public ElementType Element = ElementType.None;

        [Header("Sát thương & Tốc độ")]
        [Tooltip("Sát thương cơ bản của vũ khí (cộng thêm chỉ số AttackDamage của người đánh).")]
        [Min(0f)] public float BaseDamage = 10f;

        [Tooltip("Tầm đánh (đơn vị thế giới).")]
        [Min(0f)] public float AttackRange = 1.5f;

        [Tooltip("Số đòn đánh mỗi giây (tốc độ tấn công cơ bản).")]
        [Min(0.01f)] public float AttackSpeed = 1f;

        [Header("Chi phí tài nguyên mỗi đòn")]
        [Min(0f)] public float StaminaCost = 0f;
        [Min(0f)] public float ManaCost = 0f;

        [Header("Kinh tế")]
        [Tooltip("Mức giá cơ bản khi mua/bán tại cửa hàng.")]
        [Min(0f)] public float BasePrice = 50f;
    }
}
