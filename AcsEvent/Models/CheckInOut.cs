using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AcsEvent.Models;

public class CheckInOut
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public string MaNV { get; set; }
    public string Name { get; set; }
    public DateTimeOffset? TimeIn { get; set; }
    public DateTimeOffset? TimeOut { get; set; }
    public bool? DiMuon { get; set; }
    public bool? VeSom { get; set; }
}