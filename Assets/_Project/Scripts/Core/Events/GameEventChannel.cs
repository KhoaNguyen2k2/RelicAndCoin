using System;
using System.Collections.Generic;
using UnityEngine;

namespace RelicAndCoin.Core.Events
{
    public abstract class GameEventChannel<T> : ScriptableObject
    {
        private readonly List<Action<T>> _listeners = new();

        public void Raise(T value)
        {
            // Iterate backwards so listeners can safely unregister during callback
            for (int i = _listeners.Count - 1; i >= 0; i--)
                _listeners[i]?.Invoke(value);
        }

        public void RegisterListener(Action<T> listener)
        {
            if (!_listeners.Contains(listener))
                _listeners.Add(listener);
        }

        public void UnregisterListener(Action<T> listener) =>
            _listeners.Remove(listener);

#if UNITY_EDITOR
        // Clears listeners when scripts reload in Editor to prevent stale references
        private void OnEnable() => _listeners.Clear();
#endif
    }
}
