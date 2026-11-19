using UnityEngine;
using RelicAndCoin.Core.StateMachine;

namespace RelicAndCoin.AI.Enemies.States
{
    /// <summary>
    /// Tuần tra quanh điểm sinh ra. Nếu Player vào DetectRange -> chuyển Chase.
    /// </summary>
    public sealed class EnemyPatrolState : IState
    {
        private readonly EnemyContext _ctx;
        private readonly Vector2 _spawnPoint;
        private EnemyChaseState _chaseState;
        private float _wanderTimer;
        private Vector2 _wanderTarget;

        public EnemyPatrolState(EnemyContext ctx, Vector2 spawnPoint)
        {
            _ctx = ctx;
            _spawnPoint = spawnPoint;
            _wanderTarget = spawnPoint;
        }

        public void SetTransitions(EnemyChaseState chaseState) => _chaseState = chaseState;

        public void Enter()
        {
            _ctx.PathIndex = 0;
            _ctx.CurrentPath.Clear();
            _wanderTimer = 0f;
        }

        public void Tick(float deltaTime)
        {
            _wanderTimer += deltaTime;
            if (_wanderTimer > 3f)
            {
                _wanderTimer = 0f;
                _wanderTarget = _spawnPoint + Random.insideUnitCircle * 3f;
            }

            if (_ctx.DistanceToPlayer <= _ctx.DetectRange)
            {
                if (_chaseState != null)
                    _ctx.StateMachine.ChangeState(_chaseState);
                else
                    _ctx.StateMachine.ChangeState(new EnemyChaseState(_ctx));
                return;
            }

            MoveToward(_wanderTarget);
        }

        public void FixedTick(float fixedDeltaTime) { }

        public void Exit() { }

        private void MoveToward(Vector2 target)
        {
            var dir = (target - (Vector2)_ctx.Rigidbody.position).normalized;
            _ctx.Rigidbody.MovePosition(_ctx.Rigidbody.position + dir * _ctx.PatrolSpeed * Time.fixedDeltaTime);
        }
    }
}
