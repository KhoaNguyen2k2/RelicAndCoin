using RelicAndCoin.Core.Difficulty;
using RelicAndCoin.Core.Save;
using RelicAndCoin.Player;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RelicAndCoin.UI
{
    /// <summary>
    /// Màn hình Menu chính: chọn nhân vật Kerral/Kaery, Class Chiến binh/Phù thủy,
    /// Độ khó, và nút New Game / Continue gọi qua SaveLoadManager.
    /// MonoBehaviour mỏng — chỉ thu thập lựa chọn người dùng và điều phối Lưu/Tải.
    /// </summary>
    public sealed class MainMenuView : MonoBehaviour
    {
        [Header("Chọn nhân vật")]
        [SerializeField] private Button _btnKerral;
        [SerializeField] private Button _btnKaery;
        [SerializeField] private GameObject _markerKerral;
        [SerializeField] private GameObject _markerKaery;

        [Header("Chọn Class")]
        [SerializeField] private Button _btnWarrior;
        [SerializeField] private Button _btnMage;
        [SerializeField] private GameObject _markerWarrior;
        [SerializeField] private GameObject _markerMage;

        [Header("Chọn Độ khó")]
        [SerializeField] private Button _btnNormal;
        [SerializeField] private Button _btnHard;
        [SerializeField] private Button _btnNightmare;
        [SerializeField] private GameObject _markerNormal;
        [SerializeField] private GameObject _markerHard;
        [SerializeField] private GameObject _markerNightmare;

        [Header("Nút hành động")]
        [SerializeField] private Button _btnNewGame;
        [SerializeField] private Button _btnContinue;
        [SerializeField] private Text _statusText;

        private CharacterGender _selectedGender = CharacterGender.Kerral;
        private PlayerClassType _selectedClass = PlayerClassType.Warrior;
        private DifficultyMode _selectedDifficulty = DifficultyMode.Normal;

        private void Awake()
        {
            // Luôn khôi phục tốc độ game khi vào Menu chính (tránh bị khựng ở timeScale = 0 từ Game Over)
            Time.timeScale = 1f;

            _btnKerral?.onClick.AddListener(() => SelectGender(CharacterGender.Kerral));
            _btnKaery?.onClick.AddListener(() => SelectGender(CharacterGender.Kaery));
            _btnWarrior?.onClick.AddListener(() => SelectClass(PlayerClassType.Warrior));
            _btnMage?.onClick.AddListener(() => SelectClass(PlayerClassType.Mage));
            _btnNormal?.onClick.AddListener(() => SelectDifficulty(DifficultyMode.Normal));
            _btnHard?.onClick.AddListener(() => SelectDifficulty(DifficultyMode.Hard));
            _btnNightmare?.onClick.AddListener(() => SelectDifficulty(DifficultyMode.Nightmare));
            _btnNewGame?.onClick.AddListener(OnNewGame);
            _btnContinue?.onClick.AddListener(OnContinue);

            _btnContinue.interactable = SaveLoadManager.SaveFileExists;

            UpdateMarkers();
        }

        private void Start()
        {
            InitializeButtonLabels();
            SetupActionButtonsStyle();
            UpdateMarkers();
        }

        private void InitializeButtonLabels()
        {
            SetButtonText(_btnKerral, "Kerral");
            SetButtonText(_btnKaery, "Kaery");
            SetButtonText(_btnWarrior, "Chiến binh");
            SetButtonText(_btnMage, "Phù thủy");
            SetButtonText(_btnNormal, "Bình thường");
            SetButtonText(_btnHard, "Khó");
            SetButtonText(_btnNightmare, "Ác mộng");
            SetButtonText(_btnNewGame, "Chơi Mới");
            SetButtonText(_btnContinue, "Tiếp Tục");
        }

        private void SetupActionButtonsStyle()
        {
            if (_btnNewGame != null)
            {
                ColorBlock cb = _btnNewGame.colors;
                cb.normalColor = new Color(0.35f, 0.85f, 0.45f, 1f);     // Xanh ngọc lục bảo mời gọi
                cb.highlightedColor = new Color(0.50f, 0.95f, 0.60f, 1f);
                cb.pressedColor = new Color(0.25f, 0.70f, 0.35f, 1f);
                _btnNewGame.colors = cb;
            }

            if (_btnContinue != null && _btnContinue.interactable)
            {
                ColorBlock cb = _btnContinue.colors;
                cb.normalColor = new Color(0.35f, 0.70f, 0.95f, 1f);     // Lam thạch anh
                cb.highlightedColor = new Color(0.50f, 0.85f, 1.0f, 1f);
                cb.pressedColor = new Color(0.25f, 0.55f, 0.80f, 1f);
                _btnContinue.colors = cb;
            }
        }

        private void OnDestroy()
        {
            _btnKerral?.onClick.RemoveAllListeners();
            _btnKaery?.onClick.RemoveAllListeners();
            _btnWarrior?.onClick.RemoveAllListeners();
            _btnMage?.onClick.RemoveAllListeners();
            _btnNormal?.onClick.RemoveAllListeners();
            _btnHard?.onClick.RemoveAllListeners();
            _btnNightmare?.onClick.RemoveAllListeners();
            _btnNewGame?.onClick.RemoveAllListeners();
            _btnContinue?.onClick.RemoveAllListeners();
        }

        private void SelectGender(CharacterGender gender)
        {
            _selectedGender = gender;
            UpdateMarkers();
        }

        private void SelectClass(PlayerClassType classType)
        {
            _selectedClass = classType;
            UpdateMarkers();
        }

        private void SelectDifficulty(DifficultyMode difficulty)
        {
            _selectedDifficulty = difficulty;
            UpdateMarkers();
        }

        private void OnNewGame()
        {
            var data = new SaveData
            {
                Difficulty = _selectedDifficulty,
                CurrentDay = 1,
                Gold = 0,
                PlayerHP = 100f,
                PlayerMana = 100f,
                PlayerStamina = 100f
            };

            if (SaveLoadManager.Save(data))
            {
                SetStatus("Đã lưu tiến bộ. Đang vào game...");
                LoadGameScene();
            }
            else
            {
                SetStatus("Lưu game mới thất bại.");
            }
        }

        private void OnContinue()
        {
            if (!SaveLoadManager.SaveFileExists)
            {
                SetStatus("Không có lưu game.");
                return;
            }

            var data = SaveLoadManager.Load();
            _selectedDifficulty = data.Difficulty;
            UpdateMarkers();
            SetStatus("Đang tiếp tục...");
            LoadGameScene();
        }

        private void LoadGameScene()
        {
            SceneManager.LoadScene("Gameplay");
        }

        private void SetStatus(string message)
        {
            if (_statusText != null) _statusText.text = message;
            Debug.Log($"[MainMenuView] {message}");
        }

        private void UpdateMarkers()
        {
            SetMarker(_markerKerral, _selectedGender == CharacterGender.Kerral);
            SetMarker(_markerKaery, _selectedGender == CharacterGender.Kaery);
            SetMarker(_markerWarrior, _selectedClass == PlayerClassType.Warrior);
            SetMarker(_markerMage, _selectedClass == PlayerClassType.Mage);
            SetMarker(_markerNormal, _selectedDifficulty == DifficultyMode.Normal);
            SetMarker(_markerHard, _selectedDifficulty == DifficultyMode.Hard);
            SetMarker(_markerNightmare, _selectedDifficulty == DifficultyMode.Nightmare);

            // Nổi bật màu sắc và độ sáng của tùy chọn đang chọn
            SetButtonVisualState(_btnKerral, _selectedGender == CharacterGender.Kerral);
            SetButtonVisualState(_btnKaery, _selectedGender == CharacterGender.Kaery);
            SetButtonVisualState(_btnWarrior, _selectedClass == PlayerClassType.Warrior);
            SetButtonVisualState(_btnMage, _selectedClass == PlayerClassType.Mage);
            SetButtonVisualState(_btnNormal, _selectedDifficulty == DifficultyMode.Normal);
            SetButtonVisualState(_btnHard, _selectedDifficulty == DifficultyMode.Hard);
            SetButtonVisualState(_btnNightmare, _selectedDifficulty == DifficultyMode.Nightmare);
        }

        private static void SetMarker(GameObject marker, bool active)
        {
            if (marker != null) marker.SetActive(active);
        }

        private static void SetButtonText(Button btn, string text)
        {
            if (btn == null) return;
            var label = btn.GetComponentInChildren<Text>(true);
            if (label != null)
            {
                label.text = text;
            }
        }

        private static void SetButtonVisualState(Button btn, bool isSelected)
        {
            if (btn == null) return;

            ColorBlock cb = btn.colors;
            if (isSelected)
            {
                cb.normalColor = new Color(1.0f, 0.88f, 0.40f, 1f);     // Vàng hoàng kim sáng nổi bật
                cb.highlightedColor = new Color(1.0f, 0.95f, 0.60f, 1f);
                cb.selectedColor = new Color(1.0f, 0.88f, 0.40f, 1f);
            }
            else
            {
                cb.normalColor = new Color(0.65f, 0.65f, 0.70f, 0.90f); // Tông xám dịu khi chưa chọn
                cb.highlightedColor = new Color(0.85f, 0.85f, 0.88f, 1f);
                cb.selectedColor = new Color(0.65f, 0.65f, 0.70f, 0.90f);
            }
            btn.colors = cb;

            if (btn.image != null)
            {
                btn.image.color = isSelected
                    ? new Color(1f, 1f, 1f, 1f)
                    : new Color(0.70f, 0.70f, 0.75f, 0.85f);
            }

            var label = btn.GetComponentInChildren<Text>(true);
            if (label != null)
            {
                label.color = isSelected
                    ? new Color(0.18f, 0.10f, 0.04f, 1f) // Chữ sẫm màu dễ đọc trên nền vàng sáng
                    : new Color(0.85f, 0.85f, 0.85f, 1f);
                label.fontStyle = isSelected ? FontStyle.Bold : FontStyle.Normal;
            }
        }
    }
}
