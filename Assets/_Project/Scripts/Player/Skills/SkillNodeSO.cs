using RelicAndCoin.Core.Stats;
using RelicAndCoin.Player;
using UnityEngine;

namespace RelicAndCoin.Player.Skills
{
    /// <summary>
    /// ScriptableObject lưu dữ liệu một ô kỹ năng: ID, tên, mô tả, lớp nhân vật yêu cầu,
    /// ô kỹ năng tiền đề, điểm kỹ năng cần thiết, chi phí Mana/Stamina, thời gian hồi chiêu
    /// và danh sách StatModifier nội tại áp khi mở khóa.
    /// Pure data — không chứa business logic.
    /// </summary>
    [CreateAssetMenu(
        menuName = "RelicAndCoin/Player/Skill Node",
        fileName = "SkillNode")]
    public sealed class SkillNodeSO : ScriptableObject
    {
        [Header("Thông tin cơ bản")]
        [Tooltip("Mã định danh duy nhất của kỹ năng (dùng lưu tải, cây kỹ năng).")]
        public string Id = string.Empty;

        [Tooltip("Tên hiển thị của kỹ năng.")]
        public string DisplayName = string.Empty;

        [Tooltip("Mô tả ngắn gọn hiển thị trong tooltip / màn hình cây kỹ năng.")]
        [TextArea(2, 4)]
        public string Description = string.Empty;

        [Header("Yêu cầu mở khóa")]
        [Tooltip("Lớp nhân vật được phép học kỹ năng này. Không chọn = mọi lớp đều được học.")]
        public PlayerClassType RequiredClass;

        [Tooltip("Ô kỹ năng phải mở khóa trước才 có thể học ô này. Không chọn = mở khóa ngay.")]
        public SkillNodeSO PrerequisiteNode;

        [Tooltip("Số điểm kỹ năng cần tiêu để mở khóa ô này.")]
        [Min(0)] public int SkillPointCost = 1;

        [Header("Chi phí sử dụng")]
        [Min(0f)] public float ManaCost = 0f;
        [Min(0f)] public float StaminaCost = 0f;
        [Min(0f)] public float CooldownSeconds = 0f;

        [Header("Hiệu ứng nội tại khi mở khóa")]
        [Tooltip("Các StatModifier được áp vĩnh viễn vào CharacterStatsModel khi mở khóa kỹ năng này.")]
        public StatModifier[] StatModifiers = System.Array.Empty<StatModifier>();
    }
}
