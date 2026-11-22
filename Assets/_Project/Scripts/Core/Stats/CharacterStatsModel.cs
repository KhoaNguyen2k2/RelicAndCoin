using System;
using System.Collections.Generic;

namespace RelicAndCoin.Core.Stats
{
    /// <summary>
    /// Pure C# — không phụ thuộc UnityEngine.
    /// MonoBehaviour wrapper gọi Tick() mỗi frame và đăng ký vào các Action để nhận event.
    /// </summary>
    public sealed class CharacterStatsModel
    {
        // ── Chỉ số tối đa (tính từ base + modifiers) ──────────────────
        public float MaxHP       { get; private set; }
        public float MaxStamina  { get; private set; }
        public float MaxMana     { get; private set; }

        // ── Chỉ số hiện tại ───────────────────────────────────────────
        public float CurrentHP      { get; private set; }
        public float CurrentStamina { get; private set; }
        public float CurrentMana    { get; private set; }

        // ── Hệ thống Fatigue ──────────────────────────────────────────
        public bool IsFatigued { get; private set; }

        // Thời gian khóa hồi Stamina sau khi về 0 (giây)
        private readonly float _fatigueDuration;
        private float _fatigueTimer;

        // ── Hồi tự nhiên mỗi giây ────────────────────────────────────
        private readonly float _staminaRegenPerSecond;
        private readonly float _manaRegenPerSecond;
        private readonly float _hpRegenPerSecond;

        // Thời gian trễ trước khi hồi Stamina (reset mỗi lần TryUseStamina)
        private readonly float _staminaRegenDelay;
        private float _staminaRegenDelayTimer;

        // Thời gian trễ trước khi hồi HP (reset mỗi lần nhận damage)
        private readonly float _hpRegenDelay;
        private float _hpRegenTimer;

        // ── Modifier list ─────────────────────────────────────────────
        private readonly List<StatModifier> _modifiers = new();

        // ── Base stats (lưu để tính lại khi modifier thay đổi) ───────
        private readonly float _baseMaxHP;
        private readonly float _baseMaxStamina;
        private readonly float _baseMaxMana;

        // ── Events nội bộ (dùng C# Action theo CLAUDE.md) ─────────────
        public event Action<float, float> OnHealthChanged;   // (current, max)
        public event Action<float, float> OnStaminaChanged;  // (current, max)
        public event Action<float, float> OnManaChanged;     // (current, max)
        public event Action<bool>         OnFatigueStateChanged; // (isFatigued)
        public event Action               OnDied;

        // ── Constructor ───────────────────────────────────────────────
        public CharacterStatsModel(
            float baseMaxHP,
            float baseMaxStamina,
            float baseMaxMana,
            float fatigueDuration       = 3f,
            float staminaRegenPerSecond = 10f,
            float manaRegenPerSecond    = 5f,
            float staminaRegenDelay     = 1f,
            float hpRegenPerSecond      = 20f,
            float hpRegenDelay          = 2f)
        {
            _baseMaxHP             = baseMaxHP;
            _baseMaxStamina        = baseMaxStamina;
            _baseMaxMana           = baseMaxMana;
            _fatigueDuration       = fatigueDuration;
            _staminaRegenPerSecond = staminaRegenPerSecond;
            _manaRegenPerSecond    = manaRegenPerSecond;
            _staminaRegenDelay     = staminaRegenDelay;
            _hpRegenPerSecond      = hpRegenPerSecond;
            _hpRegenDelay          = hpRegenDelay;

            RecalculateMaxStats();
            CurrentHP      = MaxHP;
            CurrentStamina = MaxStamina;
            CurrentMana    = MaxMana;
        }

        // ── Tick (gọi mỗi frame từ MonoBehaviour) ─────────────────────
        public void Tick(float deltaTime)
        {
            TickFatigue(deltaTime);
            TickRegen(deltaTime);
        }

        private void TickFatigue(float deltaTime)
        {
            if (!IsFatigued) return;

            _fatigueTimer -= deltaTime;
            if (_fatigueTimer <= 0f)
            {
                IsFatigued = false;
                OnFatigueStateChanged?.Invoke(false);
            }
        }

