using System;
using System.Collections.Generic;

namespace RelicAndCoin.Combat.StatusEffects
{
    /// <summary>
    /// Pure C# — quản lý danh sách các hiệu ứng bất lợi đang gắn trên một nhân vật / quái vật.
    /// Cập nhật đếm ngược theo Tick(float deltaTime), tự động xóa khi hết hạn và phát sự kiện.
    /// Không phụ thuộc UnityEngine.
    /// </summary>
    public sealed class StatusEffectSystem
    {
        private readonly List<StatusEffectInstance> _active = new();

        /// <summary>Sự kiện khi một hiệu ứng mới được thêm.</summary>
        public event Action<StatusEffectInstance> OnEffectAdded;
        /// <summary>Sự kiện khi một hiệu ứng bị xóa (hết hạn hoặc gỡ tay).</summary>
        public event Action<StatusEffectInstance> OnEffectRemoved;

        /// <summary>Thêm một hiệu ứng vào danh sách đang hoạt động.</summary>
        public void AddEffect(StatusEffectInstance effect)
        {
            if (effect == null) return;
            _active.Add(effect);
            OnEffectAdded?.Invoke(effect);
        }

        /// <summary>Gỡ một hiệu ứng cụ thể khỏi danh sách.</summary>
        public bool RemoveEffect(StatusEffectInstance effect)
        {
            if (effect == null || !_active.Remove(effect)) return false;
            OnEffectRemoved?.Invoke(effect);
            return true;
        }

        /// <summary>Gỡ tất cả hiệu ứng của một loại nhất định.</summary>
        public int RemoveEffectsOfType(DebuffType type)
        {
            int removed = 0;
            for (int i = _active.Count - 1; i >= 0; i--)
            {
                if (_active[i].Type == type)
                {
                    var effect = _active[i];
                    _active.RemoveAt(i);
                    OnEffectRemoved?.Invoke(effect);
                    removed++;
                }
            }
            return removed;
        }

        /// <summary>
        /// Cập nhật toàn bộ hiệu ứng đang hoạt động. Gọi mỗi frame từ MonoBehaviour wrapper.
        /// Tự động xóa hiệu ứng hết hạn và trừ HP theo tick damage.
        /// </summary>
        /// <param name="deltaTime">Thời gian trôi qua kể từ lần gọi trước (giây).</param>
        /// <param name="applyDamage">Delegate thực hiện trừ HP cho mục tiêu (trả về true nếu mục tiêu còn sống).</param>
        public void Tick(float deltaTime, Func<float, bool> applyDamage)
        {
            for (int i = _active.Count - 1; i >= 0; i--)
            {
                var effect = _active[i];

                // Trừ HP theo tick damage mỗi giây (chỉ đối với các loại gây sátDamage liên tục).
                if (effect.TickDamage > 0f && applyDamage != null)
                {
                    float damageThisTick = effect.TickDamage * deltaTime;
                    if (!applyDamage(damageThisTick)) break; // mục tiêu đã chết, dừng xử lý
                }

                if (effect.Tick(deltaTime))
                {
                    _active.RemoveAt(i);
                    OnEffectRemoved?.Invoke(effect);
                }
            }
        }

        /// <summary>Mục tiêu đang bị chấn thương / đóng băng hay không.</summary>
        public bool IsStunned
        {
            get
            {
                for (int i = 0; i < _active.Count; i++)
                    if (_active[i].IsStunned) return true;
                return false;
            }
        }

        /// <summary>Tích lũy hệ số giảm tốc độ di chuyển từ tất cả hiệu ứng đang hoạt động.</summary>
        public float TotalMoveSpeedMultiplier
        {
            get
            {
                float multiplier = 1f;
                for (int i = 0; i < _active.Count; i++)
                    multiplier *= _active[i].MoveSpeedMultiplier;
                return multiplier;
            }
        }

        /// <summary>Tổng sátDamage mỗi giây từ tất cả hiệu ứng đang hoạt động.</summary>
        public float TotalTickDamagePerSecond
        {
            get
            {
                float total = 0f;
                for (int i = 0; i < _active.Count; i++)
                    total += _active[i].TickDamage;
                return total;
            }
        }

        /// <summary>Số lượng hiệu ứng đang hoạt động.</summary>
        public int ActiveCount => _active.Count;
    }
}
