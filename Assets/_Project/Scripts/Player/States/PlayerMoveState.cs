using RelicAndCoin.Core.StateMachine;
using UnityEngine;

namespace RelicAndCoin.Player.States
{
    /// <summary>
    /// Trạng thái di chuyển với tốc độ đi bộ. Nhập Sprint → SprintState; nhấn Dash → DashState.
    /// </summary>
    public sealed class PlayerMoveState : IState
    {
        private readonly PlayerContext             _ctx;
        private readonly StateMachine              _fsm;
        private readonly PlayerMovementConfigSO    _config;

        private PlayerIdleState   _idleState;
        private PlayerSprintState _sprintState;
        private PlayerDashState   _dashState;

        public PlayerMoveState(PlayerContext ctx, StateMachine fsm, PlayerMovementConfigSO config)
        {
            _ctx    = ctx;
            _fsm    = fsm;
            _config = config;
        }

        public void SetTransitions(PlayerIdleState idleState, PlayerSprintState sprintState, PlayerDashState dashState)
        {
            _idleState   = idleState;
            _sprintState = sprintState;
            _dashState   = dashState;
        }

        public void Enter() { }

        public void Tick(float deltaTime)
        {
            if (_ctx.MoveInput.sqrMagnitude <= 0.01f)
            {
                _fsm.ChangeState(_idleState);
                return;
            }

            if (_ctx.IsDashPressed)
            {
                _ctx.IsDashPressed = false;
                _fsm.ChangeState(_dashState);
                return;
            }

            if (_ctx.IsSprintPressed)
                _fsm.ChangeState(_sprintState);
        }

        public void FixedTick(float fixedDeltaTime)
        {
            _ctx.Rigidbody.linearVelocity = _ctx.MoveInput.normalized * _config.WalkSpeed;
        }

        public void Exit()
        {
            _ctx.Rigidbody.linearVelocity = Vector2.zero;
        }
    }
}
