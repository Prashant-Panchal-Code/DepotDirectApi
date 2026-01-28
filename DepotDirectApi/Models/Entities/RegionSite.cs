using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace DepotDirectApi.Models.Entities;

[Table("region_sites", Schema = "depotdirect")]
public class RegionSite
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("site_id")]
    public int SiteId { get; set; }

    [Required]
    [Column("region_id")]
    public int RegionId { get; set; }

    [Column("site_code")]
    public string? SiteCode { get; set; }

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
    [ForeignKey("SiteId")]
    public virtual Site Site { get; set; } = null!;

    [ForeignKey("RegionId")]
    public virtual Region Region { get; set; } = null!;
}
