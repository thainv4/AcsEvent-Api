using System.Text.Json;
using AcsEvent.DTOs.AcsEvent;
using AcsEvent.Services;
using Microsoft.AspNetCore.Mvc;

namespace AcsEvent.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AcsEventController : ControllerBase
{
    private readonly HikvisionService _hikvisionService;
    private readonly ThietBiService _thietBiService;
    private readonly EmployeeService _employeeService;
    private readonly ILogger<AcsEventController> _logger;

    public AcsEventController(
        HikvisionService hikvisionService,
        ThietBiService thietBiService,
        EmployeeService employeeService,
        ILogger<AcsEventController> logger)
    {
        _hikvisionService = hikvisionService;
        _thietBiService = thietBiService;
        _employeeService = employeeService;
        _logger = logger;
    }

    [HttpPost("get-acs-events")]
    public async Task<IActionResult> GetAcsEvents([FromBody] GetAcsEventRequestDto request)
    {
        try
        {
            // Validate request
            if (request.ThietBiId <= 0)
            {
                return BadRequest(new
                {
                    success = false,
                    error = "ThietBiId is required and must be greater than 0"
                });
            }

            // Lấy thông tin authentication từ database
            var authInfo = await _thietBiService.GetThietBiAuthInfoAsync(request.ThietBiId);

            if (authInfo == null)
            {
                return NotFound(new
                {
                    success = false,
                    error = $"ThietBi with ID {request.ThietBiId} not found"
                });
            }

            // Validate auth info
            if (string.IsNullOrEmpty(authInfo.IP) ||
                string.IsNullOrEmpty(authInfo.username) ||
                string.IsNullOrEmpty(authInfo.password))
            {
                return BadRequest(new
                {
                    success = false,
                    error = $"ThietBi {request.ThietBiId} has incomplete authentication information"
                });
            }

            // Tạo request body chỉ chứa AcsEventCond
            var requestBody = new { request.AcsEventCond };

            // Gọi Hikvision API
            var result = await _hikvisionService.CallAcsEventApiAsync(authInfo, requestBody);

            // Parse và format lại JSON response
            var parsedData = ParseAndFormatResult(result);

            return Ok(new
            {
                success = true,
                thietBiId = request.ThietBiId,
                deviceIP = authInfo.IP,
                timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                data = parsedData // Trả về object đã parse thay vì raw string
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting ACS events for ThietBi ID: {request.ThietBiId}");
            return BadRequest(new
            {
                success = false,
                error = ex.Message
            });
        }
    }

    [HttpPost("attendance-by-phongban")]
    public async Task<IActionResult> GetAttendanceByPhongBan([FromBody] int phongBanId)
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

            var employees = await _employeeService.GetEmployeesByPhongBanIdAsync(phongBanId);
            var authInfos = await _thietBiService.GetThietBiAuthInfosAsync();
            var allRecords = new List<InfoRecord>();

            // Collect all records from all devices for all employees
            foreach (var auth in authInfos)
            {
                foreach (var employee in employees)
                {
                    if (employee != null && !string.IsNullOrEmpty(employee.MaCC.ToString()))
                    {
                        var requestBody = new
                        {
                            AcsEventCond = new
                            {
                                SearchID = "7",
                                SearchResultPosition = 0,
                                maxResults = 24,
                                major = 5,
                                minor = 75,
                                startTime = DateTime.Today.ToString("yyyy-MM-ddTHH:mm:sszzz"),
                                endTime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:sszzz"),
                                employeeNoString = employee.MaCC.ToString(),
                            }
                        };

                        var result = await _hikvisionService.CallAcsEventApiAsync(auth, requestBody);

                        try
                        {
                            var doc = JsonDocument.Parse(result);
                            if (doc.RootElement.TryGetProperty("AcsEvent", out var acsEvent) &&
                                acsEvent.TryGetProperty("responseStatusStrg", out var status) &&
                                status.GetString()?.ToUpper() == "OK" &&
                                acsEvent.TryGetProperty("InfoList", out var infoList))
                            {
                                var records = JsonSerializer.Deserialize<List<InfoRecord>>(infoList.GetRawText());
                                if (records != null)
                                {
                                    allRecords.AddRange(records);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Error processing results for employee {employee.MaCC}");
                        }
                    }
                }
            }

            // Process all collected records
            var formattedAttendance = allRecords
                .GroupBy(r => new { r.employeeNoString, r.name, Date = DateTime.Parse(r.time).Date })
                .Select(g =>
                {
                    var morning = g
                        .Where(x => DateTime.Parse(x.time).Hour < 12)
                        .OrderBy(x => DateTime.Parse(x.time))
                        .FirstOrDefault();

                    var afternoon = g
                        .Where(x => DateTime.Parse(x.time).Hour >= 12)
                        .OrderByDescending(x => DateTime.Parse(x.time))
                        .FirstOrDefault();

                    return new
                    {
                        macc = g.Key.employeeNoString,
                        name = g.Key.name,
                        date = g.Key.Date.ToString("yyyy-MM-dd"),
                        firstin = morning?.time,
                        lastout = afternoon?.time
                    };
                })
                .ToList();

            return Ok(new
            {
                success = true,
                data = formattedAttendance
            });
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

    private object ParseAndFormatResult(string result)
    {
        try
        {
            var doc = JsonDocument.Parse(result);

            if (doc.RootElement.TryGetProperty("AcsEvent", out var acsEvent) &&
                acsEvent.TryGetProperty("InfoList", out var infoList))
            {
                var records = JsonSerializer.Deserialize<List<InfoRecord>>(infoList.GetRawText());

                var grouped = records
                    .GroupBy(r => new { r.employeeNoString, r.name, Date = DateTime.Parse(r.time).Date })
                    .Select(g =>
                    {
                        var morning = g
                            .Where(x => DateTime.Parse(x.time).Hour < 12)
                            .OrderBy(x => DateTime.Parse(x.time))
                            .FirstOrDefault();

                        var afternoon = g
                            .Where(x => DateTime.Parse(x.time).Hour >= 12)
                            .OrderByDescending(x => DateTime.Parse(x.time))
                            .FirstOrDefault();

                        return new
                        {
                            macc = g.Key.employeeNoString,
                            name = g.Key.name,
                            date = g.Key.Date.ToString("yyyy-MM-dd"),
                            firstin = morning?.time,
                            lastout = afternoon?.time
                        };
                    });

                return grouped.ToList();
            }

            return new List<object>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error parsing result: {result}");
            return new List<object>();
        }
    }
}