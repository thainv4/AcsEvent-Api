using AcsEvent.DTOs.AcsEvent;

namespace AcsEvent.Interface;

public interface IAttendanceService
{
    Task<List<AttendanceResponseDto>> GetAttendanceByPhongBanAsync(int phongBanId, DateTime? date = null);
    Task<List<AttendanceResponseDto>> GetAttendanceByEmployeeAndDateRangeAsync(string employeeName, DateTime startDate, DateTime endDate);
}