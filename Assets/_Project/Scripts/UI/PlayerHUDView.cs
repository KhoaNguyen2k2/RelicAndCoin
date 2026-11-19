using RelicAndCoin.Core.Events;
using RelicAndCoin.Core.Time;
using UnityEngine;
using UnityEngine.UI;

namespace RelicAndCoin.UI
{
    /// <summary>
    /// HUD người chơi: thanh trượt HP/Stamina/Mana, hiệu ứng cảnh báo khi Fatigue,
    /// số vàng và pha Ngày/Đêm. Lắng nghe Event Channels, không chứa business logic.
    /// </summary>
    public sealed class PlayerHUDView : MonoBehaviour
    {
        [Header("Event Channels")]
        [SerializeField] private FloatEventChannel _healthChannel;
        [SerializeField] private FloatEventChannel _staminaChannel;
        [SerializeField] private FloatEventChannel _manaChannel;
        [SerializeField] private IntEventChannel   _goldChannel;
        [SerializeField] private DayPhaseEventChannel _phaseChannel;
        [SerializeField] private VoidEventChannel  _fatigueEnteredChannel;
        [SerializeField] private VoidEventChannel  _fatigueExitedChannel;

        [Header("UI")]
        [SerializeField] private Slider _hpSlider;
        [SerializeField] private Slider _staminaSlider;
        [SerializeField] private Slider _manaSlider;
        [SerializeField] private GameObject _fatigueWarning;
        [SerializeField] private Text _goldText;
        [SerializeField] private Text _phaseText;

        private void Awake()
        {
            // Đảm bảo slider nhận tỷ lệ 0..1 bất kể giá trị chỉnh tay trong Inspector
            InitSlider(_hpSlider);
            InitSlider(_staminaSlider);
            InitSlider(_manaSlider);
        }

        private static void InitSlider(Slider slider)
        {
            if (slider == null) return;
            slider.minValue = 0f;
            slider.maxValue = 1f;
        }

        private void OnEnable()
        {
            _healthChannel?.RegisterListener(OnHealthChanged);
            _staminaChannel?.RegisterListener(OnStaminaChanged);
            _manaChannel?.RegisterListener(OnManaChanged);
            _goldChannel?.RegisterListener(OnGoldChanged);
            _phaseChannel?.RegisterListener(OnPhaseChanged);
            _fatigueEnteredChannel?.RegisterListener(OnFatigueEntered);
            _fatigueExitedChannel?.RegisterListener(OnFatigueExited);
        }

        private void OnDisable()
        {
            _healthChannel?.UnregisterListener(OnHealthChanged);
            _staminaChannel?.UnregisterListener(OnStaminaChanged);
            _manaChannel?.UnregisterListener(OnManaChanged);
            _goldChannel?.UnregisterListener(OnGoldChanged);
            _phaseChannel?.UnregisterListener(OnPhaseChanged);
            _fatigueEnteredChannel?.UnregisterListener(OnFatigueEntered);
            _fatigueExitedChannel?.UnregisterListener(OnFatigueExited);
        }

        private void OnHealthChanged(float ratio)  => SetSlider(_hpSlider,  ratio);
        private void OnStaminaChanged(float ratio) => SetSlider(_staminaSlider, ratio);
        private void OnManaChanged(float ratio)    => SetSlider(_manaSlider,    ratio);

        private void OnGoldChanged(int gold) => _goldText.text = gold.ToString();

        private void OnPhaseChanged(DayPhase phase)
        {
            _phaseText.text = phase switch
            {
                DayPhase.Morning_Prep   => "Sáng - Chuẩn bị",
                DayPhase.Day_ShopOpen   => "Ngày - Cửa hàng mở",
                DayPhase.Evening_Closing => "Chiều - Đóng cửa",
                DayPhase.Night_Dungeon  => "Đêm - Hầm ngục",
                _                       => phase.ToString()
            };
        }

        private void OnFatigueEntered()  => _fatigueWarning.SetActive(true);
        private void OnFatigueExited()   => _fatigueWarning.SetActive(false);

        private static void SetSlider(Slider slider, float ratio)
        {
            if (slider == null) return;
            slider.value = Mathf.Clamp01(ratio);
        }
    }
}
