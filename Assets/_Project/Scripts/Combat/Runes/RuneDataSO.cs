using RelicAndCoin.Core.Stats;
using UnityEngine;

namespace RelicAndCoin.Combat.Runes
{
    /// <summary>
    /// ScriptableObject lưu dữ liệu một loại Rune: ID, tên, bậc độ hiếm, nguyên tố cộng thêm,
    /// danh sách StatModifier tăng chỉ số khi khảm, và giá trị vàng cơ bản.
    /// Pure data — không chứa business logic.
    /// </summary>
    [CreateAssetMenu(
        menuName = "RelicAndCoin/Combat/Rune Data",
        fileName = "RuneData")]
    public sealed class RuneDataSO : ScriptableObject
    {
        [Header("Thông tin cơ bản")]
        [Tooltip("Mã định danh duy nhất của Rune (dùng lưu tải, craft, drop table).")]
        public string Id = string.Empty;

        [Tooltip("Tên hiển thị của Rune.")]
        public string DisplayName = string.Empty;

        [Tooltip("Mô tả ngắn gọn hiển thị trong tooltip.")]
        [TextArea(2, 4)]
        public string Description = string.Empty;

        [Header("Bậc độ & Nguyên tố")]
        public RuneRarity Rarity = RuneRarity.Normal;
        public ElementType BonusElement = ElementType.None;

        [Header("Chỉ số tăng khi khảm")]
        [Tooltip("Các StatModifier được cộng vào CharacterStatsModel khi Rune được gắn vào ô khảm.")]
        public StatModifier[] StatModifiers = System.Array.Empty<StatModifier>();

        [Header("Kinh tế")]
        [Tooltip("Mức giá vàng cơ bản khi mua/bán tại cửa hàng / thợ rèn.")]
        [Min(0f)] public float BasePrice = 100f;
    }
}
