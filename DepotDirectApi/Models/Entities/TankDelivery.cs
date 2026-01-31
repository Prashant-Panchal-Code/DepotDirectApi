using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DepotDirectApi.Models.Entities;

[Table("tank_deliveries", Schema = "depotdirect")]
public class TankDelivery
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("tank_id")]
    public int TankId { get; set; }

    [Column("status")]
    public string Status { get; set; } = "Planned";

    [Column("planned_quantity_l")]
    public decimal? PlannedQuantityL { get; set; }

    [Column("confirmed_quantity_l")]
    public decimal? ConfirmedQuantityL { get; set; }

    [Column("scheduled_arrival")]
    public DateTime? ScheduledArrival { get; set; }

    [Column("actual_arrival")]
    public DateTime? ActualArrival { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [Column("deleted_at")]
    public DateTime? DeletedAt { get; set; }

    [ForeignKey("TankId")]
    public virtual SiteTank Tank { get; set; } = null!;
}
