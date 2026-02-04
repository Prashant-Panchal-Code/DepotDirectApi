using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace DepotDirectApi.Models.Entities;

[Table("tractors", Schema = "depotdirect")]
public class Tractor
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("tractor_code")]
    public string TractorCode { get; set; } = string.Empty;

    [Required]
    [Column("tractor_name")]
    public string TractorName { get; set; } = string.Empty;

    [Required]
    [Column("license_plate")]
    public string LicensePlate { get; set; } = string.Empty;

    [Required]
    [Column("haulier_id")]
    public int HaulierId { get; set; }

    [Column("region_id")]
    public int? RegionId { get; set; }

    [Column("status")]
    public string Status { get; set; } = "Active";

    [Column("pump_available")]
    public bool PumpAvailable { get; set; } = false;

    [Column("pump_flow_rate_lpm")]
    public decimal? PumpFlowRateLpm { get; set; }

    [Column("curb_weight_kg")]
    public decimal? CurbWeightKg { get; set; }

    [Column("number_of_axles")]
    public int? NumberOfAxles { get; set; }

    [Column("axle_configuration", TypeName = "jsonb")]
    public JsonDocument? AxleConfiguration { get; set; }

    [Column("metadata", TypeName = "jsonb")]
    public JsonDocument? Metadata { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [Column("deleted_at")]
    public DateTime? DeletedAt { get; set; }

    // Navigation properties
    [ForeignKey("HaulierId")]
    public virtual Haulier Haulier { get; set; } = null!;

    [ForeignKey("RegionId")]
    public virtual Region? Region { get; set; }

    public virtual ICollection<VehicleCombination> VehicleCombinations { get; set; } = new List<VehicleCombination>();
    public virtual ICollection<TractorSchedule> TractorSchedules { get; set; } = new List<TractorSchedule>();
}