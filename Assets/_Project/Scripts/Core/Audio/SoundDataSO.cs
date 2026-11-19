using UnityEngine;

namespace RelicAndCoin.Core.Audio
{
    /// <summary>
    /// Dữ liệu âm thanh thuần túy (ScriptableObject): clip, âm lượng, cao độ, cờ lặp.
    /// Không chứa business logic.
    /// </summary>
    [CreateAssetMenu(
        menuName = "RelicAndCoin/Audio/Sound Data",
        fileName = "SoundData")]
    public sealed class SoundDataSO : ScriptableObject
    {
        [Header("Dữ liệu")]
        [Tooltip("Mã định danh hiệu ứng âm thanh (dùng để tìm/tách loại âm thanh nếu cần).")]
        public string Id = string.Empty;

        [Tooltip("Tệp âm thanh (clip).")]
        public AudioClip Clip;

        [Range(0f, 1f)]
        [Tooltip("Âm lượng phát (0 = tắt, 1 = mức gốc của clip).")]
        public float Volume = 1f;

        [Range(-3f, 3f)]
        [Tooltip("Cao độ phát (1 = không đổi).")]
        public float Pitch = 1f;

        [Tooltip("Có lặp lại clips khi hết hay không.")]
        public bool Loop = false;
    }
}
