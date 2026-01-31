using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace DepotDirectApi.Models.Entities;

[Table("depot_products", Schema = "depotdirect")]
public class DepotProduct
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("depot_id")]
    public int DepotId { get; set; }

    [Required]
    [Column("product_id")]
    public int ProductId { get; set; }

    [Column("density")]
    public decimal? Density { get; set; }

    [Column("planning_temperature")]
    public decimal? PlanningTemperature { get; set; }

    [Column("loading_rate_lpm")]
    public decimal LoadingRateLpm { get; set; } = 1500.00M;

    [Column("product_available")]
    public bool ProductAvailable { get; set; } = true;

    [Column("cost_per_litre")]
    public decimal? CostPerLitre { get; set; }

    [Column("offtake_limit_active")]
    public bool OfftakeLimitActive { get; set; } = false;

    [Column("daily_min_limit_l")]
    public decimal? DailyMinLimitL { get; set; }

    [Column("daily_max_limit_l")]
    public decimal? DailyMaxLimitL { get; set; }

    [Column("metadata", TypeName = "jsonb")]
    public JsonDocument? Metadata { get; set; }

    [Column("created_by")]
    public int? CreatedBy { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [Column("deleted_at")]
    public DateTime? DeletedAt { get; set; }

    // Navigation properties
    [ForeignKey("ProductId")]
    public virtual Product Product { get; set; } = null!;

    [ForeignKey("DepotId")]
    public virtual Depot Depot { get; set; } = null!;
}
