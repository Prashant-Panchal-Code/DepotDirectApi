using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace DepotDirectApi.Models.Entities;

[Table("trailer_compartments", Schema = "depotdirect")]
public class TrailerCompartment
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("trailer_id")]
    public int TrailerId { get; set; }

    [Required]
    [Column("compartment_number")]
    public int CompartmentNumber { get; set; }

    [Required]
    [Column("capacity_l")]
    public decimal CapacityL { get; set; }

    [Column("min_volume_l")]
    public decimal? MinVolumeL { get; set; }

    [Column("safe_fill_l")]
    public decimal? SafeFillL { get; set; }

    [Column("must_use")]
    public bool MustUse { get; set; } = false;

    [Column("partial_load_allowed")]
    public bool PartialLoadAllowed { get; set; } = true;

    [Column("metadata", TypeName = "jsonb")]
    public JsonDocument? Metadata { get; set; }

    // Navigation properties
    [ForeignKey("TrailerId")]
    public virtual Trailer Trailer { get; set; } = null!;

    public virtual ICollection<CompartmentAllowedProduct> CompartmentAllowedProducts { get; set; } = new List<CompartmentAllowedProduct>();
}