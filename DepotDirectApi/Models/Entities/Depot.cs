using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace DepotDirectApi.Models.Entities;

[Table("depots", Schema = "depotdirect")]
public class Depot
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("depot_code")]
    public string DepotCode { get; set; } = string.Empty;

    [Required]
    [Column("depot_name")]
    public string DepotName { get; set; } = string.Empty;

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

    [Column("loading_bays")]
    public int? LoadingBays { get; set; }

    [Column("operating_hours", TypeName = "jsonb")]
    public JsonDocument? OperatingHours { get; set; }

    [Column("manager_name")]
    public string? ManagerName { get; set; }

    [Column("manager_phone")]
    public string? ManagerPhone { get; set; }

    [Column("manager_email")]
    public string? ManagerEmail { get; set; }

    [Column("emergency_contact")]
    public string? EmergencyContact { get; set; }

    [Column("average_loading_time")]
    public int? AverageLoadingTime { get; set; }

    [Column("max_truck_size")]
    public string? MaxTruckSize { get; set; }

    [Column("certifications")]
    public string? Certifications { get; set; }

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

    public virtual ICollection<RegionDepot> RegionDepots { get; set; } = new List<RegionDepot>();
}
