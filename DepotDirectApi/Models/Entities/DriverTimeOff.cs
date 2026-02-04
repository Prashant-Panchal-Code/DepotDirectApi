using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DepotDirectApi.Models.Entities;

[Table("driver_time_off", Schema = "depotdirect")]
public class DriverTimeOff
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("driver_id")]
    public int DriverId { get; set; }

    [Required]
    [Column("start_date")]
    public DateTime StartDate { get; set; }

    [Required]
    [Column("end_date")]
    public DateTime EndDate { get; set; }

    [Column("reason")]
    public string? Reason { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey("DriverId")]
    public virtual Driver Driver { get; set; } = null!;
}