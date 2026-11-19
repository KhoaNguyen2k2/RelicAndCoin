using RelicAndCoin.Core.StateMachine;
using UnityEngine;

namespace RelicAndCoin.Player.States
{
    /// <summary>
    /// Trạng thái chết: dừng mọi vận tốc, tắt va chạm nhận sát thương, phát sự kiện ngã xuống.
    /// Trạng thái terminal — không chuyển sang trạng thái khác.
    /// </summary>
    public sealed class PlayerDeadState : IState
    {
        private readonly PlayerContext _ctx;
        private readonly StateMachine  _fsm;

        private bool _handled;

        public PlayerDeadState(PlayerContext ctx, StateMachine fsm)
        {
            _ctx = ctx;
            _fsm = fsm;
        }

        public void Enter()
        {
            if (_handled) return;
            _handled = true;

            _ctx.Rigidbody.linearVelocity = Vector2.zero;
            _ctx.Rigidbody.bodyType = RigidbodyType2D.Static; // dừng vật lý, không bị đẩy

            // TODO: Tắt collider nhận sát thương + phát sự kiện OnDied qua CharacterStatsController
            // (UI Game Over, respawn, v.v. sẽ lắng nghe sự kiện này — Lượt 5)
        }

        public void Tick(float deltaTime) { }

        public void FixedTick(float fixedDeltaTime)
        {
            _ctx.Rigidbody.linearVelocity = Vector2.zero;
        }

        public void Exit() { }
    }
}
