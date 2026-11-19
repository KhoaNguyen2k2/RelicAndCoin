using RelicAndCoin.Core.Stats;
using System.Collections.Generic;

namespace RelicAndCoin.Player.Skills
{
    /// <summary>
    /// Pure C# — quản lý điểm kỹ năng, kiểm tra điều kiện mở khóa, thực hiện mở khóa và áp dụng
    /// StatModifier vào CharacterStatsModel. Không phụ thuộc UnityEngine.
    /// </summary>
    public sealed class SkillTreeService
    {
        private int _availableSkillPoints;
        private readonly HashSet<SkillNodeSO> _unlocked = new();

        /// <summary>Số điểm kỹ năng còn lại có thể dùng.</summary>
        public int AvailableSkillPoints => _availableSkillPoints;

        /// <summary>Danh sách các kỹ năng đã mở khóa (chỉ đọc).</summary>
        public IReadOnlyCollection<SkillNodeSO> Unlocked => _unlocked;

        public SkillTreeService(int startingSkillPoints)
        {
            _availableSkillPoints = startingSkillPoints;
        }

        /// <summary>Cộng thêm điểm kỹ năng (ví dụ khi lên cấp).</summary>
        public void AddSkillPoints(int amount)
        {
            if (amount > 0) _availableSkillPoints += amount;
        }

        /// <summary>
        /// Kiểm tra có thể mở khóa <paramref name="node"/> hay không:
        /// đủ điểm kỹ năng, lớp nhân vật phù hợp, và đã mở khóa node tiền đề (nếu có).
        /// </summary>
        public bool CanUnlock(SkillNodeSO node, PlayerClassType playerClass)
        {
            if (node == null) return false;
            if (_unlocked.Contains(node)) return false;
            if (_availableSkillPoints < node.SkillPointCost) return false;
            if (node.RequiredClass != playerClass)return false;
            if (node.PrerequisiteNode != null && !_unlocked.Contains(node.PrerequisiteNode)) return false;
            return true;
        }

        /// <summary>
        /// Mở khóa <paramref name="node"/>: trừ điểm kỹ năng, đánh dấu đã mở khóa,
        /// áp StatModifier vào model. Trả về true nếu thành công.
        /// </summary>
        public bool Unlock(SkillNodeSO node, PlayerClassType playerClass, CharacterStatsModel model)
        {
            if (!CanUnlock(node, playerClass)) return false;

            _availableSkillPoints -= node.SkillPointCost;
            _unlocked.Add(node);
            ApplyNodeModifiers(node, model, add: true);
            return true;
        }

        /// <summary>Kiểm tra một kỹ năng đã được mở khóa chưa.</summary>
        public bool IsUnlocked(SkillNodeSO node) => node != null && _unlocked.Contains(node);

        private static void ApplyNodeModifiers(SkillNodeSO node, CharacterStatsModel model, bool add)
        {
            if (node == null || model == null) return;
            object source = node;

            foreach (StatModifier mod in node.StatModifiers)
            {
                if (add) model.AddModifier(new StatModifier(mod.Stat, mod.Type, mod.Value, source));
                else     model.RemoveModifiersBySource(source);
            }
        }
    }
}
