using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace DepotDirectApi.Models.Entities;

[Table("depot_sites", Schema = "depotdirect")]
public class DepotSite
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("depot_id")]
    public int DepotId { get; set; }

    [Required]
    [Column("site_id")]
    public int SiteId { get; set; }

    [Required]
    [Column("distance_km")]
    [Range(0, 9999999.99)]
    public decimal DistanceKm { get; set; }

    [Required]
    [Column("travel_time_mins")]
    [Range(0, int.MaxValue)]
    public int TravelTimeMins { get; set; }

    [Column("return_time_mins")]
    [Range(0, int.MaxValue)]
    public int? ReturnTimeMins { get; set; }

    [Column("active")]
    public bool Active { get; set; } = true;

    [Column("is_primary")]
    public bool IsPrimary { get; set; } = false;

    [Column("transport_rate")]
    public decimal? TransportRate { get; set; }

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
    [ForeignKey("DepotId")]
    public virtual Depot Depot { get; set; } = null!;

    [ForeignKey("SiteId")]
    public virtual Site Site { get; set; } = null!;
}