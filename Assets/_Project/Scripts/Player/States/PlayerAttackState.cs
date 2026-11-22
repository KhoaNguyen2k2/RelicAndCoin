using RelicAndCoin.Core.StateMachine;
using UnityEngine;

namespace RelicAndCoin.Player.States
{
    /// <summary>
    /// Trạng thái tấn công: khóa di chuyển trong thời gian vung vũ khí, sau đó quay về Idle/Move.
    /// </summary>
    public sealed class PlayerAttackState : IState
    {
        private readonly PlayerContext             _ctx;
        private readonly StateMachine              _fsm;
        private readonly PlayerMovementConfigSO    _config;

        private float _elapsed;
        private readonly float _attackDuration = 0.5f; // thời gian khóa khi vung vũ khí

        private PlayerMoveState _moveState;
        private PlayerIdleState _idleState;

        public PlayerAttackState(PlayerContext ctx, StateMachine fsm, PlayerMovementConfigSO config)
        {
            _ctx    = ctx;
            _fsm    = fsm;
            _config = config;
        }

        public void SetTransitions(PlayerMoveState moveState, PlayerIdleState idleState)
        {
            _moveState = moveState;
            _idleState = idleState;
        }

        public void Enter()
        {
            _elapsed = 0f;
            // Cho phép người chơi duy trì quán tính di chuyển khi bắt đầu vung kiếm
            if (_ctx.MoveInput.sqrMagnitude > 0.01f)
                _ctx.Rigidbody.linearVelocity = _ctx.MoveInput.normalized * _config.WalkSpeed;
        }

        public void Tick(float deltaTime)
        {
            _elapsed += deltaTime;
            if (_elapsed >= _attackDuration)
            {
                _fsm.ChangeState(_ctx.MoveInput.sqrMagnitude > 0.01f
                    ? (IState)_moveState
                    : _idleState);
            }
        }

        public void FixedTick(float fixedDeltaTime)
        {
            // Cho phép vừa di chuyển vừa tấn công mượt mà
            if (_ctx.MoveInput.sqrMagnitude > 0.01f)
            {
                _ctx.Rigidbody.linearVelocity = _ctx.MoveInput.normalized * _config.WalkSpeed;
            }
            else
            {
                _ctx.Rigidbody.linearVelocity = Vector2.zero;
            }
        }

        public void Exit() { }
    }
}
