namespace AcsEvent.DTOs.AcsEvent;

public class AttendanceDto
{
    public string EmployeeId { get; set; }
    public DateTime EventTime { get; set; }
    public string Status { get; set; } // ví dụ "OK"
}