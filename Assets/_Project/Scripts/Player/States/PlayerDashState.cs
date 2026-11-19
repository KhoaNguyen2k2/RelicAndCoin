using RelicAndCoin.Core.StateMachine;
using UnityEngine;

namespace RelicAndCoin.Player.States
{
    /// <summary>
    /// Trạng thái lướt né: lanci theo hướng di chuyển, bất tử trong suốt thời gian lướt,
    /// sau đó vào hồi chiêu DashCooldown trước khi có thể lướt lại.
    /// </summary>
    public sealed class PlayerDashState : IState
    {
        private readonly PlayerContext             _ctx;
        private readonly StateMachine              _fsm;
        private readonly PlayerMovementConfigSO    _config;

        private float _elapsed;
        private float _cooldownRemaining;
        private Vector2 _dashDirection;

        private PlayerMoveState _moveState;

        public PlayerDashState(PlayerContext ctx, StateMachine fsm, PlayerMovementConfigSO config)
        {
            _ctx    = ctx;
            _fsm    = fsm;
            _config = config;
        }

        public void SetTransitions(PlayerMoveState moveState) => _moveState = moveState;

        public void Enter()
        {
            _elapsed = 0f;

            // Hướng lướt theo hướng di chuyển hiện tại; nếu không có hướng thì dùng hướng cuối cùng của Rigidbody
            _dashDirection = _ctx.MoveInput.sqrMagnitude > 0.01f
                ? _ctx.MoveInput.normalized
                : (_ctx.Rigidbody.linearVelocity.sqrMagnitude > 0.01f
                    ? _ctx.Rigidbody.linearVelocity.normalized
                    : Vector2.right);

            // Tiêu hao stamina khi bắt đầu lướt
            if (!_ctx.Stats.TryUseStamina(_config.DashStaminaCost))
            {
                _fsm.ChangeState(_moveState);
                return;
            }

            _ctx.IsInvulnerable = true;
            _ctx.Rigidbody.linearVelocity = _dashDirection * _config.DashSpeed;
        }

        public void Tick(float deltaTime)
        {
            _cooldownRemaining = Mathf.Max(0f, _cooldownRemaining - deltaTime);
            _elapsed += deltaTime;

            if (_elapsed >= _config.DashDuration)
            {
                _ctx.IsInvulnerable = false;
                _cooldownRemaining = _config.DashCooldown;
                _ctx.Rigidbody.linearVelocity = Vector2.zero;
                _fsm.ChangeState(_moveState);
            }
        }

        public void FixedTick(float fixedDeltaTime) { }

        public void Exit()
        {
            _ctx.IsInvulnerable = false;
        }
    }
}
