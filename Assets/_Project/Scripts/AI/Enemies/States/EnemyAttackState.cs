using RelicAndCoin.Core.StateMachine;
using RelicAndCoin.Core.Stats;
using UnityEngine;

namespace RelicAndCoin.AI.Enemies.States
{
    /// <summary>
    /// Thực hiện đòn đánh gây sát thương thật lên Player và chờ thời gian hồi chiêu. Sau đó quay lại Chase.
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

        public void Enter()
        {
            // Đòn đánh đầu tiên diễn ra sau nhịp vồ ngắn 0.15s thay vì chờ trọn vẹn thời gian hồi chiêu
            _cooldownTimer = 0.15f;
        }

        public void Tick(float deltaTime)
        {
            _cooldownTimer -= deltaTime;

            // Nới rộng khoảng cách thêm +0.6f (tính cả bán kính 2 BoxCollider) để Slime không bị bật ra khỏi tầm đánh
            float effectiveAttackRange = _ctx.Data.AttackRange + 0.6f;
            if (_ctx.DistanceToPlayer > effectiveAttackRange)
            {
                if (_chaseState != null)
                    _ctx.StateMachine.ChangeState(_chaseState);
                return;
            }

            // Giữ Slime đứng yên khi tấn công, không bị lực húc đẩy dính vào người chơi
            if (_ctx.Rigidbody != null)
                _ctx.Rigidbody.linearVelocity = Vector2.zero;

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
            if (_ctx.PlayerTransform == null) return;

            var playerStats = _ctx.PlayerTransform.GetComponent<CharacterStatsController>();
            if (playerStats == null) return;

            var enemyCtrl = _ctx.Rigidbody != null ? _ctx.Rigidbody.GetComponent<EnemyController>() : null;
            if (enemyCtrl != null)
            {
                enemyCtrl.AttackPlayer(playerStats);
            }
            else
            {
                float damage = _ctx.Data != null ? _ctx.Data.AttackDamage : 10f;
                playerStats.TakeDamage(damage);
                Debug.Log($"<color=red>[SLIME TẤN CÔNG]</color> [{_ctx.Data.EnemyName}] giáng đòn gây {damage:F1} sát thương lên Player! (HP: {playerStats.CurrentHP:F1}/{playerStats.MaxHP:F1})");
            }
        }
    }
}
