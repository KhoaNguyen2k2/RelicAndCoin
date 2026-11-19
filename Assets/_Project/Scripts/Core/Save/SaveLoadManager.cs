using System;
using System.IO;
using UnityEngine;

namespace RelicAndCoin.Core.Save
{
    /// <summary>
    /// Pure C# static service — không kế thừa MonoBehaviour.
    /// Lưu/đọc JSON an toàn tại Application.persistentDataPath dùng cơ chế atomic write (.tmp).
    /// </summary>
    public static class SaveLoadManager
    {
        private const string SaveFileName = "savegame.json";

        public static string SaveFilePath =>
            Path.Combine(Application.persistentDataPath, SaveFileName);

        public static bool SaveFileExists => File.Exists(SaveFilePath);

        /// <summary>
        /// Ghi SaveData ra file JSON bằng cơ chế atomic (ghi ra .tmp rồi đổi tên).
        /// Tránh corrupt save khi game bị tắt đột ngột giữa lúc đang ghi.
        /// </summary>
        public static bool Save(SaveData data)
        {
            if (data == null) return false;

            data.SaveDateTime = DateTime.UtcNow.ToString("o"); // ISO 8601
            string json       = JsonUtility.ToJson(data, prettyPrint: true);
            string tmpPath    = SaveFilePath + ".tmp";

            try
            {
                File.WriteAllText(tmpPath, json);

                // Unity .NET Standard 2.1 không hỗ trợ overload overwrite của File.Move
                if (File.Exists(SaveFilePath)) File.Delete(SaveFilePath);
                File.Move(tmpPath, SaveFilePath);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveLoadManager] Lỗi khi lưu file: {ex.Message}");

                // Dọn dẹp file tạm nếu còn sót lại
                if (File.Exists(tmpPath))
                    try { File.Delete(tmpPath); } catch { /* ignore */ }

                return false;
            }
        }

        /// <summary>
        /// Đọc SaveData từ file. Trả về SaveData mới với giá trị mặc định nếu file không tồn tại hoặc lỗi.
        /// </summary>
        public static SaveData Load()
        {
            if (!SaveFileExists)
                return new SaveData();

            try
            {
                string json = File.ReadAllText(SaveFilePath);
                var data    = JsonUtility.FromJson<SaveData>(json);
                return data ?? new SaveData();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveLoadManager] Lỗi khi đọc file save, trả về save mới: {ex.Message}");
                return new SaveData();
            }
        }

        public static bool DeleteSave()
        {
            try
            {
                if (SaveFileExists) File.Delete(SaveFilePath);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveLoadManager] Lỗi khi xóa file: {ex.Message}");
                return false;
            }
        }
    }
}
