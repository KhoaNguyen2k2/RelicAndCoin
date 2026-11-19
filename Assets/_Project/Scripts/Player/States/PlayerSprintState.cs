using RelicAndCoin.Core.StateMachine;
using UnityEngine;

namespace RelicAndCoin.Player.States
{
    /// <summary>
    /// Trạng thái sprint: di chuyển nhanh hơn, tiêu hao Stamina theo thời gian.
    /// Tự động về MoveState nếu hết Stamina hoặc bị Fatigue.
    /// </summary>
    public sealed class PlayerSprintState : IState
    {
        private readonly PlayerContext             _ctx;
        private readonly StateMachine              _fsm;
        private readonly PlayerMovementConfigSO    _config;
        private readonly PlayerMoveState           _moveState;
        private readonly PlayerDashState           _dashState;

        public PlayerSprintState(PlayerContext ctx, StateMachine fsm, PlayerMovementConfigSO config,
                                 PlayerMoveState moveState, PlayerDashState dashState)
        {
            _ctx       = ctx;
            _fsm       = fsm;
            _config    = config;
            _moveState = moveState;
            _dashState = dashState;
        }

        public void Enter() { }

        public void Tick(float deltaTime)
        {
            // Không còn giữ sprint, hoặc bị fatigue → về MoveState
            if (!_ctx.IsSprintPressed || _ctx.Stats.IsFatigued)
            {
                _fsm.ChangeState(_moveState);
                return;
            }

            // Hết stamina → về MoveState
            if (!_ctx.Stats.TryUseStamina(_config.SprintStaminaCostPerSecond * deltaTime))
            {
                _fsm.ChangeState(_moveState);
                return;
            }

            if (_ctx.IsDashPressed)
            {
                _ctx.IsDashPressed = false;
                _fsm.ChangeState(_dashState);
                return;
            }
        }

        public void FixedTick(float fixedDeltaTime)
        {
            _ctx.Rigidbody.linearVelocity = _ctx.MoveInput.normalized * _config.SprintSpeed;
        }

        public void Exit() { }
    }
}