        private void TickRegen(float deltaTime)
        {
            // Đếm ngược delay trước khi hồi Stamina
            if (_staminaRegenDelayTimer > 0f)
                _staminaRegenDelayTimer -= deltaTime;

            if (!IsFatigued && _staminaRegenDelayTimer <= 0f && CurrentStamina < MaxStamina)
                ModifyStamina(_staminaRegenPerSecond * deltaTime);

            if (CurrentMana < MaxMana)
                ModifyMana(_manaRegenPerSecond * deltaTime);

            // Đếm ngược delay trước khi hồi HP (reset mỗi lần nhận damage)
            if (_hpRegenTimer > 0f)
                _hpRegenTimer -= deltaTime;

            if (_hpRegenTimer <= 0f && CurrentHP < MaxHP)
                SetHP(CurrentHP + _hpRegenPerSecond * deltaTime);
        }

        // ── API công khai: nhận damage / tiêu tốn tài nguyên ──────────

        public void TakeDamage(float amount)
        {
            if (amount <= 0f) return;
            // Reset bộ đếm ngưỡng hồi HP: chỉ hồi sau khi không nhận damage trong một khoảng thời gian
            _hpRegenTimer = _hpRegenDelay;
            SetHP(CurrentHP - amount);
        }

        public void Heal(float amount)
        {
            if (amount <= 0f) return;
            SetHP(CurrentHP + amount);
        }

        /// <returns>false nếu không đủ Stamina (dùng để block skill/sprint)</returns>
        public bool TryUseStamina(float amount)
        {
            if (IsFatigued || CurrentStamina < amount) return false;
            ModifyStamina(-amount);
            // Reset delay mỗi lần tiêu hao để regen không bù ngay lập tức
            _staminaRegenDelayTimer = _staminaRegenDelay;
            return true;
        }

        /// <returns>false nếu không đủ Mana</returns>
        public bool TryUseMana(float amount)
        {
            if (CurrentMana < amount) return false;
            ModifyMana(-amount);
            return true;
        }

        // ── Modifier API ──────────────────────────────────────────────

        public void AddModifier(StatModifier modifier)
        {
            _modifiers.Add(modifier);
            RecalculateMaxStats();
        }

        public void RemoveModifiersBySource(object source)
        {
            _modifiers.RemoveAll(m => m.Source == source);
            RecalculateMaxStats();
        }

        // ── Nội bộ ────────────────────────────────────────────────────

        private void SetHP(float value)
        {
            float prev = CurrentHP;
            CurrentHP = Math.Clamp(value, 0f, MaxHP);

            if (Math.Abs(CurrentHP - prev) < 1e-4f) return;

            OnHealthChanged?.Invoke(CurrentHP, MaxHP);
            if (CurrentHP <= 0f) OnDied?.Invoke();
        }

        private void ModifyStamina(float delta)
        {
            float prev = CurrentStamina;
            CurrentStamina = Math.Clamp(CurrentStamina + delta, 0f, MaxStamina);

            if (Math.Abs(CurrentStamina - prev) < 1e-4f) return;

            OnStaminaChanged?.Invoke(CurrentStamina, MaxStamina);

            if (CurrentStamina <= 0f && !IsFatigued)
                EnterFatigue();
        }

        private void ModifyMana(float delta)
        {
            float prev = CurrentMana;
            CurrentMana = Math.Clamp(CurrentMana + delta, 0f, MaxMana);

            if (Math.Abs(CurrentMana - prev) < 1e-4f) return;

            OnManaChanged?.Invoke(CurrentMana, MaxMana);
        }

        private void EnterFatigue()
        {
            IsFatigued    = true;
            _fatigueTimer = _fatigueDuration;
            OnFatigueStateChanged?.Invoke(true);
        }

        private void RecalculateMaxStats()
        {
            MaxHP      = ApplyModifiers(_baseMaxHP,      StatType.MaxHP);
            MaxStamina = ApplyModifiers(_baseMaxStamina, StatType.MaxStamina);
            MaxMana    = ApplyModifiers(_baseMaxMana,    StatType.MaxMana);

            // Clamp hiện tại để không vượt max mới
            CurrentHP      = Math.Min(CurrentHP,      MaxHP);
            CurrentStamina = Math.Min(CurrentStamina, MaxStamina);
            CurrentMana    = Math.Min(CurrentMana,    MaxMana);
        }

        private float ApplyModifiers(float baseValue, StatType type)
        {
            float flat    = baseValue;
            float percent = 0f;

            foreach (StatModifier m in _modifiers)
            {
                if (m.Stat != type) continue;
                if (m.Type == ModifierType.Flat)    flat    += m.Value;
                if (m.Type == ModifierType.Percent) percent += m.Value;
            }

            // Flat trước, Percent sau: (base + flat) × (1 + sumPercent)
            return flat * (1f + percent);
        }
    }
}
