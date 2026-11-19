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
            _ctx.Rigidbody.linearVelocity = Vector2.zero; // khóa di chuyển khi đánh
            // TODO: Kích hoạt animation vung vũ khí + hitbox qua WeaponController (Lượt 4)
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
            _ctx.Rigidbody.linearVelocity = Vector2.zero; // giữ khóa di chuyển trong đánh
        }

        public void Exit() { }
    }
}
