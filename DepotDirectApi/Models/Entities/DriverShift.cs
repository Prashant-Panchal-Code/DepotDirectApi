using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DepotDirectApi.Models.Entities;

[Table("driver_shifts", Schema = "depotdirect")]
public class DriverShift
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("driver_id")]
    public int DriverId { get; set; }

    [Column("day_of_week")]
    public int? DayOfWeek { get; set; }

    [Required]
    [Column("start_time")]
    public TimeSpan StartTime { get; set; }

    [Required]
    [Column("end_time")]
    public TimeSpan EndTime { get; set; }

    [Column("start_depot_id")]
    public int? StartDepotId { get; set; }

    [Column("active")]
    public bool Active { get; set; } = true;

    // Navigation properties
    [ForeignKey("DriverId")]
    public virtual Driver Driver { get; set; } = null!;

    [ForeignKey("StartDepotId")]
    public virtual Depot? StartDepot { get; set; }
}