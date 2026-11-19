using RelicAndCoin.Core.StateMachine;
using UnityEngine;

namespace RelicAndCoin.Player.States
{
    /// <summary>
    /// Trạng thái đứng yên: không di chuyển, chờ nhậpMove để sang MoveState.
    /// </summary>
    public sealed class PlayerIdleState : IState
    {
        private readonly PlayerContext             _ctx;
        private readonly StateMachine              _fsm;
        private readonly PlayerMovementConfigSO    _config;

        private PlayerMoveState _moveState;

        public PlayerIdleState(PlayerContext ctx, StateMachine fsm, PlayerMovementConfigSO config)
        {
            _ctx    = ctx;
            _fsm    = fsm;
            _config = config;
        }

        public void SetTransitions(PlayerMoveState moveState) => _moveState = moveState;

        public void Enter()
        {
            _ctx.Rigidbody.linearVelocity = Vector2.zero;
        }

        public void Tick(float deltaTime)
        {
            if (_ctx.MoveInput.sqrMagnitude > 0.01f)
                _fsm.ChangeState(_moveState);
        }

        public void FixedTick(float fixedDeltaTime) { }

        public void Exit() { }
    }
}
