using RelicAndCoin.Core.Events;
using RelicAndCoin.Core.Stats;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RelicAndCoin.UI
{
    /// <summary>
    /// Màn hình Game Over: hiển thị bảng phủ tối khi người chơi hết máu (0 HP),
    /// tạm dừng game (Time.timeScale = 0f) và cung cấp 2 nút "Chơi Lại" và "Về Menu Chính".
    /// Có khả năng tự dựng UI tự động trên Canvas nếu chưa được gán sẵn qua Inspector.
    /// </summary>
    public sealed class GameOverView : MonoBehaviour
    {
        [Header("UI Panels & Text")]
        [SerializeField] private GameObject _gameOverPanel;
        [SerializeField] private Text       _titleText;

        [Header("Buttons")]
        [SerializeField] private Button _btnRestart;
        [SerializeField] private Button _btnMainMenu;

        [Header("Event Channel (Tùy chọn)")]
        [SerializeField] private VoidEventChannel _onDiedChannel;

        private CharacterStatsController _playerStats;
        private bool _isGameOver = false;
        private static GameOverView _instance;

        public bool IsGameOver => _isGameOver;

        private void Awake()
        {
            // Không cho phép trùng lặp nhiều instance GameOverView khi chuyển / tải lại Scene
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;

            if (_gameOverPanel == null)
            {
                BuildProgrammaticUI();
            }

            _btnRestart?.onClick.AddListener(RestartGame);
            _btnMainMenu?.onClick.AddListener(ReturnToMainMenu);

            if (_gameOverPanel != null)
                _gameOverPanel.SetActive(false);
        }

        private void OnEnable()
        {
            _onDiedChannel?.RegisterListener(TriggerGameOver);
        }

        private void OnDisable()
        {
            _onDiedChannel?.UnregisterListener(TriggerGameOver);
        }

        private void OnDestroy()
        {
            // Dọn sạch sự kiện khi chuyển Scene, tránh listener trôi dỗi giữ đối tượng bị huỷ
            _onDiedChannel?.UnregisterListener(TriggerGameOver);
        }

        private void Start()
        {
            FindPlayerStats();
        }

        private void Update()
        {
            if (_isGameOver || _playerStats == null)
            {
                if (!_isGameOver) FindPlayerStats();
                return;
            }

            // Theo dõi HP trực tiếp từ CharacterStatsController của người chơi
            if (_playerStats.CurrentHP <= 0f)
            {
                TriggerGameOver();
            }
        }

        private void FindPlayerStats()
        {
            var playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
                _playerStats = playerObj.GetComponent<CharacterStatsController>();
        }

        /// <summary>
        /// Kích hoạt trạng thái Game Over: tạm dừng game và hiện bảng UI.
        /// </summary>
        public void TriggerGameOver()
        {
            if (_isGameOver) return;
            _isGameOver = true;

            Time.timeScale = 0f; // Tạm dừng mọi chuyển động trong game
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            if (_gameOverPanel != null)
                _gameOverPanel.SetActive(true);

            Debug.Log("<color=red><b>[GAME OVER]</b></color> Máu nhân vật về 0 HP! Đã dừng game và hiển thị màn hình kết thúc.");
        }

        /// <summary>
        /// Chơi lại màn hiện tại: khôi phục tốc độ và load lại Scene.
        /// </summary>
        public void RestartGame()
        {
            Time.timeScale = 1f;
            _isGameOver = false;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        /// <summary>
        /// Quay về Menu chính: khôi phục tốc độ và load Scene MainMenu.
        /// </summary>
        public void ReturnToMainMenu()
        {
            Time.timeScale = 1f;
            _isGameOver = false;
            SceneManager.LoadScene("MainMenu");
        }

        /// <summary>
        /// Tự động sinh cấu trúc UI hoàn chỉnh trên Canvas nếu chưa gán từ Inspector.
        /// </summary>
        private void BuildProgrammaticUI()
        {
            Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")
                            ?? Resources.GetBuiltinResource<Font>("Arial.ttf");

            // 1. Panel nền phủ tối màn hình
            var panelObj = new GameObject("GameOverPanel");
            panelObj.transform.SetParent(transform, false);
            var panelRt = panelObj.AddComponent<RectTransform>();
            panelRt.anchorMin = Vector2.zero;
            panelRt.anchorMax = Vector2.one;
            panelRt.sizeDelta = Vector2.zero;
            var panelImg = panelObj.AddComponent<Image>();
            panelImg.color = new Color(0.04f, 0.04f, 0.07f, 0.92f); // Phủ tối 92%

            // 2. Tiêu đề lớn màu đỏ "GAME OVER"
            var titleObj = new GameObject("TitleText");
            titleObj.transform.SetParent(panelObj.transform, false);
            var titleRt = titleObj.AddComponent<RectTransform>();
            titleRt.anchoredPosition = new Vector2(0f, 75f);
            titleRt.sizeDelta = new Vector2(600f, 90f);
            _titleText = titleObj.AddComponent<Text>();
            _titleText.text = "GAME OVER";
            _titleText.font = defaultFont;
            _titleText.fontSize = 54;
            _titleText.fontStyle = FontStyle.Bold;
            _titleText.alignment = TextAnchor.MiddleCenter;
            _titleText.color = new Color(0.96f, 0.18f, 0.18f, 1f);

            // 3. Dòng thông điệp phụ
            var subObj = new GameObject("SubText");
            subObj.transform.SetParent(panelObj.transform, false);
            var subRt = subObj.AddComponent<RectTransform>();
            subRt.anchoredPosition = new Vector2(0f, 15f);
            subRt.sizeDelta = new Vector2(600f, 40f);
            var subText = subObj.AddComponent<Text>();
            subText.text = "Bạn đã ngã xuống trong hầm ngục...";
            subText.font = defaultFont;
            subText.fontSize = 20;
            subText.alignment = TextAnchor.MiddleCenter;
            subText.color = new Color(0.85f, 0.85f, 0.85f, 0.85f);

            // 4. Nút "Chơi Lại" (Màu xanh lục ngọc)
            _btnRestart = CreateButton(
                panelObj.transform,
                "BtnRestart",
                "Chơi Lại",
                new Vector2(-115f, -65f),
                new Color(0.18f, 0.60f, 0.35f, 1f),
                defaultFont);

            // 5. Nút "Về Menu Chính" (Màu xanh xám)
            _btnMainMenu = CreateButton(
                panelObj.transform,
                "BtnMainMenu",
                "Về Menu Chính",
                new Vector2(115f, -65f),
                new Color(0.30f, 0.40f, 0.55f, 1f),
                defaultFont);

            _gameOverPanel = panelObj;
        }

        private static Button CreateButton(
            Transform parent,
            string name,
            string label,
            Vector2 position,
            Color normalColor,
            Font font)
        {
            var btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent, false);

            var rt = btnObj.AddComponent<RectTransform>();
            rt.anchoredPosition = position;
            rt.sizeDelta = new Vector2(190f, 48f);

            var img = btnObj.AddComponent<Image>();
            img.color = normalColor;

            var btn = btnObj.AddComponent<Button>();
            ColorBlock cb = btn.colors;
            cb.normalColor = normalColor;
            cb.highlightedColor = normalColor * 1.25f;
            cb.pressedColor = normalColor * 0.8f;
            btn.colors = cb;

            var textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);
            var textRt = textObj.AddComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.sizeDelta = Vector2.zero;

            var txt = textObj.AddComponent<Text>();
            txt.text = label;
            txt.font = font;
            txt.fontSize = 20;
            txt.fontStyle = FontStyle.Bold;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = Color.white;

            return btn;
        }

        /// <summary>
        /// Tự động đính kèm GameOverView vào Canvas trong Scene Gameplay nếu chưa tồn tại.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoEnsureInScene()
        {
            if (SceneManager.GetActiveScene().name != "Gameplay") return;

            if (Object.FindAnyObjectByType<GameOverView>() == null)
            {
                var canvas = Object.FindAnyObjectByType<Canvas>();
                if (canvas != null)
                {
                    var go = new GameObject("GameOverView");
                    go.transform.SetParent(canvas.transform, false);
                    go.AddComponent<GameOverView>();
                }
            }
        }
    }
}
