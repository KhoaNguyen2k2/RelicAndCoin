using RelicAndCoin.Core.Events;
using RelicAndCoin.Core.Stats;
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

        private CharacterStatsController _playerStats;
        private int _currentGold;
        private DayPhase _currentPhase;

        private void Awake()
        {
            // Tự động tìm theo tên nếu Inspector chưa gán
            _hpSlider    = _hpSlider    ?? FindSliderByName("HP_Slider");
            _staminaSlider = _staminaSlider ?? FindSliderByName("Stamina_Slider");
            _manaSlider  = _manaSlider  ?? FindSliderByName("Mana_Slider");

            // Tự động tìm đối tượng cảnh báo kiệt sức nếu chưa gán
            _fatigueWarning = _fatigueWarning ?? FindGameObjectByName("Fatigue_Warning");
            if (_fatigueWarning != null)
            {
                // Đổi text bên trong thành "KIỆT SỨC!" và ẩn ngay lập tức
                var warningText = _fatigueWarning.GetComponentInChildren<Text>(true);
                if (warningText != null) warningText.text = "KIỆT SỨC!";
                _fatigueWarning.SetActive(false);
            }

            // Tự động tìm text vàng & pha thời gian (kể cả biến thể Legacy)
            _goldText  = _goldText  ?? FindTextByName("Gold_Text")   ?? FindTextByName("Gold_Text(Legacy)");
            _phaseText = _phaseText ?? FindTextByName("Phase_Text")  ?? FindTextByName("Phase_Text(Legacy)");

            // Cập nhật ngay nội dung thực tế từ đầu màn chơi, tuyệt đối không để chữ "New Text"
            RefreshGoldText();
            RefreshPhaseText();

            // Đảm bảo slider nhận tỷ lệ 0..1 bất kể giá trị chỉnh tay trong Inspector
            InitSlider(_hpSlider);
            InitSlider(_staminaSlider);
            InitSlider(_manaSlider);

            // Xóa lề thừa của Fill Area / Fill để thanh đầy khít 100% khung nền
            FixSliderFillRect(_hpSlider);
            FixSliderFillRect(_staminaSlider);
            FixSliderFillRect(_manaSlider);

            // Tô màu phân biệt rõ ràng cho từng thanh
            SetSliderFillColor(_hpSlider,    new Color32(220, 50, 50, 255));   // Đỏ – HP
            SetSliderFillColor(_staminaSlider, new Color32(60, 180, 75, 255)); // Xanh lá – Stamina
            SetSliderFillColor(_manaSlider,  new Color32(60, 130, 220, 255));  // Xanh dương – Mana
        }

        private GameObject FindGameObjectByName(string name)
        {
            var found = transform.Find(name);
            return found != null ? found.gameObject : null;
        }

        private Text FindTextByName(string name)
        {
            var found = transform.Find(name);
            return found != null ? found.GetComponent<Text>() : null;
        }

        private Slider FindSliderByName(string name)
        {
            var found = transform.Find(name);
            return found != null ? found.GetComponent<Slider>() : null;
        }

        private static void FixSliderFillRect(Slider slider)
        {
            if (slider == null) return;
            var fillArea = slider.transform.Find("Fill Area");
            if (fillArea != null)
            {
                var rt = fillArea.GetComponent<RectTransform>();
                if (rt != null) { rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero; }
                var fill = fillArea.Find("Fill");
                if (fill != null)
                {
                    var fillRt = fill.GetComponent<RectTransform>();
                    if (fillRt != null) { fillRt.offsetMin = Vector2.zero; fillRt.offsetMax = Vector2.zero; }
                }
            }
        }

        private static void SetSliderFillColor(Slider slider, Color color)
        {
            if (slider == null) return;
            var fillArea = slider.transform.Find("Fill Area");
            if (fillArea == null) return;
            var fill = fillArea.Find("Fill");
            if (fill == null) return;
            var image = fill.GetComponent<Image>();
            if (image != null) image.color = color;
        }

        private void Start()
        {
            FindPlayerStats();
        }

        private void Update()
        {
            if (_hpSlider == null && _staminaSlider == null && _manaSlider == null) return;

            // Dự phòng: cập nhật trực tiếp từ CharacterStatsController của Player nếu Event Channel chưa liên kết
            if (_playerStats == null)
            {
                FindPlayerStats();
            }

            if (_playerStats != null)
            {
                if (_hpSlider != null)
                {
                    float hpRatio = _playerStats.MaxHP > 0f ? _playerStats.CurrentHP / _playerStats.MaxHP : 0f;
                    SetSlider(_hpSlider, hpRatio);
                }

                if (_staminaSlider != null)
                {
                    float staRatio = _playerStats.MaxStamina > 0f ? _playerStats.CurrentStamina / _playerStats.MaxStamina : 0f;
                    SetSlider(_staminaSlider, staRatio);
                }

                if (_manaSlider != null)
                {
                    float manaRatio = _playerStats.MaxMana > 0f ? _playerStats.CurrentMana / _playerStats.MaxMana : 0f;
                    SetSlider(_manaSlider, manaRatio);
                }

                // Cảnh báo kiệt sức: hiện khi hết Stamina, ẩn khi hồi lại
                UpdateFatigueWarning(_playerStats.IsFatigued);
            }

            // Cập nhật số vàng và pha thời gian mỗi khung hình (dự phòng nếu Event Channel chưa gán)
            RefreshGoldText();
            RefreshPhaseText();
        }

        private void UpdateFatigueWarning(bool isFatigued)
        {
            if (_fatigueWarning == null) return;
            _fatigueWarning.SetActive(isFatigued);
        }

        private void FindPlayerStats()
        {
            var playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
                _playerStats = playerObj.GetComponent<CharacterStatsController>();
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

        private void OnGoldChanged(int gold)
        {
            _currentGold = gold;
            _goldText.text = $"Vàng: {gold}G";
        }

        private void OnPhaseChanged(DayPhase phase)
        {
            _currentPhase = phase;
            _phaseText.text = PhaseLabel(phase);
        }

        private void RefreshGoldText()
        {
            if (_goldText == null) return;
            _goldText.text = $"Vàng: {_currentGold}G";
        }

        private void RefreshPhaseText()
        {
            if (_phaseText == null) return;
            _phaseText.text = PhaseLabel(_currentPhase);
        }

        private static string PhaseLabel(DayPhase phase)
        {
            return phase switch
            {
                DayPhase.Morning_Prep   => "Buổi Sáng",
                DayPhase.Day_ShopOpen   => "Ban Ngày",
                DayPhase.Evening_Closing => "Chiều Tà",
                DayPhase.Night_Dungeon  => "Ban Đêm",
                _                       => phase.ToString()
            };
        }

        private void OnFatigueEntered()  => UpdateFatigueWarning(true);
        private void OnFatigueExited()   => UpdateFatigueWarning(false);

        private static void SetSlider(Slider slider, float ratio)
        {
            if (slider == null) return;
            slider.value = Mathf.Clamp01(ratio);
        }
    }
}
