using RelicAndCoin.Core.Stats;
using UnityEngine;

namespace RelicAndCoin.Player
{
    /// <summary>
    /// ScriptableObject lưu thông tin một lớp nhân vật: tên, mô tả, vũ khí khởi đầu,
    /// danh sách StatModifier cộng thêm vào chỉ số gốc, và chỉ số Mana/Stamina ưu tiên.
    /// Pure data — không chứa business logic.
    /// </summary>
    [CreateAssetMenu(
        menuName = "RelicAndCoin/Player/Class Config",
        fileName = "PlayerClassConfig")]
    public sealed class PlayerClassConfigSO : ScriptableObject
    {
        [Header("Thông tin lớp")]
        public PlayerClassType ClassType;
        [Tooltip("Tên hiển thị lớp, ví dụ: Chiến binh / Phù thủy")]
        public string ClassName = string.Empty;
        [TextArea(2, 4)] [Tooltip("Mô tả lớp hiển thị trên màn hình chọn nhân vật")]
        public string Description = string.Empty;

        [Header("Vũ khí khởi đầu")]
        [Tooltip("Tham chiếu placeholder — sẽ thay bằng WeaponData khi hệ thống Vũ khí xong (Lượt 5).")]
        public GameObject StartingWeaponPrefab;

        [Header("Chỉ số cộng thêm (áp vào chỉ số gốc)")]
        [Tooltip("Các modifier được cộng vào CharacterStatsModel khi xây dựng nhân vật.")]
        public StatModifier[] StatModifiers = System.Array.Empty<StatModifier>();

        [Header("Ưu tiên chỉ số tài nguyên")]
        [Range(0f, 2f)] [Tooltip("Mana ưu tiên cao hơn Stamina cho Phù thủy, và ngược lại.")]
        public float ManaPriority = 1f;
        [Range(0f, 2f)]
        public float StaminaPriority = 1f;
    }
}
