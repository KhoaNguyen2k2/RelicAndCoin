using System;
using System.Collections.Generic;

namespace RelicAndCoin.AI.Shop
{
    /// <summary>
    /// Quản lý danh sách nhân viên đã thuê, tính tổng tiền lương cuối ngày,
    /// tự động trừ vàng khi kết thúc ngày bán hàng và tạm nghỉ việc nếu người chơi
    /// không đủ vàng trả lương.
    /// Pure C# class — không phụ thuộc Unity lifecycle.
    /// </summary>
    public sealed class StaffPayrollService
    {
        // Nhân viên hiện đang được thuê (khóa là StaffDataSO.Id).
        private readonly Dictionary<string, StaffDataSO> _hiredStaff = new();

        public IReadOnlyDictionary<string, StaffDataSO> HiredStaff => _hiredStaff;

        /// <summary>Thuê một nhân viên. Thay thế nếu nhân viên đó đã có trong danh sách.</summary>
        public void Hire(StaffDataSO staff)
        {
            if (staff == null) throw new ArgumentNullException(nameof(staff));
            if (string.IsNullOrEmpty(staff.Id)) throw new ArgumentException("Staff Id không được rỗng.", nameof(staff.Id));

            _hiredStaff[staff.Id] = staff;
        }

        /// <summary>Tạm nghỉ việc một nhân viên theo Id.</summary>
        public bool Fire(string staffId)
        {
            if (string.IsNullOrEmpty(staffId)) throw new ArgumentException("Staff Id không được rỗng.", nameof(staffId));
            return _hiredStaff.Remove(staffId);
        }

        /// <summary>Tính tổng tiền lương phải trả cho tất cả nhân viên trong một ngày.</summary>
        public float CalculateTotalDailySalary()
        {
            float total = 0f;
            foreach (var staff in _hiredStaff.Values)
                total += staff.DailySalary;
            return total;
        }

        /// <summary>
        /// Xử lý trả lương cuối ngày.
        /// Trả về (thành công, số vàng còn lại, danh sách nhân viên bị nghỉ việc do thiếu tiền).
        /// </summary>
        public (bool Success, int RemainingGold, IReadOnlyList<string> FiredStaffIds) ProcessPayday(int currentGold)
        {
            float totalSalary = CalculateTotalDailySalary();
            var fired = new List<string>();

            if (currentGold >= totalSalary)
            {
                return (true, currentGold - (int)totalSalary, fired);
            }

            // Không đủ tiền: nghỉ việc nhân viên từ lương cao nhất đến thấp nhất cho đủ tiền.
            int remainingGold = currentGold;
            foreach (var staff in SortBySalaryDescending(_hiredStaff.Values))
            {
                if (remainingGold >= totalSalary) break;
                _hiredStaff.Remove(staff.Id);
                fired.Add(staff.Id);
                remainingGold = (int)(remainingGold - staff.DailySalary);
                totalSalary -= staff.DailySalary;
            }

            return (false, remainingGold, fired);
        }

        private static List<StaffDataSO> SortBySalaryDescending(IEnumerable<StaffDataSO> staffs)
        {
            var list = new List<StaffDataSO>(staffs);
            list.Sort((a, b) => b.DailySalary.CompareTo(a.DailySalary));
            return list;
        }
    }
}
