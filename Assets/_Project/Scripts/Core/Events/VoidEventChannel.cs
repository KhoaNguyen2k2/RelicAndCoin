using System;
using System.Collections.Generic;
using UnityEngine;

namespace RelicAndCoin.Core.Events
{
    [CreateAssetMenu(menuName = "RelicAndCoin/Events/Void Event Channel", fileName = "VoidEventChannel")]
    public sealed class VoidEventChannel : ScriptableObject
    {
        private readonly List<Action> _listeners = new();

        public void Raise()
        {
            for (int i = _listeners.Count - 1; i >= 0; i--)
                _listeners[i]?.Invoke();
        }

        public void RegisterListener(Action listener)
        {
            if (!_listeners.Contains(listener))
                _listeners.Add(listener);
        }

        public void UnregisterListener(Action listener) =>
            _listeners.Remove(listener);

#if UNITY_EDITOR
        private void OnEnable() => _listeners.Clear();
#endif
    }
}
