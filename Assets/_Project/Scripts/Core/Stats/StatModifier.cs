using System;

namespace RelicAndCoin.Core.Stats
{
    public enum ModifierType { Flat, Percent }

    [Serializable]
    public struct StatModifier
    {
        public StatType    Stat;
        public ModifierType Type;
        public float       Value;

        // Source dùng để nhận diện và xóa modifier đúng (buff hết hạn, tháo trang bị)
        [NonSerialized]
        public object Source;

        public StatModifier(StatType stat, ModifierType type, float value, object source = null)
        {
            Stat   = stat;
            Type   = type;
            Value  = value;
            Source = source;
        }

        /// <summary>Áp modifier lên giá trị base đã cộng Flat, hoặc lên Flat total.</summary>
        /// <param name="baseValue">Giá trị gốc (chưa nhân Percent).</param>
        public float Apply(float baseValue) => Type switch
        {
            ModifierType.Flat    => baseValue + Value,
            ModifierType.Percent => baseValue * (1f + Value),
            _                    => baseValue
        };
    }
}
