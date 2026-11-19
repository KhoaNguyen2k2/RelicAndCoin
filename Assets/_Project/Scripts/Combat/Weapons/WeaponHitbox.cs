using System;
using System.Collections.Generic;
using UnityEngine;

namespace RelicAndCoin.Combat.Weapons
{
    /// <summary>
    /// Vùng va chạm của một nhát đánh. Gắn trên child GameObject có Collider2D (IsTrigger)
    /// của vũ khí. Theo dõi các IDamageable đã trúng trong một nhát chém để tránh gây
    /// sát thương 2 lần cùng một mục tiêu, và xóa danh sách khi bắt đầu nhát mới.
    /// </summary>
    public sealed class WeaponHitbox : MonoBehaviour
    {
        private readonly HashSet<IDamageable> _hitThisSwing = new();

        /// <summary>Sự kiện kích hoạt khi va chạm với một IDamageable chưa từng trúng trong nhát hiện tại.</summary>
        public event Action<IDamageable> OnNewHit;

        /// <summary>Gọi khi bắt đầu một nhát đánh mới để xóa danh sách đã trúng.</summary>
        public void BeginSwing() => _hitThisSwing.Clear();

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.TryGetComponent(out IDamageable target)) return;
            if (!_hitThisSwing.Add(target)) return; // đã trúng mục tiêu này trong nhát hiện tại
            OnNewHit?.Invoke(target);
        }
    }
}
