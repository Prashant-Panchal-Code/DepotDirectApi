using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace DepotDirectApi.Models.Entities;

[Table("products", Schema = "depotdirect")]
public class Product
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("product_code")]
    public string ProductCode { get; set; } = string.Empty;

    [Required]
    [Column("product_name")]
    public string ProductName { get; set; } = string.Empty;

    [Column("short_name")]
    [MaxLength(50)]
    public string? ShortName { get; set; }

    [Column("density")]
    public decimal? Density { get; set; }

    [Column("base_temperature")]
    public decimal? BaseTemperature { get; set; }

    [Column("viscosity")]
    public decimal? Viscosity { get; set; }

    [Required]
    [Column("region_id")]
    public int RegionId { get; set; }

    [Required]
    [Column("company_id")]
    public int CompanyId { get; set; }

    [Required]
    [Column("active")]
    public bool Active { get; set; } = true;

    [Required]
    [Column("is_hazardous")]
    public bool IsHazardous { get; set; } = true;

    [Column("color_code")]
    [MaxLength(7)]
    public string? ColorCode { get; set; }

    [Column("metadata", TypeName = "jsonb")]
    public JsonDocument? Metadata { get; set; }

    [Column("created_by")]
    public int? CreatedBy { get; set; }

    [Column("last_updated_by")]
    public int? LastUpdatedBy { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [Column("deleted_at")]
    public DateTime? DeletedAt { get; set; }

    // Navigation
    [ForeignKey("CompanyId")]
    public virtual Company Company { get; set; } = null!;

    [ForeignKey("RegionId")]
    public virtual Region Region { get; set; } = null!;
}
