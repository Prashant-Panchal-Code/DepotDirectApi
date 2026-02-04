using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DepotDirectApi.Models.Entities;

[Table("vehicle_combination_trailers", Schema = "depotdirect")]
public class VehicleCombinationTrailer
{
    [Required]
    [Column("combination_id")]
    public int CombinationId { get; set; }

    [Required]
    [Column("trailer_id")]
    public int TrailerId { get; set; }

    [Column("sequence_number")]
    public int SequenceNumber { get; set; } = 1;

    // Navigation properties
    [ForeignKey("CombinationId")]
    public virtual VehicleCombination VehicleCombination { get; set; } = null!;

    [ForeignKey("TrailerId")]
    public virtual Trailer Trailer { get; set; } = null!;
}