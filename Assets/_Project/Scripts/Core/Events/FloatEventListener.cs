using UnityEngine;
using UnityEngine.Events;

namespace RelicAndCoin.Core.Events
{
    public sealed class FloatEventListener : MonoBehaviour
    {
        [SerializeField] private FloatEventChannel _channel;
        [SerializeField] private UnityEvent<float> _response;

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

        private void OnEventRaised(float value) => _response?.Invoke(value);
    }
}
