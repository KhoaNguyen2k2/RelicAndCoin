using RelicAndCoin.Core.Events;
using UnityEngine;

namespace RelicAndCoin.Core.Stats
{
    /// <summary>
    /// MonoBehaviour mỏng: khởi tạo CharacterStatsModel và bridge sự kiện sang SO Event Channels.
    /// </summary>
    public sealed class CharacterStatsController : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private BaseStatsConfigSO _config;

        [Header("Event Channels — HP")]
        [SerializeField] private FloatEventChannel _onHealthChanged;
        [SerializeField] private VoidEventChannel  _onDied;

        [Header("Event Channels — Stamina")]
        [SerializeField] private FloatEventChannel _onStaminaChanged;
        [SerializeField] private VoidEventChannel  _onFatigueEntered;
        [SerializeField] private VoidEventChannel  _onFatigueExited;

        [Header("Event Channels — Mana")]
        [SerializeField] private FloatEventChannel _onManaChanged;

        private CharacterStatsModel _model;

        // Expose để các hệ thống khác đọc trạng thái (Combat, UI, FSM)
        public float CurrentHP       => _model.CurrentHP;
        public float MaxHP           => _model.MaxHP;
        public float CurrentStamina  => _model.CurrentStamina;
        public float MaxStamina      => _model.MaxStamina;
        public float CurrentMana     => _model.CurrentMana;
        public float MaxMana         => _model.MaxMana;
        public bool  IsFatigued      => _model.IsFatigued;

        // ── Chỉ số chiến đấu (đọc từ BaseStatsConfigSO) ─────────────────
        public float AttackDamage => _config != null ? _config.AttackDamage : 10f;
        public float Defense      => _config != null ? _config.Defense      : 0f;
        public float CritChance   => _config != null ? _config.CritChance   : 0.05f;

        private void Awake()
        {
            _model = _config != null
                ? _config.CreateModel()
                : new CharacterStatsModel(100f, 100f, 100f);

            _model.OnHealthChanged       += OnHealthChanged;
            _model.OnStaminaChanged      += OnStaminaChanged;
            _model.OnManaChanged         += OnManaChanged;
            _model.OnFatigueStateChanged += OnFatigueStateChanged;
            _model.OnDied                += OnDied;
        }

        private void OnDestroy()
        {
            _model.OnHealthChanged       -= OnHealthChanged;
            _model.OnStaminaChanged      -= OnStaminaChanged;
            _model.OnManaChanged         -= OnManaChanged;
            _model.OnFatigueStateChanged -= OnFatigueStateChanged;
            _model.OnDied                -= OnDied;
        }

        private void Update() => _model.Tick(UnityEngine.Time.deltaTime);

        // ── Proxy API (delegate xuống model) ─────────────────────────
        public void TakeDamage(float amount)         => _model.TakeDamage(amount);
        public void Heal(float amount)               => _model.Heal(amount);
        public bool TryUseStamina(float amount)      => _model.TryUseStamina(amount);
        public bool TryUseMana(float amount)         => _model.TryUseMana(amount);
        public void AddModifier(StatModifier mod)    => _model.AddModifier(mod);
        public void RemoveModifiersFrom(object src)  => _model.RemoveModifiersBySource(src);

        // ── Handlers: C# Action → SO Event Channel ───────────────────
        // FloatEventChannel nhận tỷ lệ 0–1 để UI progress bar dùng trực tiếp
        private void OnHealthChanged(float cur, float max)
            => _onHealthChanged?.Raise(max > 0f ? cur / max : 0f);

        private void OnStaminaChanged(float cur, float max)
            => _onStaminaChanged?.Raise(max > 0f ? cur / max : 0f);

        private void OnManaChanged(float cur, float max)
            => _onManaChanged?.Raise(max > 0f ? cur / max : 0f);

        private void OnFatigueStateChanged(bool isFatigued)
        {
            if (isFatigued) _onFatigueEntered?.Raise();
            else            _onFatigueExited?.Raise();
        }

        private void OnDied() => _onDied?.Raise();
    }
}
