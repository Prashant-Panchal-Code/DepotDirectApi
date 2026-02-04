using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace DepotDirectApi.Models.Entities;

[Table("trailers", Schema = "depotdirect")]
public class Trailer
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("trailer_code")]
    public string TrailerCode { get; set; } = string.Empty;

    [Required]
    [Column("trailer_name")]
    public string TrailerName { get; set; } = string.Empty;

    [Required]
    [Column("license_plate")]
    public string LicensePlate { get; set; } = string.Empty;

    [Required]
    [Column("haulier_id")]
    public int HaulierId { get; set; }

    [Column("region_id")]
    public int? RegionId { get; set; }

    [Column("unladen_weight_kg")]
    public decimal? UnladenWeightKg { get; set; }

    [Column("max_payload_kg")]
    public decimal? MaxPayloadKg { get; set; }

    [Column("max_payload_liters")]
    public decimal? MaxPayloadLiters { get; set; }

    [Column("number_of_axles")]
    public int? NumberOfAxles { get; set; }

    [Column("axle_configuration", TypeName = "jsonb")]
    public JsonDocument? AxleConfiguration { get; set; }

    [Column("status")]
    public string Status { get; set; } = "Active";

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

    public virtual ICollection<TrailerCompartment> TrailerCompartments { get; set; } = new List<TrailerCompartment>();
    public virtual ICollection<VehicleCombinationTrailer> VehicleCombinationTrailers { get; set; } = new List<VehicleCombinationTrailer>();
}