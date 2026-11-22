using UnityEngine;
using RelicAndCoin.Core.StateMachine;

namespace RelicAndCoin.AI.Enemies.States
{
    /// <summary>
    /// Di chuyển theo các điểm nút CurrentPath do A* cung cấp.
    /// Vao AttackRange -> Attack. Khoi ngoai DetectRange -> quay lai Patrol.
    /// </summary>
    public sealed class EnemyChaseState : IState
    {
        private readonly EnemyContext _ctx;
        private EnemyPatrolState _patrolState;
        private EnemyAttackState _attackState;

        private float _dodgeCooldown;

        public EnemyChaseState(EnemyContext ctx) => _ctx = ctx;

        public void SetTransitions(EnemyPatrolState patrolState, EnemyAttackState attackState)
        {
            _patrolState = patrolState;
            _attackState = attackState;
        }

        public void Enter()
        {
            _ctx.DodgeTimer = 0f;
            _dodgeCooldown = 0f;
        }

        public void Tick(float deltaTime)
        {
            if (_dodgeCooldown > 0f)
                _dodgeCooldown -= deltaTime;

            if (_ctx.DistanceToPlayer > _ctx.DetectRange)
            {
                if (_patrolState != null)
                    _ctx.StateMachine.ChangeState(_patrolState);
                else
                    _ctx.StateMachine.ChangeState(new EnemyPatrolState(_ctx, _ctx.Rigidbody.position));
                return;
            }

            // Nới rộng tầm tấn công (+0.6f) trùng với EnemyAttackState để không bị "khoảng trống" khiến Slime lắc transitions
            float effectiveAttackRange = _ctx.Data.AttackRange + 0.6f;
            if (_ctx.DistanceToPlayer <= effectiveAttackRange)
            {
                if (_attackState != null)
                    _ctx.StateMachine.ChangeState(_attackState);
                else
                    _ctx.StateMachine.ChangeState(new EnemyAttackState(_ctx));
                return;
            }

            // Hành vi Nightmare: né đòn sang ngang khi Player đang vung đòn
            if (_ctx.CanDodgeAndPunish && _ctx.IsPlayerAttacking && _dodgeCooldown <= 0f && _ctx.DodgeTimer <= 0f)
            {
                if (_ctx.PlayerTransform != null)
                {
                    Vector2 toPlayer = ((Vector2)_ctx.PlayerTransform.position - _ctx.Rigidbody.position).normalized;
                    // Chọn ngẫu nhiên vector vuông góc bên trái (-y, x) hoặc bên phải (y, -x)
                    Vector2 perp = Random.value > 0.5f ? new Vector2(-toPlayer.y, toPlayer.x) : new Vector2(toPlayer.y, -toPlayer.x);
                    _ctx.DodgeDirection = perp;
                    _ctx.DodgeTimer = 0.25f;
                    _dodgeCooldown = 1.8f;
                }
            }

            FollowPath(deltaTime);
        }

        public void FixedTick(float fixedDeltaTime) { }

        public void Exit()
        {
            _ctx.DodgeTimer = 0f;
        }

        private void FollowPath(float deltaTime)
        {
            float speed = _ctx.ChaseSpeed;

            // Nếu người chơi bị Fatigue -> Quái vật dồn đòn trừng phạt (+35% tốc độ truy đuổi)
            if (_ctx.CanDodgeAndPunish && _ctx.IsPlayerFatigued)
                speed *= 1.35f;

            // Đang trong thời gian né đòn sang ngang
            if (_ctx.DodgeTimer > 0f)
            {
                _ctx.DodgeTimer -= deltaTime;
                _ctx.Rigidbody.MovePosition(_ctx.Rigidbody.position + _ctx.DodgeDirection * (speed * 1.5f) * Time.fixedDeltaTime);
                return;
            }

            // Di chuyển theo A* path nếu có
            if (_ctx.CurrentPath != null && _ctx.CurrentPath.Count > 0 && _ctx.PathIndex < _ctx.CurrentPath.Count)
            {
                var target = _ctx.CurrentPath[_ctx.PathIndex];
                var dir = (target - (Vector2)_ctx.Rigidbody.position).normalized;
                _ctx.Rigidbody.MovePosition(_ctx.Rigidbody.position + dir * speed * Time.fixedDeltaTime);

                if (Vector2.Distance(_ctx.Rigidbody.position, target) < 0.2f)
                    _ctx.PathIndex++;
            }
            else if (_ctx.PlayerTransform != null)
            {
                // Fallback direct chase nếu chưa có đường dẫn A*
                var dir = ((Vector2)_ctx.PlayerTransform.position - _ctx.Rigidbody.position).normalized;
                _ctx.Rigidbody.MovePosition(_ctx.Rigidbody.position + dir * speed * Time.fixedDeltaTime);
            }
        }
    }
}
