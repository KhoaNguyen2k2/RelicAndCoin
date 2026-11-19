using System.Collections.Generic;
using RelicAndCoin.Core.Events;
using RelicAndCoin.Core.Time;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace RelicAndCoin.Core.Lighting
{
    /// <summary>
    /// Điều chỉnh ánh sáng Global Light 2D theo pha ngày/đêm (màu sắc + cường độ).
    /// MonoBehaviour mỏng: lắng nghe DayPhaseEventChannel, không chứa logic game loop.
    /// </summary>
    [RequireComponent(typeof(Light2D))]
    public sealed class DayNightLightingController : MonoBehaviour
    {
        [Header("Event Channel (Input)")]
        [SerializeField] private DayPhaseEventChannel _phaseChannel;

        [Header("Ánh sáng Global Light 2D (URP)")]
        [SerializeField] private Light2D _globalLight;

        [Header("Cài đặt màu / cường độ theo từng pha")]
        [SerializeField] private PhaseLightConfig[] _phaseConfigs;

        [Header("Thời gian chuyển màu giữa hai pha (giây)")]
        [SerializeField] [Min(0f)] private float _transitionDuration = 2f;

        private readonly Dictionary<DayPhase, PhaseLightConfig> _configByPhase = new();
        private Color _targetColor;
        private float _targetIntensity;
        private float _transitionTimer;

        [System.Serializable]
        public struct PhaseLightConfig
        {
            public DayPhase Phase;
            public Color Color;
            [Min(0f)] public float Intensity;
        }

        private void Awake()
        {
            _globalLight ??= GetComponent<Light2D>();

            foreach (var cfg in _phaseConfigs)
                _configByPhase[cfg.Phase] = cfg;
        }

        private void OnEnable()
        {
            _phaseChannel?.RegisterListener(OnPhaseChanged);
            ApplyConfig(GetConfigForPhase(DayPhase.Morning_Prep), immediate: true);
        }

        private void OnDisable() => _phaseChannel?.UnregisterListener(OnPhaseChanged);

        private void OnPhaseChanged(DayPhase phase)
        {
            ApplyConfig(GetConfigForPhase(phase), immediate: false);
        }

        private void Update()
        {
            if (_globalLight == null) return;

            if (_transitionTimer > 0f)
            {
                _transitionTimer = Mathf.Max(0f, _transitionTimer - UnityEngine.Time.unscaledDeltaTime);
                float t = 1f - (_transitionTimer / _transitionDuration);
                t = t * t; // ease-in-out đơn giản

                _globalLight.color = Color.Lerp(_globalLight.color, _targetColor, t);
                _globalLight.intensity = Mathf.Lerp(_globalLight.intensity, _targetIntensity, t);
            }
        }

        private PhaseLightConfig GetConfigForPhase(DayPhase phase)
        {
            return _configByPhase.TryGetValue(phase, out var cfg) ? cfg : default;
        }

        private void ApplyConfig(PhaseLightConfig config, bool immediate)
        {
            if (_globalLight == null) return;

            _targetColor = config.Color;
            _targetIntensity = config.Intensity;

            if (immediate || _transitionDuration <= 0f)
            {
                _globalLight.color = _targetColor;
                _globalLight.intensity = _targetIntensity;
                _transitionTimer = 0f;
            }
            else
            {
                _transitionTimer = _transitionDuration;
            }
        }
    }
}
