using RelicAndCoin.Core.StateMachine;
using RelicAndCoin.Core.Stats;
using RelicAndCoin.Player.States;
using UnityEngine;

namespace RelicAndCoin.Player
{
    /// <summary>
    /// MonoBehaviour mỏng: sở hữu Rigidbody2D + CharacterStatsController,
    /// khởi tạo các State một lần duy nhất trong Awake() (tránh cấp phát new khi chuyển state),
    /// nhận Input và cập nhật StateMachine.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(CharacterStatsController))]
    public sealed class PlayerController : MonoBehaviour
    {
        [Header("Tham chiếu")]
        [SerializeField] private PlayerMovementConfigSO _config;

        private Rigidbody2D              _rigidbody;
        private CharacterStatsController _stats;
        private PlayerContext            _ctx;
        private StateMachine             _fsm;

        // Các instance State được tạo một lần duy nhất
        private PlayerIdleState     _idleState;
        private PlayerMoveState     _moveState;
        private PlayerSprintState   _sprintState;
        private PlayerDashState     _dashState;
        private PlayerAttackState   _attackState;
        private PlayerInteractState _interactState;
        private PlayerDeadState     _deadState;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            _stats     = GetComponent<CharacterStatsController>();

            if (_config == null)
                Debug.LogWarning($"[PlayerController] {name}: PlayerMovementConfigSO chưa được gán trong Inspector.");

            _ctx = new PlayerContext(_rigidbody, _stats);
            _fsm = new StateMachine();

            // ── Tạo instance State một lần duy nhất ─────────────────────
            _idleState     = new PlayerIdleState(_ctx, _fsm, _config);
            _moveState     = new PlayerMoveState(_ctx, _fsm, _config);
            _dashState     = new PlayerDashState(_ctx, _fsm, _config);
            _sprintState   = new PlayerSprintState(_ctx, _fsm, _config, _moveState, _dashState);
            _attackState   = new PlayerAttackState(_ctx, _fsm, _config);
            _interactState = new PlayerInteractState(_ctx, _fsm, _config);
            _deadState     = new PlayerDeadState(_ctx, _fsm);

            // ── Wiring: kết nối transition target cho từng state ─────────
            _idleState.SetTransitions(_moveState);
            _moveState.SetTransitions(_idleState, _sprintState, _dashState);
            _dashState.SetTransitions(_moveState);
            _attackState.SetTransitions(_moveState, _idleState);
            _interactState.SetTransitions(_moveState, _idleState);

            _fsm.ChangeState(_idleState);
        }

        private void Update()
        {
            // Đọc input (cần gán lại trong Inspector hoặc qua Input System)
            _ctx.MoveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            _ctx.IsSprintPressed = Input.GetKey(KeyCode.LeftShift);
            _ctx.IsDashPressed   = Input.GetKeyDown(KeyCode.Space);
            _ctx.IsAttackPressed = Input.GetKeyDown(KeyCode.J);
            _ctx.IsInteractPressed = Input.GetKeyDown(KeyCode.F);

            _fsm.Tick(Time.deltaTime);
        }

        private void FixedUpdate()
        {
            // Giảm hồi chiêu lướt né
            if (_ctx.DashCooldownRemaining > 0f)
                _ctx.DashCooldownRemaining -= Time.fixedDeltaTime;

            _fsm.FixedTick(Time.fixedDeltaTime);
        }
    }
}
