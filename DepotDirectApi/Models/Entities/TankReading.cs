using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace DepotDirectApi.Models.Entities;

[Table("tank_readings", Schema = "depotdirect")]
public class TankReading
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("tank_id")]
    public int TankId { get; set; }

    [Column("reading_timestamp")]
    public DateTime ReadingTimestamp { get; set; } = DateTime.UtcNow;

    [Column("reading_method")]
    public string ReadingMethod { get; set; } = string.Empty;

    [Column("current_volume_l")]
    public decimal CurrentVolumeL { get; set; }

    [Column("sales_since_last_reading_l")]
    public decimal? SalesSinceLastReadingL { get; set; } = 0;

    [Column("avg_daily_sales_l")]
    public decimal? AvgDailySalesL { get; set; } = 0;

    [Column("metadata", TypeName = "jsonb")]
    public JsonDocument? Metadata { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [Column("deleted_at")]
    public DateTime? DeletedAt { get; set; }

    [ForeignKey("TankId")]
    public virtual SiteTank Tank { get; set; } = null!;
}
