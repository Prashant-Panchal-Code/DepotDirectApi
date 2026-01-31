using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace DepotDirectApi.Models.Entities;

[Table("site_tanks", Schema = "depotdirect")]
public class SiteTank
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("site_id")]
    public int SiteId { get; set; }

    [Column("product_id")]
    public int? ProductId { get; set; }

    [Required]
    [Column("tank_code")]
    public string TankCode { get; set; } = string.Empty;

    [Column("capacity_l")]
    public decimal CapacityL { get; set; }

    [Column("safe_fill_l")]
    public decimal SafeFillL { get; set; }

    [Column("deadstock_l")]
    public decimal DeadstockL { get; set; }

    [Column("discharge_rate_lpm")]
    public decimal? DischargeRateLpm { get; set; }

    [Column("active")]
    public bool Active { get; set; } = true;

    [Column("metadata", TypeName = "jsonb")]
    public JsonDocument? Metadata { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    [ForeignKey("SiteId")]
    public virtual Site Site { get; set; } = null!;

    [ForeignKey("ProductId")]
    public virtual Product? Product { get; set; }

    public virtual ICollection<TankReading> TankReadings { get; set; } = new List<TankReading>();
    public virtual ICollection<TankDelivery> TankDeliveries { get; set; } = new List<TankDelivery>();
}
