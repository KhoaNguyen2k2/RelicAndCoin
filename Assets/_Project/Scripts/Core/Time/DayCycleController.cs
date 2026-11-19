using RelicAndCoin.Core.Events;
using UnityEngine;

namespace RelicAndCoin.Core.Time
{
    /// <summary>
    /// MonoBehaviour mỏng: cầu nối giữa Unity lifecycle và DayCycleModel (Pure C#).
    /// </summary>
    public sealed class DayCycleController : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private DayCycleConfigSO _config;
        [SerializeField] [Range(0.1f, 10f)] private float _gameSpeed = 1f;

        [Header("Event Channels (Output)")]
        [SerializeField] private DayPhaseEventChannel _onPhaseChanged;
        [SerializeField] private IntEventChannel      _onNewDayStarted;
        [SerializeField] private VoidEventChannel     _onMorningStarted;
        [SerializeField] private VoidEventChannel     _onNightStarted;

        private DayCycleModel _model;

        public DayPhase CurrentPhase => _model.CurrentPhase;
        public int CurrentDay        => _model.CurrentDay;
        public float PhaseProgress   => _model.GetPhaseProgress();

        private void Awake()
        {
            float[] durations = _config != null ? _config.ToDurationsArray() : null;
            _model = new DayCycleModel(durations);

            _model.OnPhaseChanged   += HandlePhaseChanged;
            _model.OnNewDayStarted  += HandleNewDay;
        }

        private void OnDestroy()
        {
            _model.OnPhaseChanged  -= HandlePhaseChanged;
            _model.OnNewDayStarted -= HandleNewDay;
        }

        private void Update() => _model.Tick(UnityEngine.Time.deltaTime * _gameSpeed);

        private void HandlePhaseChanged(DayPhase phase)
        {
            _onPhaseChanged?.Raise(phase);

            if (phase == DayPhase.Morning_Prep) _onMorningStarted?.Raise();
            if (phase == DayPhase.Night_Dungeon) _onNightStarted?.Raise();
        }

        private void HandleNewDay(int day) => _onNewDayStarted?.Raise(day);
    }
}
