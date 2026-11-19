using RelicAndCoin.Core.Events;
using RelicAndCoin.Core.Time;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace RelicAndCoin.Core.Lighting
{
    /// <summary>
    /// Điều khiển đèn lồng ấm áp quanh nhân vật (Point Light 2D URP) tự động bật sáng khi trời tối (Evening / Night),
    /// đồng thời tự động kiến tạo lớp nền đất làng lợp ngói đá lặp lại (Tiled SpriteRenderer) nếu chưa có.
    /// </summary>
    public sealed class PlayerLanternAndAtmosphere : MonoBehaviour
    {
        private const string LanternGameObjectName = "PlayerLantern";
        private const string GroundGameObjectName = "AtmosphereGround_Tiled";
        private const string DefaultGroundSpritePath = "Assets/_Project/Art/VillageGround_Pixel.png";

        [Header("Event Channel (Input)")]
        [SerializeField] private DayPhaseEventChannel _phaseChannel;

        [Header("Đèn lồng URP (Point Light 2D)")]
        [SerializeField] private Light2D _lanternLight;
        [SerializeField] private Color _lanternColor = new Color(1.0f, 0.72f, 0.32f, 1f); // Vàng hổ phách ấm áp
        [SerializeField] [Min(0f)] private float _dayIntensity = 0.15f;
        [SerializeField] [Min(0f)] private float _eveningIntensity = 1.25f;
        [SerializeField] [Min(0f)] private float _nightIntensity = 1.85f;
        [SerializeField] [Min(0.1f)] private float _outerRadius = 4.8f;
        [SerializeField] [Min(0.05f)] private float _innerRadius = 0.8f;
        [SerializeField] private bool _enableFlicker = true;

        [Header("Mặt đất làng (Atmosphere Ground)")]
        [SerializeField] private Sprite _groundSprite;
        [SerializeField] private Vector2 _groundAreaSize = new Vector2(40f, 40f);
        [SerializeField] private int _groundSortingOrder = -100;

        private float _targetIntensity;
        private float _currentIntensity;
        private float _flickerOffset;

        private void Awake()
        {
            EnsureLanternLight();
            EnsureGroundAtmosphere();
            EnsureEventChannel();

            _targetIntensity = _dayIntensity;
            _currentIntensity = _dayIntensity;
            if (_lanternLight != null)
            {
                _lanternLight.intensity = _currentIntensity;
            }
        }

        private void OnEnable()
        {
            if (_phaseChannel != null)
            {
                _phaseChannel.RegisterListener(OnPhaseChanged);
            }
        }

        private void OnDisable()
        {
            if (_phaseChannel != null)
            {
                _phaseChannel.UnregisterListener(OnPhaseChanged);
            }
        }

        private void Update()
        {
            if (_lanternLight == null) return;

            // Chuyển đổi mượt mà cường độ sáng giữa ngày và đêm
            _currentIntensity = Mathf.MoveTowards(_currentIntensity, _targetIntensity, UnityEngine.Time.deltaTime * 1.5f);

            // Bập bùng nhẹ của ngọn lửa đèn dầu khi về đêm
            float flicker = 0f;
            if (_enableFlicker && _currentIntensity > 0.4f)
            {
                _flickerOffset += UnityEngine.Time.deltaTime * 6f;
                flicker = (Mathf.PerlinNoise(_flickerOffset, 0f) - 0.5f) * 0.15f;
            }

            _lanternLight.intensity = Mathf.Max(0f, _currentIntensity + flicker);
        }

        private void OnPhaseChanged(DayPhase phase)
        {
            switch (phase)
            {
                case DayPhase.Morning_Prep:
                case DayPhase.Day_ShopOpen:
                    _targetIntensity = _dayIntensity;
                    break;

                case DayPhase.Evening_Closing:
                    _targetIntensity = _eveningIntensity;
                    break;

                case DayPhase.Night_Dungeon:
                    _targetIntensity = _nightIntensity;
                    break;
            }
        }

        private void EnsureLanternLight()
        {
            if (_lanternLight != null) return;

            var childTransform = transform.Find(LanternGameObjectName);
            GameObject lanternObj;
            if (childTransform != null)
            {
                lanternObj = childTransform.gameObject;
            }
            else
            {
                lanternObj = new GameObject(LanternGameObjectName);
                lanternObj.transform.SetParent(transform, false);
                lanternObj.transform.localPosition = Vector3.zero;
            }

            _lanternLight = lanternObj.GetComponent<Light2D>();
            if (_lanternLight == null)
            {
                _lanternLight = lanternObj.AddComponent<Light2D>();
            }

            _lanternLight.lightType = Light2D.LightType.Point;
            _lanternLight.color = _lanternColor;
            _lanternLight.pointLightInnerRadius = _innerRadius;
            _lanternLight.pointLightOuterRadius = _outerRadius;
            _lanternLight.falloffIntensity = 0.5f;
            _lanternLight.intensity = _dayIntensity;
        }

        private void EnsureGroundAtmosphere()
        {
            var existingGround = GameObject.Find(GroundGameObjectName);
            if (existingGround != null) return;

            var groundObj = new GameObject(GroundGameObjectName);
            groundObj.transform.position = Vector3.zero;
            var sr = groundObj.AddComponent<SpriteRenderer>();

            if (_groundSprite == null)
            {
#if UNITY_EDITOR
                _groundSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(DefaultGroundSpritePath);
#endif
            }

            if (_groundSprite != null)
            {
                sr.sprite = _groundSprite;
                sr.drawMode = SpriteDrawMode.Tiled;
                sr.size = _groundAreaSize;
            }
            else
            {
                // Fallback nếu sprite chưa được sinh trong Editor
                sr.drawMode = SpriteDrawMode.Simple;
                sr.color = new Color(0.18f, 0.28f, 0.16f, 1f); // Nền cỏ sẫm màu
            }

            sr.sortingOrder = _groundSortingOrder;
        }

        private void EnsureEventChannel()
        {
            if (_phaseChannel != null) return;

            // Tìm DayNightLightingController trong scene để lấy tham chiếu channel nếu có
            var controller = FindAnyObjectByType<DayNightLightingController>();
            if (controller != null)
            {
                // Gán ngầm nếu cùng scene
            }
        }
    }
}
