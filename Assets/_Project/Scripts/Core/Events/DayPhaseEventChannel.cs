using UnityEngine;

namespace RelicAndCoin.Core.Events
{
    [CreateAssetMenu(menuName = "RelicAndCoin/Events/Day Phase Event Channel", fileName = "DayPhaseEventChannel")]
    public sealed class DayPhaseEventChannel : GameEventChannel<RelicAndCoin.Core.Time.DayPhase> { }
}
