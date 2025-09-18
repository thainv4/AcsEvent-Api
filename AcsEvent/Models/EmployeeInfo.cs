using System.ComponentModel.DataAnnotations.Schema;

namespace AcsEvent.Entities;

public class EmployeeInfo
{
    public int MaCC { get; set; }
    [Column(TypeName = "nvarchar(100)")]
    public string HoTen { get; set; }
    public string MaNV { get; set; }
    public int MaPb { get; set; }
}