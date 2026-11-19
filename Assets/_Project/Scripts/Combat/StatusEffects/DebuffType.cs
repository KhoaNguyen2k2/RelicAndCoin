namespace RelicAndCoin.Combat.StatusEffects
{
    /// <summary>Loại hiệu ứng bất lợi có thể gắn trên nhân vật / quái vật.</summary>
    public enum DebuffType
    {
        Burn,     // Trừ HP mỗi giây
        Freeze,   // Đóng băng: không thể di chuyển hay tấn công
        Poison,   // Trừ HP mỗi giây (ticks chậm hơn Burn)
        Stun,     // Chấn thương: hoàn toàn không thể hành động
        Bleed     // Trừ HP mỗi giây, có thể xếp chồng
    }
}
