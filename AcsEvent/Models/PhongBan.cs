using System.ComponentModel.DataAnnotations;

namespace AcsEvent.Entities;

public class PhongBan
{
    [Key]
    public int MaPb { get; set; } 
    public string TenPb { get; set; }
}