using UnityEngine;
using UnityEngine.Events;

namespace RelicAndCoin.Core.Events
{
    public sealed class VoidEventListener : MonoBehaviour
    {
        [SerializeField] private VoidEventChannel _channel;
        [SerializeField] private UnityEvent _response;

        private void OnEnable()
        {
            if (_channel != null)
                _channel.RegisterListener(OnEventRaised);
        }

        private void OnDisable()
        {
            if (_channel != null)
                _channel.UnregisterListener(OnEventRaised);
        }

        private void OnEventRaised() => _response?.Invoke();
    }
}
