using AcsEvent.Context;
using AcsEvent.Interface;
using AcsEvent.Models;
using Microsoft.EntityFrameworkCore;

namespace AcsEvent.Services
{
    public class AttendanceBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AttendanceBackgroundService> _logger;
        private readonly TimeSpan _runTime = TimeSpan.FromMinutes(1); // kiểm tra mỗi 1 phút
        private bool _hasRunToday;
        private DateTime _lastRunDate = DateTime.MinValue;

        public AttendanceBackgroundService(IServiceProvider serviceProvider, ILogger<AttendanceBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("AttendanceBackgroundService is starting...");
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var now = DateTime.Now;
                    _logger.LogInformation($"Background service running at {now}");

                    // Nếu sang ngày mới, reset cờ và cập nhật ngày
                    if (_lastRunDate.Date != now.Date)
                    {
                        _hasRunToday = false;
                        _lastRunDate = now.Date;
                        _logger.LogInformation($"New day detected. Resetting run flag. Current date: {now.Date}");
                    }

                    // Nếu chưa chạy hôm nay thì chạy job
                    if (!_hasRunToday)
                    {
                        _logger.LogInformation($"Starting to save yesterday attendance data at {now}");
                        await SaveYesterdayAttendanceAsync();
                        _hasRunToday = true;
                        _logger.LogInformation($"Finished saving yesterday attendance data at {DateTime.Now}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in AttendanceBackgroundService");
                }
                _logger.LogInformation($"Waiting {_runTime.TotalMinutes} minutes for next check...");
                await Task.Delay(_runTime, stoppingToken);
            }
            _logger.LogInformation("AttendanceBackgroundService is stopping...");
        }

        private async Task SaveYesterdayAttendanceAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AcsEventDbContext>();
            var attendanceService = scope.ServiceProvider.GetRequiredService<IAttendanceService>();
            var phongBanService = scope.ServiceProvider.GetRequiredService<IPhongBanService>();

            DateTime yesterday = DateTime.Today.AddDays(-1);
            _logger.LogInformation($"Processing attendance data for date: {yesterday.Date}");

            var phongBans = await phongBanService.GetPhongBansAsync();
            _logger.LogInformation($"Found {phongBans?.Count() ?? 0} departments");

            int totalRecordsProcessed = 0;
            int totalRecordsAdded = 0;
            int totalRecordsUpdated = 0;

            foreach (var pb in phongBans)
            {
                var attendances = await attendanceService.GetAttendanceByPhongBanAsync(pb.MaPb, yesterday);
                _logger.LogInformation($"Processing {attendances?.Count() ?? 0} attendance records for department: {pb.MaPb}");

                foreach (var att in attendances)
                {
                    totalRecordsProcessed++;

                    // Quy định giờ làm: 07:30 sáng, 16:30 chiều
                    var quyDinhSang = new TimeSpan(7, 30, 0);
                    var quyDinhChieu = new TimeSpan(16, 30, 0);
                    bool diMuon = false;
                    bool veSom = false;

                    if (att.FirstIn == null && att.LastOut == null)
                    {
                        diMuon = true;
                        veSom = true;
                    }
                    else
                    {
                        if (att.FirstIn == null || att.FirstIn.Value.TimeOfDay > quyDinhSang)
                            diMuon = true;
                        if (att.LastOut == null || att.LastOut.Value.TimeOfDay < quyDinhChieu)
                            veSom = true;
                    }

                    if (att.FirstIn != null || att.LastOut != null)
                    {
                        var existing = await dbContext.CheckInOuts.FirstOrDefaultAsync(x => x.MaNV == att.Manv && x.TimeIn.HasValue && x.TimeIn.Value.Date == yesterday.Date);
                        if (existing != null)
                        {
                            existing.TimeIn = att.FirstIn;
                            existing.TimeOut = att.LastOut; // Ensure TimeOut is updated
                            existing.DiMuon = diMuon;
                            existing.VeSom = veSom;
                            await dbContext.SaveChangesAsync();
                            totalRecordsUpdated++;
                            _logger.LogDebug($"Updated record for employee: {att.Manv}");
                        }
                        else
                        {
                            dbContext.CheckInOuts.Add(new CheckInOut
                            {
                                MaNV = att.Manv,
                                Name = att.Name,
                                TimeIn = att.FirstIn,
                                TimeOut = att.LastOut, // Ensure TimeOut is added
                                DiMuon = diMuon,
                                VeSom = veSom
                            });
                            await dbContext.SaveChangesAsync();
                            totalRecordsAdded++;
                            _logger.LogDebug($"Added new record for employee: {att.Manv}");
                        }
                    }
                    else
                    {
                        _logger.LogDebug($"Skipped employee {att.Manv} - no check-in/out data");
                    }
                }
            }

            _logger.LogInformation($"Attendance processing completed. Total: {totalRecordsProcessed}, Added: {totalRecordsAdded}, Updated: {totalRecordsUpdated}");
        }
    }
}