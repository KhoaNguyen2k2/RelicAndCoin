namespace RelicAndCoin.Core.StateMachine
{
    /// <summary>
    /// Lớp Pure C# quản lý chuyển trạng thái. Không cấp phát GC trong vòng lặp
    /// (chỉ gán tham chiếu và gọi interface, không new() hay danh sách).
    /// </summary>
    public sealed class StateMachine
    {
        private IState _currentState;

        /// <summary>Trạng thái hiện tại (có thể null).</summary>
        public IState CurrentState => _currentState;

        /// <summary>Có trạng thái đang active không.</summary>
        public bool HasState => _currentState != null;

        /// <summary>
        /// Chuyển sang trạng thái mới. Bỏ qua nếu giống trạng thái hiện tại.
        /// Gọi Exit() trên state cũ rồi Enter() trên state mới.
        /// </summary>
        public void ChangeState(IState newState)
        {
            if (_currentState == newState) return;

            _currentState?.Exit();
            _currentState = newState;
            _currentState?.Enter();
        }

        /// <summary>Gọi từ Update() của MonoBehaviour wrapper.</summary>
        public void Tick(float deltaTime) => _currentState?.Tick(deltaTime);

        /// <summary>Gọi từ FixedUpdate() của MonoBehaviour wrapper.</summary>
        public void FixedTick(float fixedDeltaTime) => _currentState?.FixedTick(fixedDeltaTime);
    }
}
