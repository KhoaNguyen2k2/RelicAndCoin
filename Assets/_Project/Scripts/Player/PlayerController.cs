using System.Collections;
using RelicAndCoin.Combat;
using RelicAndCoin.Combat.Loot;
using RelicAndCoin.Combat.Weapons;
using RelicAndCoin.Core.StateMachine;
using RelicAndCoin.Core.Stats;
using RelicAndCoin.Player.States;
using UnityEngine;

namespace RelicAndCoin.Player
{
    /// <summary>
    /// MonoBehaviour mỏng: sở hữu Rigidbody2D + CharacterStatsController + WeaponController,
    /// khởi tạo các State một lần duy nhất trong Awake() (tránh cấp phát new khi chuyển state),
    /// nhận Input (WASD di chuyển, Shift chạy nhanh, Space lướt, J/Chuột trái chém, E tương tác) và cập nhật StateMachine.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(CharacterStatsController))]
    public sealed class PlayerController : MonoBehaviour
    {
        [Header("Tham chiếu")]
        [SerializeField] private PlayerMovementConfigSO _config;

        private Rigidbody2D              _rigidbody;
        private CharacterStatsController _stats;
        private WeaponController         _weaponController;
        private PlayerContext            _ctx;
        private StateMachine             _fsm;

        private Vector2 _facingDirection = Vector2.right;

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
            _rigidbody        = GetComponent<Rigidbody2D>();
            _stats            = GetComponent<CharacterStatsController>();
            _weaponController = GetComponent<WeaponController>();

            if (_config == null)
            {
                Debug.LogWarning($"[PlayerController] {name}: PlayerMovementConfigSO chưa được gán. Tự tạo instance mặc định.");
                _config = ScriptableObject.CreateInstance<PlayerMovementConfigSO>();
            }

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
            if (_fsm == null || _ctx == null) return;

            // Đọc input di chuyển
            _ctx.MoveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            if (_ctx.MoveInput.sqrMagnitude > 0.01f)
                _facingDirection = _ctx.MoveInput.normalized;

            _ctx.IsSprintPressed   = Input.GetKey(KeyCode.LeftShift);
            _ctx.IsDashPressed     = Input.GetKeyDown(KeyCode.Space);
            _ctx.IsAttackPressed   = Input.GetKeyDown(KeyCode.J) || Input.GetMouseButtonDown(0);
            _ctx.IsInteractPressed = Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.F);

            // ── Xử lý tấn công (Phím J hoặc Chuột trái) ──────────────────
            if (_ctx.IsAttackPressed)
            {
                Vector2 attackDir = _facingDirection;

                // Tự động nhắm thẳng vào quái vật còn sống gần nhất trong bán kính 3.5m (Auto-aim)
                IDamageable nearestEnemy = FindNearestAliveEnemy(3.5f);
                if (nearestEnemy != null)
                {
                    Vector2 toEnemy = (Vector2)nearestEnemy.Position - (Vector2)transform.position;
                    if (toEnemy.sqrMagnitude > 0.001f)
                    {
                        attackDir = toEnemy.normalized;
                    }
                }
                else if (Input.GetMouseButtonDown(0) && Camera.main != null)
                {
                    Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    Vector2 mouseDir = (Vector2)mouseWorld - (Vector2)transform.position;
                    if (mouseDir.sqrMagnitude > 0.01f)
                        attackDir = mouseDir.normalized;
                }

                if (_weaponController != null && _weaponController.TryAttack(attackDir))
                {
                    _facingDirection = attackDir;
                    _fsm.ChangeState(_attackState);
                }
            }

            // ── Xử lý tương tác (Phím E / F) ────────────────────────────
            if (_ctx.IsInteractPressed)
            {
                _fsm.ChangeState(_interactState);
                TryInteractNearby();
            }

            _fsm.Tick(Time.deltaTime);
        }

        private void TryInteractNearby()
        {
            // Quét các vật thể có thể tương tác xung quanh người chơi
            var hits = Physics2D.OverlapCircleAll(transform.position, 2.0f);
            bool interacted = false;

            for (int i = 0; i < hits.Length; i++)
            {
                var hit = hits[i];
                if (hit.TryGetComponent<ChestController>(out var chest) && !chest.IsOpened)
                {
                    chest.OpenChest();
                    interacted = true;
                    break;
                }
                if (hit.TryGetComponent<DroppedItemPickup>(out var pickup))
                {
                    pickup.Interact(gameObject);
                    interacted = true;
                    break;
                }
            }

            if (interacted)
            {
                _interactState.OnInteractComplete();
            }
            else
            {
                StartCoroutine(FinishInteractDelayed());
            }
        }

        private IDamageable FindNearestAliveEnemy(float radius)
        {
            var hits = Physics2D.OverlapCircleAll(transform.position, radius);
            IDamageable nearest = null;
            float minSqrDist = float.MaxValue;

            for (int i = 0; i < hits.Length; i++)
            {
                var hit = hits[i];
                if (hit.gameObject == gameObject) continue;

                if (hit.TryGetComponent<IDamageable>(out var damageable) && damageable.IsAlive)
                {
                    float sqrDist = ((Vector2)damageable.Position - (Vector2)transform.position).sqrMagnitude;
                    if (sqrDist < minSqrDist)
                    {
                        minSqrDist = sqrDist;
                        nearest = damageable;
                    }
                }
            }

            return nearest;
        }

        private IEnumerator FinishInteractDelayed()
        {
            yield return new WaitForSeconds(0.1f);
            _interactState.OnInteractComplete();
        }

        private void FixedUpdate()
        {
            if (_fsm == null || _ctx == null) return;

            // Giảm hồi chiêu lướt né
            if (_ctx.DashCooldownRemaining > 0f)
                _ctx.DashCooldownRemaining -= Time.fixedDeltaTime;

            _fsm.FixedTick(Time.fixedDeltaTime);
        }
    }
}
