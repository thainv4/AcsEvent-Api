using System.Text.Json;
using AcsEvent.Context;
using AcsEvent.DTOs.AcsEvent;
using AcsEvent.Interface;
using AcsEvent.Models;
using Microsoft.EntityFrameworkCore;

namespace AcsEvent.Services;

public class AttendanceService : IAttendanceService                                                                                                                   
{
    private readonly HikvisionService _hikvisionService;
    private readonly ThietBiService _thietBiService;
    private readonly IEmployeeService _employeeService;
    private readonly ILogger<AttendanceService> _logger;

    public AttendanceService(
        HikvisionService hikvisionService,
        ThietBiService thietBiService,
        IEmployeeService employeeService,
        ILogger<AttendanceService> logger)
    {
        _hikvisionService = hikvisionService;
        _thietBiService = thietBiService;
        _employeeService = employeeService;
        _logger = logger;
    }
    
    public async Task<List<AttendanceResponseDto>> GetAttendanceByPhongBanAsync(int phongBanId, DateTime? date = null)
    {
        DateTime targetDate = date ?? DateTime.Today;
        DateTime startTime = new DateTime(targetDate.Year, targetDate.Month, targetDate.Day, 0, 0, 0);
        DateTime endTime = new DateTime(targetDate.Year, targetDate.Month, targetDate.Day, 23, 59, 59);

        var employees = await _employeeService.GetEmployeesByPhongBanIdAsync(phongBanId);
        var authInfos = await _thietBiService.GetThietBiAuthInfosAsync();
        var allRecords = new List<InfoRecord>();
        var disconnectedDevices = new List<string>();

        foreach (var auth in authInfos)
        {
            bool isConnected = await _hikvisionService.IsDeviceConnected(auth);
            if (!isConnected)
            {
                disconnectedDevices.Add(auth.IP);
                _logger.LogWarning($"Skipping device {auth.IP} as it's not responding");
                continue;
            }

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
                            maxResults = 48,
                            major = 5,
                            minor = 75,
                            startTime = startTime.ToString("yyyy-MM-ddTHH:mm:sszzz"),
                            endTime = endTime.ToString("yyyy-MM-ddTHH:mm:sszzz"),
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

        // Gom nhóm theo nhân viên và ngày
        var attendanceByEmployee = allRecords
            .GroupBy(r => new { r.employeeNoString, r.name, Date = DateTime.Parse(r.time).Date })
            .ToDictionary(
                g => g.Key.employeeNoString,
                g =>
                {
                    var firstInSang = g.Where(x => DateTime.Parse(x.time).Hour < 12)
                        .OrderBy(x => DateTime.Parse(x.time)).FirstOrDefault();
                    var lastOutChieu = g.Where(x => DateTime.Parse(x.time).Hour >= 12)
                        .OrderByDescending(x => DateTime.Parse(x.time)).FirstOrDefault();
                    return new
                    {
                        macc = g.Key.employeeNoString,
                        name = g.Key.name,
                        date = g.Key.Date.ToString("yyyy-MM-dd"),
                        firstInSang = firstInSang?.time,
                        lastOutChieu = lastOutChieu?.time
                    };
                });

        // Trả về kết quả cho từng nhân viên
        return employees
            .Select(e =>
            {
                if (attendanceByEmployee.TryGetValue(e.MaCC.ToString(), out var attendance))
                {
                    return new AttendanceResponseDto()
                    {
                        Manv = e.MaNV.ToString(),
                        Name = e.HoTen,
                        Date = DateTimeOffset.Now,
                        FirstIn = attendance.firstInSang != null ? DateTimeOffset.Parse(attendance.firstInSang) : null,
                        LastOut = attendance.lastOutChieu != null ? DateTimeOffset.Parse(attendance.lastOutChieu) : null
                    };
                }
                return new AttendanceResponseDto()
                {
                    Manv = e.MaCC.ToString(),
                    Name = e.HoTen,
                    Date = DateTimeOffset.Now,
                    FirstIn = null,
                    LastOut = null
                };
            })
            .OrderBy(item =>
            {
                string[] nameParts = item.Name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                return nameParts.Length > 0 ? nameParts[nameParts.Length - 1] : item.Name;
            })
            .ToList();
    }
    
    
    /*public async Task<int> CountAttendanceLateAsync(int phongBanId, DateTimeOffset date)
    {
        try
        {
            var employees = await _employeeService.GetEmployeesByPhongBanIdAsync(phongBanId);
            var employeeIds = employees.Select(e => e.MaCC.ToString()).ToList();
            var dateOnly = date.Date;

            // Count both employees who checked in late AND employees who didn't check in at all
            var lateCount = await _context.CheckInOuts
                .AsQueryable()
                .Where(c => employeeIds.Contains(c.MaNV) &&
                            c.TimeIn.Value.Date == dateOnly &&
                            c.DiMuon == true)
                .CountAsync();

            // Count employees who have no record for that day
            var presentEmployeeIds = await _context.CheckInOuts
                .Where(c => c.TimeIn.Value.Date == dateOnly)
                .Select(c => c.MaNV)
                .Distinct()
                .ToListAsync();
        
            var absentEmployeeCount = employeeIds.Count(id => !presentEmployeeIds.Contains(id));
        
            var totalLateCount = lateCount + absentEmployeeCount;
            _logger.LogInformation($"Late count for {dateOnly}: {totalLateCount} (Late: {lateCount}, Absent: {absentEmployeeCount})");
            return totalLateCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error counting late attendance for department {phongBanId} on {date:yyyy-MM-dd}");
            throw;
        }
    }


    public async Task<int> CountAttendanceEarlyAsync(int phongBanId, DateTimeOffset date)
    {
        try
        {
            var employees = await _employeeService.GetEmployeesByPhongBanIdAsync(phongBanId);
            var employeeIds = employees.Select(e => e.MaCC.ToString()).ToList();
            var dateOnly = date.Date;

            // Count employees who left early (VeSom == true)
            var earlyCount = await _context.CheckInOuts
                .AsQueryable()
                .Where(c => employeeIds.Contains(c.MaNV) &&
                            c.TimeOut.HasValue &&
                            c.TimeOut.Value.Date == dateOnly &&
                            c.VeSom == true)
                .CountAsync();

            // Count employees who have no checkout record for that day
            var checkedOutEmployeeIds = await _context.CheckInOuts
                .Where(c => c.TimeOut.HasValue && 
                            c.TimeOut.Value.Date == dateOnly &&
                            !c.TimeOut.Equals(c.TimeIn)) // Ensure it's a real checkout
                .Select(c => c.MaNV)
                .Distinct()
                .ToListAsync();

            var noCheckoutCount = employeeIds.Count(id => !checkedOutEmployeeIds.Contains(id));

            var totalEarlyCount = earlyCount + noCheckoutCount;
            _logger.LogInformation($"Early departure count for {dateOnly}: {totalEarlyCount} (Early: {earlyCount}, No checkout: {noCheckoutCount})");
            return totalEarlyCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error counting early departures for department {phongBanId} on {date:yyyy-MM-dd}");
            throw;
        }
    }*/
    
}