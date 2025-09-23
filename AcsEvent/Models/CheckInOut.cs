namespace AcsEvent.Models;

public class CheckInOut
{
    public string MaNV { get; set; }
    public string Name { get; set; }
    public DateTimeOffset TimeIn { get; set; }
    public DateTimeOffset TimeOut { get; set; }
    public bool DiMuon { get; set; }
    public bool VeSom { get; set; }
}