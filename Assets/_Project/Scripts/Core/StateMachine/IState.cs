namespace RelicAndCoin.Core.StateMachine
{
    /// <summary>
    /// Giao diện cho mọi trạng thái trong State Machine.
    /// Mỗi state là một class riêng implement IState.
    /// Logic chuyển trạng thái nằm trong state hoặc context, không rải rác trong Update().
    /// </summary>
    public interface IState
    {
        /// <summary>Gọi khi vào trạng thái.</summary>
        void Enter();

        /// <summary>Gọi mỗi frame (Update).</summary>
        /// <param name="deltaTime">Thời gian của frame hiện tại (giây).</param>
        void Tick(float deltaTime);

        /// <summary>Gọi mỗi frame vật lý (FixedUpdate).</summary>
        /// <param name="fixedDeltaTime">Khoảng thời gian vật lý (giây).</param>
        void FixedTick(float fixedDeltaTime);

        /// <summary>Gọi khi rời khỏi trạng thái.</summary>
        void Exit();
    }
}
