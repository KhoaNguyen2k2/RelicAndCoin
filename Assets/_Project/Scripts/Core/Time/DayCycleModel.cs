using System;

namespace RelicAndCoin.Core.Time
{
    /// <summary>
    /// Pure C# — không phụ thuộc UnityEngine.
    /// MonoBehaviour controller gọi Tick() mỗi frame và đăng ký vào các Action để nhận event.
    /// </summary>
    public sealed class DayCycleModel
    {
        // ── Trạng thái ────────────────────────────────────────────────
        public DayPhase CurrentPhase { get; private set; } = DayPhase.Morning_Prep;
        public int CurrentDay { get; private set; } = 1;
        public float PhaseElapsed { get; private set; }

        // ── Events (nội bộ hệ thống — dùng C# Action theo CLAUDE.md) ──
        public event Action<DayPhase> OnPhaseChanged;
        public event Action<int> OnNewDayStarted;

        // ── Config ────────────────────────────────────────────────────
        // Thứ tự index khớp với thứ tự enum DayPhase (0=Morning, 1=Day, 2=Evening, 3=Night)
        private readonly float[] _phaseDurations;

        private static readonly float[] DefaultDurations =
        {
            60f,   // Morning_Prep
            180f,  // Day_ShopOpen
            60f,   // Evening_Closing
            120f,  // Night_Dungeon
        };

        public DayCycleModel(float[] phaseDurations = null)
        {
            int phaseCount = Enum.GetValues(typeof(DayPhase)).Length;
            _phaseDurations = new float[phaseCount];

            if (phaseDurations != null && phaseDurations.Length == phaseCount)
                Array.Copy(phaseDurations, _phaseDurations, phaseCount);
            else
                Array.Copy(DefaultDurations, _phaseDurations, phaseCount);
        }

        // ── API công khai ─────────────────────────────────────────────

        /// <param name="deltaTime">Thời gian thực tế đã trôi qua (giây), nhân với game speed bên ngoài.</param>
        public void Tick(float deltaTime)
        {
            PhaseElapsed += deltaTime;

            float currentPhaseDuration = _phaseDurations[(int)CurrentPhase];
            if (PhaseElapsed >= currentPhaseDuration)
                AdvancePhase();
        }

        public float GetPhaseProgress() =>
            PhaseElapsed / _phaseDurations[(int)CurrentPhase];

        public float GetPhaseDuration(DayPhase phase) =>
            _phaseDurations[(int)phase];

        // ── Nội bộ ────────────────────────────────────────────────────

        private void AdvancePhase()
        {
            PhaseElapsed = 0f;

            int next = ((int)CurrentPhase + 1) % _phaseDurations.Length;
            CurrentPhase = (DayPhase)next;

            // Mỗi khi quay lại Morning_Prep = bắt đầu ngày mới
            if (CurrentPhase == DayPhase.Morning_Prep)
            {
                CurrentDay++;
                OnNewDayStarted?.Invoke(CurrentDay);
            }

            OnPhaseChanged?.Invoke(CurrentPhase);
        }
    }
}
