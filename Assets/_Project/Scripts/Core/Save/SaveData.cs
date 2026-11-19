using System;
using RelicAndCoin.Core.Difficulty;

namespace RelicAndCoin.Core.Save
{
    [Serializable]
    public sealed class SaveData
    {
        // ── Meta ──────────────────────────────────────────────────────
        public string  SaveVersion  = "1.0";
        public string  SaveDateTime = string.Empty; // ISO 8601, set khi lưu

        // ── Progression ───────────────────────────────────────────────
        public DifficultyMode Difficulty = DifficultyMode.Normal;
        public int            CurrentDay = 1;
        public int            Gold       = 0;

        // ── Player vitals ─────────────────────────────────────────────
        public float PlayerHP     = 100f;
        public float PlayerMana   = 100f;
        public float PlayerStamina = 100f;

        // ── Mở rộng: thêm field mới ở đây khi Giai đoạn sau yêu cầu ──
        // Các field không tồn tại trong file cũ sẽ nhận giá trị default
        // khi JSON deserialize — không cần migration code.
    }
}
