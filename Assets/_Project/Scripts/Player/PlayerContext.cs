using RelicAndCoin.Core.Stats;
using UnityEngine;

namespace RelicAndCoin.Player
{
    /// <summary>
    /// Dữ liệu dùng chung cho tất cả State của Player.
    /// MonoBehaviour wrapper (PlayerController) sở hữu Rigidbody2D và truyền tham chiếu vào đây.
    /// Pure C# — không chứa business logic, chỉ giữ trạng thái tham chiếu.
    /// </summary>
    public sealed class PlayerContext
    {
        public Rigidbody2D            Rigidbody   { get; }
        public CharacterStatsController Stats     { get; }
        public Vector2           MoveInput   { get; set; }

        /// <summary>Nhân vật đang bất tử (I-Frames khi lướt né) hay không.</summary>
        public bool IsInvulnerable { get; set; }

        /// <summary>Phím Sprint đang được giữ.</summary>
        public bool IsSprintPressed { get; set; }

        /// <summary>Phím Dash vừa nhấn (được reset mỗi frame bởi wrapper).</summary>
        public bool IsDashPressed { get; set; }

        /// <summary>Thời gian còn lại của hồi chiêu lướt né (giây).</summary>
        public float DashCooldownRemaining { get; set; }

        /// <summary>Phím tấn công vừa nhấn (được reset mỗi frame bởi wrapper).</summary>
        public bool IsAttackPressed { get; set; }

        /// <summary>Phím tương tác vừa nhấn (được reset mỗi frame bởi wrapper).</summary>
        public bool IsInteractPressed { get; set; }

        public PlayerContext(Rigidbody2D rigidbody, CharacterStatsController stats)
        {
            Rigidbody = rigidbody;
            Stats     = stats;
            MoveInput = Vector2.zero;
        }
    }
}
