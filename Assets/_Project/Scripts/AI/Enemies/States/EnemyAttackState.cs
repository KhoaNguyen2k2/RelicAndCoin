using UnityEngine;
using RelicAndCoin.Core.StateMachine;

namespace RelicAndCoin.AI.Enemies.States
{
    /// <summary>
    /// Thực hiện đòn đánh và chờ thời gian hồi chiêu. Sau đó quay lại Chase.
    /// Hỗ trợ Zero-Allocation State Machine và giảm 30% hồi chiêu khi trừng phạt người chơi Fatigue.
    /// </summary>
    public sealed class EnemyAttackState : IState
    {
        private readonly EnemyContext _ctx;
        private EnemyChaseState _chaseState;
        private float _cooldownTimer;

        public EnemyAttackState(EnemyContext ctx) => _ctx = ctx;

        public void SetTransitions(EnemyChaseState chaseState)
        {
            _chaseState = chaseState;
        }

        public void Enter() => _cooldownTimer = CalculateCooldown();

        public void Tick(float deltaTime)
        {
            _cooldownTimer -= deltaTime;

            if (_ctx.DistanceToPlayer > _ctx.Data.AttackRange)
            {
                if (_chaseState != null)
                    _ctx.StateMachine.ChangeState(_chaseState);
                return;
            }

            if (_cooldownTimer <= 0f)
            {
                PerformAttack();
                _cooldownTimer = CalculateCooldown();
            }
        }

        public void FixedTick(float fixedDeltaTime) { }

        public void Exit() { }

        private float CalculateCooldown()
        {
            float cooldown = _ctx.AttackCooldown;

            // Giảm thời gian chờ thêm 30% khi bật chế độ né/trừng phạt và người chơi đang bị kiệt sức (Fatigued)
            if (_ctx.CanDodgeAndPunish && _ctx.IsPlayerFatigued)
            {
                cooldown *= 0.7f;
            }

            return cooldown;
        }

        private void PerformAttack()
        {
            // TODO: Player nhận DamageResult qua WeaponHitbox/DamageCalculator ở lượt sau.
            // Hiện tại chỉ đánh dấu đã đánh, không cấp phát GC.
            Debug.Log($"[{_ctx.Data.EnemyName}] giáng đòn tại {_ctx.Rigidbody.position}");
        }
    }
}
