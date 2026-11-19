using RelicAndCoin.Core.Stats;

namespace RelicAndCoin.Player
{
    /// <summary>
    /// Pure C# — áp dụng PlayerClassConfigSO vào CharacterStatsModel
    /// khi người chơi bắt đầu game mới hoặc tải lại file Save.
    /// Không phụ thuộc UnityEngine.
    /// </summary>
    public sealed class PlayerBuildService
    {
        /// <summary>
        /// Xây dựng một nhân vật mới từ lớp và giới tính đã chọn, kết hợp chỉ số gốc
        /// (truyền từ BaseStatsConfigSO qua wrapper MonoBehaviour) với modifier của lớp.
        /// Giới tính hiện tại chỉ ảnh hưởng hình ảnh, không thay đổi chỉ số.
        /// </summary>
        public CharacterStatsModel BuildNew(
            PlayerClassType classType,
            CharacterGender gender,
            float baseMaxHP,
            float baseMaxStamina,
            float baseMaxMana,
            float fatigueDuration,
            float staminaRegenPerSecond,
            float manaRegenPerSecond,
            PlayerClassConfigSO classConfig)
        {
            var model = new CharacterStatsModel(
                baseMaxHP,
                baseMaxStamina,
                baseMaxMana,
                fatigueDuration,
                staminaRegenPerSecond,
                manaRegenPerSecond);

            ApplyClass(classConfig, model);
            return model;
        }

        /// <summary>
        /// Áp modifier của lớp vào model đã có (dùng khi tải lại Save, khi model
        /// đã được khởi tạo từ BaseStatsConfigSO rồi). Có thể gọi nhiều lần an toàn
        /// nếu caller đã xóa modifier cũ của cùng source trước đó.
        /// </summary>
        public void ApplyClass(PlayerClassConfigSO classConfig, CharacterStatsModel model)
        {
            if (classConfig == null || model == null) return;

            foreach (StatModifier mod in classConfig.StatModifiers)
                model.AddModifier(mod);
        }
    }
}
