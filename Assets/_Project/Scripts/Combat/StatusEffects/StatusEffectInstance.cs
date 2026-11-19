namespace RelicAndCoin.Combat.StatusEffects
{
    /// <summary>
    /// Một instance của hiệu ứng bất lợi đang đang tồn tại trên một mục tiêu.
    /// Lưu loại hiệu ứng, sát thương mỗi giây, thời gian còn lại và các cờ khống chế.
    /// </summary>
    public sealed class StatusEffectInstance
    {
        public DebuffType Type { get; }
        public float TickDamage { get; }
        public float RemainingDuration { get; private set; }
        public bool IsStunned => Type == DebuffType.Stun || Type == DebuffType.Freeze;
        public float MoveSpeedMultiplier { get; }

        public StatusEffectInstance(DebuffType type, float tickDamage, float duration, float moveSpeedMultiplier = 1f)
        {
            Type = type;
            TickDamage = tickDamage;
            RemainingDuration = duration;
            MoveSpeedMultiplier = moveSpeedMultiplier;
        }

        /// <summary>Giảm thời gian còn lại. Trả về true nếu hiệu ứng đã hết hạn và nên được xóa.</summary>
        public bool Tick(float deltaTime)
        {
            RemainingDuration -= deltaTime;
            return RemainingDuration <= 0f;
        }
    }
}
