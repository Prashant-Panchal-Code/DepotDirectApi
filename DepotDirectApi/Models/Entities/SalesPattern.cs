using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DepotDirectApi.Models.Entities;

[Table("sales_patterns", Schema = "depotdirect")]
public class SalesPattern
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("tank_id")]
    public int TankId { get; set; }

    [Column("day_of_week")]
    public int DayOfWeek { get; set; }

    [Column("hour_of_day")]
    public int HourOfDay { get; set; }

    [Column("weight_factor")]
    public decimal WeightFactor { get; set; }

    [Column("avg_hourly_sales_l")]
    public decimal AvgHourlySalesL { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("TankId")]
    public virtual SiteTank Tank { get; set; } = null!;
}
