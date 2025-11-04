using AcsEvent.Helpers;
using AcsEvent.Interface;
using Microsoft.AspNetCore.Mvc;
using AcsEvent.DTOs.AcsEvent;

namespace AcsEvent.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AcsEventController : ControllerBase
{
    private readonly IAttendanceService _attendanceService;
    private readonly ILogger<AcsEventController> _logger;

    public AcsEventController(
        IAttendanceService attendanceService,
        ILogger<AcsEventController> logger)
    {
        _attendanceService = attendanceService;
        _logger = logger;
    }

    [HttpPost("attendance-by-phongban")]
    public async Task<IActionResult> GetAttendanceByPhongBan([FromBody] int phongBanId, int pageNumber = 1,
        int pageSize = 15)
    {
        try
        {
            if (phongBanId <= 0)
            {
                return BadRequest(new
                {
                    success = false,
                    error = "PhongBanId is required and must be greater than 0"
                });
            }

            // Get attendance data from service
            var completeAttendanceList = await _attendanceService.GetAttendanceByPhongBanAsync(phongBanId);

            // Apply pagination
            var totalRecords = completeAttendanceList.Count;
            var pagedData = completeAttendanceList
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Create paged response using PaginationHelper
            var pagedResponse = PaginationHelper.CreatePagedResponse(
                pagedData,
                pageNumber,
                pageSize,
                totalRecords);

            return Ok(pagedResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetAttendanceByPhongBan");
            return BadRequest(new
            {
                success = false,
                error = ex.Message
            });
        }
    }

    [HttpPost("GetAttendanceByEmployeeNameAndDateRange")]
    public async Task<IActionResult> GetAttendanceByEmployeeNameAndDateRange([FromBody] AttendanceRangeRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (string.IsNullOrWhiteSpace(request.EmployeeName))
            {
                return BadRequest(new { Message = "employeeId is required." });
            }

            if (request.StartTime == default || request.EndTime == default)
            {
                return BadRequest(new { Message = "startTime and endTime are required and must be valid dates." });
            }

            if (request.StartTime > request.EndTime)
            {
                return BadRequest(new { Message = "startTime must be less than or equal to endTime." });
            }

            var attendance = await _attendanceService.GetAttendanceByEmployeeAndDateRangeAsync(
                request.EmployeeName,
                request.StartTime,
                request.EndTime);

            if (attendance == null || attendance.Count == 0)
            {
                return NotFound(new
                    { Message = "No attendance data found for the specified employee and time range." });
            }

            return Ok(attendance);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching attendance data for employee {EmployeeName} between {Start} and {End}",
                request.EmployeeName, request.StartTime, request.EndTime);
            return StatusCode(500, new { Message = "An error occurred while fetching attendance data." });
        }
    }
}