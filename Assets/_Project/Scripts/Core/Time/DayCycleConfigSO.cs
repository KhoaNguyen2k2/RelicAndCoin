using UnityEngine;

namespace RelicAndCoin.Core.Time
{
    [CreateAssetMenu(menuName = "RelicAndCoin/Config/Day Cycle Config", fileName = "DayCycleConfig")]
    public sealed class DayCycleConfigSO : ScriptableObject
    {
        [Header("Thời lượng mỗi pha (giây)")]
        [Min(1f)] public float MorningPrepDuration   = 60f;
        [Min(1f)] public float DayShopOpenDuration   = 180f;
        [Min(1f)] public float EveningClosingDuration = 60f;
        [Min(1f)] public float NightDungeonDuration  = 120f;

        // Trả về array theo đúng thứ tự DayPhase enum (0=Morning, 1=Day, 2=Evening, 3=Night)
        public float[] ToDurationsArray() => new[]
        {
            MorningPrepDuration,
            DayShopOpenDuration,
            EveningClosingDuration,
            NightDungeonDuration,
        };
    }
}
