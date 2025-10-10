using AcsEvent.DTOs.AcsEvent;

namespace AcsEvent.Interface;

public interface IAttendanceService
{
    Task<List<AttendanceResponseDto>> GetAttendanceByPhongBanAsync(int phongBanId);
}