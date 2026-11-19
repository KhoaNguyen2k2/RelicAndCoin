using RelicAndCoin.Core.Stats;
using UnityEngine;

namespace RelicAndCoin.Dungeon.Curse
{
    /// <summary>
    /// ScriptableObject dinh nghia mot loai loi nguyen: ten, mo ta,
    /// ty loi xuat hien goc, va StatModifier giam chi so nhan vat khi bi dính.
    /// </summary>
    [CreateAssetMenu(menuName = "RelicAndCoin/Curse/Curse Data", fileName = "CurseData")]
    public sealed class CurseDataSO : ScriptableObject
    {
        [Header("Thong tin")]
        [SerializeField] private string _curseId = "curse_001";
        [SerializeField] private string _displayName = "Loi nguyen";
        [SerializeField] [TextArea(2, 4)] private string _description = "";

        [Header("Xuat hien")]
        [Range(0f, 1f)] public float BaseChance = 0.1f;

        [Header("Phat qua (giam chi so nhan vat)")]
        public StatModifier StatPenalty;

        public string CurseId       => _curseId;
        public string DisplayName   => _displayName;
        public string Description   => _description;
    }
}
