using UnityEngine;
using UnityEngine.Events;

namespace RelicAndCoin.Core.Events
{
    public sealed class IntEventListener : MonoBehaviour
    {
        [SerializeField] private IntEventChannel _channel;
        [SerializeField] private UnityEvent<int> _response;

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

        private void OnEventRaised(int value) => _response?.Invoke(value);
    }
}
