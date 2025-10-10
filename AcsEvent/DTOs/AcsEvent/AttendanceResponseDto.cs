namespace AcsEvent.DTOs.AcsEvent;

public class AttendanceResponseDto
{
    public string Manv { get; set; }
    public string Name { get; set; }
    public DateTimeOffset Date { get; set; }
    public DateTimeOffset? FirstIn { get; set; }
    public DateTimeOffset? LastOut { get; set; }
}