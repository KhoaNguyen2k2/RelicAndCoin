using UnityEngine;

namespace RelicAndCoin.AI.Shop
{
    /// <summary>
    /// Vai trò của nhân viên cửa hàng, xác định nhiệm vụ tự động trong ngày bán hàng.
    /// </summary>
    public enum StaffRole
    {
        Cashier,
        Restocker,
        Guard
    }

    /// <summary>
    /// ScriptableObject lưu dữ liệu một nhân viên: ID, tên, vai trò, mức lương mỗi ngày
    /// và tốc độ làm việc. Pure data — không chứa business logic.
    /// </summary>
    [CreateAssetMenu(
        menuName = "RelicAndCoin/Shop/Staff Data",
        fileName = "StaffData")]
    public sealed class StaffDataSO : ScriptableObject
    {
        [Header("Thông tin cơ bản")]
        [Tooltip("Mã định danh duy nhất của nhân viên.")]
        public string Id = string.Empty;

        [Tooltip("Tên hiển thị của nhân viên.")]
        public string DisplayName = string.Empty;

        [Header("Vai trò & Lương")]
        [Tooltip("Vai trò của nhân viên trong cửa hàng.")]
        public StaffRole Role;

        [Tooltip("Mức lương phải trả mỗi ngày (đơn vị vàng).")]
        [Min(0f)] public float DailySalary = 50f;

        [Header("Hiệu suất")]
        [Tooltip("Tốc độ làm việc của nhân viên (tương đối, dùng để tính thời gian hoàn thành nhiệm vụ).")]
        [Min(0.1f)] public float WorkSpeed = 1f;
    }
}
