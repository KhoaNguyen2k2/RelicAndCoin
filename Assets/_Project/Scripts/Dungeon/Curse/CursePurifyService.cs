using System;
using System.Collections.Generic;
using RelicAndCoin.Core.Stats;
using UnityEngine;

namespace RelicAndCoin.Dungeon.Curse
{
    /// <summary>
    /// Pure C#: quan ly cac loi nguyen dang dinh tren nguoi choi,
    /// tinh ty loi dính nhan nhan voi DifficultyConfigSO.CurseChanceMultiplier,
    /// va xu ly thanh tiah loi nguyen qua Tu si khi du vang.
    /// </summary>
    public sealed class CursePurifyService
    {
        private readonly List<CurseDataSO> _activeCurses = new();
        private readonly System.Random _rng;
        private float _curseChanceMultiplier = 1f;

        public IReadOnlyList<CurseDataSO> ActiveCurses => _activeCurses;
        public int CurseCount => _activeCurses.Count;

        public CursePurifyService(int seed)
        {
            _rng = new System.Random(seed);
        }

        public void SetDifficulty(float curseChanceMultiplier) =>
            _curseChanceMultiplier = Mathf.Clamp01(curseChanceMultiplier);

        /// <summary>Tra ve true neu loi nguyen duoc ap dinh (dap ung ty le xac suat).</summary>
        public bool TryApplyCurse(CurseDataSO curse)
        {
            if (curse == null) return false;
            float roll = (float)_rng.NextDouble();
            float finalChance = curse.BaseChance * _curseChanceMultiplier;
            if (roll < finalChance)
            {
                _activeCurses.Add(curse);
                return true;
            }
            return false;
        }

        public bool HasCurse(CurseDataSO curse) => curse != null && _activeCurses.Contains(curse);

        /// <summary>Thanh tiah mot loi nguyen neu co du vang.</summary>
        /// <returns>true neu thanh cong, false neu khong du vang hoac khong co loi nguyen.</returns>
        public bool Purge(CurseDataSO curse, int gold, int purgeCost)
        {
            if (curse == null || !_activeCurses.Contains(curse)) return false;
            if (gold < purgeCost) return false;

            _activeCurses.Remove(curse);
            return true;
        }

        public void ClearAll() => _activeCurses.Clear();
    }
}
