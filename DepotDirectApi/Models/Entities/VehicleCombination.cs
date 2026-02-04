using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DepotDirectApi.Models.Entities;

[Table("vehicle_combinations", Schema = "depotdirect")]
public class VehicleCombination
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("combination_code")]
    public string CombinationCode { get; set; } = string.Empty;

    [Required]
    [Column("tractor_id")]
    public int TractorId { get; set; }

    [Column("gross_weight_limit_kg")]
    public decimal? GrossWeightLimitKg { get; set; }

    [Column("total_capacity_l")]
    public decimal? TotalCapacityL { get; set; }

    [Column("active")]
    public bool Active { get; set; } = true;

    [Column("is_default")]
    public bool IsDefault { get; set; } = false;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [Column("deleted_at")]
    public DateTime? DeletedAt { get; set; }

    // Navigation properties
    [ForeignKey("TractorId")]
    public virtual Tractor Tractor { get; set; } = null!;

    public virtual ICollection<VehicleCombinationTrailer> VehicleCombinationTrailers { get; set; } = new List<VehicleCombinationTrailer>();
}