using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace DepotDirectApi.Models.Entities;

[Table("sites", Schema = "depotdirect")]
public class Site
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("site_code")]
    public string SiteCode { get; set; } = string.Empty;

    [Required]
    [Column("site_name")]
    public string SiteName { get; set; } = string.Empty;

    [Column("shortcode")]
    public string? Shortcode { get; set; }

    [Column("latitude")]
    public decimal? Latitude { get; set; }

    [Column("longitude")]
    public decimal? Longitude { get; set; }

    [Column("latlong")]
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public string? LatLong { get; set; }

    [Column("street")]
    public string? Street { get; set; }

    [Column("postal_code")]
    public string? PostalCode { get; set; }

    [Column("town")]
    public string? Town { get; set; }

    [Column("active")]
    public bool Active { get; set; } = true;

    [Column("priority")]
    public string Priority { get; set; } = "Medium";

    [Column("contact_person")]
    public string? ContactPerson { get; set; }

    [Column("phone")]
    public string? Phone { get; set; }

    [Column("email")]
    public string? Email { get; set; }

    [Column("operating_hours", TypeName = "jsonb")]
    public JsonDocument? OperatingHours { get; set; }

    [Column("depot_id")]
    public int? DepotId { get; set; }

    [Column("delivery_stopped")]
    public bool DeliveryStopped { get; set; } = false;

    [Column("pumped_required")]
    public bool PumpedRequired { get; set; } = false;

    [Required]
    [Column("country_id")]
    public int CountryId { get; set; }

    [Required]
    [Column("company_id")]
    public int CompanyId { get; set; }

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

    // Navigation properties
    [ForeignKey("CountryId")]
    public virtual Country Country { get; set; } = null!;

    [ForeignKey("CompanyId")]
    public virtual Company Company { get; set; } = null!;

    public virtual ICollection<RegionSite> RegionSites { get; set; } = new List<RegionSite>();
}
