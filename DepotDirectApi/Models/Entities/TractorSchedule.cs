using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DepotDirectApi.Models.Entities;

[Table("tractor_schedules", Schema = "depotdirect")]
public class TractorSchedule
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("tractor_id")]
    public int TractorId { get; set; }

    [Column("driver_id")]
    public int? DriverId { get; set; }

    [Required]
    [Column("day_of_week")]
    public int DayOfWeek { get; set; }

    [Required]
    [Column("shift_start_time")]
    public TimeSpan ShiftStartTime { get; set; }

    [Required]
    [Column("shift_end_time")]
    public TimeSpan ShiftEndTime { get; set; }

    [Column("start_depot_id")]
    public int? StartDepotId { get; set; }

    [Column("start_parking_id")]
    public int? StartParkingId { get; set; }

    [Column("end_depot_id")]
    public int? EndDepotId { get; set; }

    [Column("end_parking_id")]
    public int? EndParkingId { get; set; }

    [Column("is_overtime")]
    public bool IsOvertime { get; set; } = false;

    [Column("active")]
    public bool Active { get; set; } = true;

    [Column("created_by")]
    public int? CreatedBy { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [Column("deleted_at")]
    public DateTime? DeletedAt { get; set; }

    // Navigation properties
    [ForeignKey("TractorId")]
    public virtual Tractor Tractor { get; set; } = null!;

    [ForeignKey("DriverId")]
    public virtual Driver? Driver { get; set; }

    [ForeignKey("StartDepotId")]
    public virtual Depot? StartDepot { get; set; }

    [ForeignKey("StartParkingId")]
    public virtual Parking? StartParking { get; set; }

    [ForeignKey("EndDepotId")]
    public virtual Depot? EndDepot { get; set; }

    [ForeignKey("EndParkingId")]
    public virtual Parking? EndParking { get; set; }
}