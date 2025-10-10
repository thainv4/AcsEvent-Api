using System.Text.Json;
using AcsEvent.DTOs.AcsEvent;
using AcsEvent.Helpers;
using AcsEvent.Interface;
using AcsEvent.Services;
using Microsoft.AspNetCore.Mvc;

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
}