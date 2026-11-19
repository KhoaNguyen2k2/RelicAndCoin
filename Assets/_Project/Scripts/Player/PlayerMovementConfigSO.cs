using UnityEngine;

namespace RelicAndCoin.Player
{
    /// <summary>
    /// ScriptableObject cấu hình chuyển động cho Player (Kerral / Kaery).
    /// </summary>
    [CreateAssetMenu(
        menuName = "RelicAndCoin/Player/Movement Config",
        fileName = "PlayerMovementConfig")]
    public sealed class PlayerMovementConfigSO : ScriptableObject
    {
        [Header("Tốc độ di chuyển")]
        [Min(0f)] public float WalkSpeed       = 5f;
        [Min(0f)] public float SprintSpeed     = 8f;

        [Header("Lướt né (Dash)")]
        [Min(0f)] public float DashSpeed       = 15f;
        [Min(0f)] public float DashDuration    = 0.3f;
        [Min(0f)] public float DashCooldown    = 1.5f;

        [Header("Tiêu hao Stamina")]
        [Min(0f)] public float SprintStaminaCostPerSecond = 20f;
        [Min(0f)] public float DashStaminaCost            = 20f;
    }
}
