using RelicAndCoin.Core.StateMachine;
using UnityEngine;

namespace RelicAndCoin.Player.States
{
    /// <summary>
    /// Trạng thái tương tác: mở rương, nói chuyện NPC, kiểm tra quầy hàng.
    /// Ngắn pauses di chuyển và chờ xác nhận từ hệ thống tương tác.
    /// </summary>
    public sealed class PlayerInteractState : IState
    {
        private readonly PlayerContext             _ctx;
        private readonly StateMachine              _fsm;
        private readonly PlayerMovementConfigSO    _config;

        private bool _interacted;

        private PlayerMoveState _moveState;
        private PlayerIdleState _idleState;

        public PlayerInteractState(PlayerContext ctx, StateMachine fsm, PlayerMovementConfigSO config)
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
            _interacted = false;
            _ctx.Rigidbody.linearVelocity = Vector2.zero;
            // TODO: Gọi InteractSystem.TryInteract(_ctx.MoveInput) và đăng ký callback
            // để quay về Idle/Move khi tương tác hoàn tất (Lượt 4)
        }

        public void Tick(float deltaTime)
        {
            if (_interacted)
            {
                _fsm.ChangeState(_ctx.MoveInput.sqrMagnitude > 0.01f
                    ? (IState)_moveState
                    : _idleState);
            }
        }

        public void FixedTick(float fixedDeltaTime)
        {
            _ctx.Rigidbody.linearVelocity = Vector2.zero;
        }

        public void Exit() { }

        /// <summary>Gọi từ InteractSystem khi tương tác hoàn tất.</summary>
        public void OnInteractComplete() => _interacted = true;
    }
}
