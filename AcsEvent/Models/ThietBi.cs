using System.ComponentModel.DataAnnotations;

namespace AcsEvent.Entities;

public class ThietBi
{
    [Key]
    public int Id { get; set; }
    public string TenTB { get; set; }
    public string IP { get; set; }
    public string username { get; set; }
    public string password { get; set; }
}