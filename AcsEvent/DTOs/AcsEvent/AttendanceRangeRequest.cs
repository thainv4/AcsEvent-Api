using System.ComponentModel.DataAnnotations;

namespace AcsEvent.DTOs.AcsEvent;

public class AttendanceRangeRequest
{
    [Required]
    public string EmployeeName { get; set; } = string.Empty;

    [Required]
    public DateTime StartTime { get; set; }

    [Required]
    public DateTime EndTime { get; set; }
}
