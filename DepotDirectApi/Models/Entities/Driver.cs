using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace DepotDirectApi.Models.Entities;

[Table("drivers", Schema = "depotdirect")]
public class Driver
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("driver_code")]
    public string DriverCode { get; set; } = string.Empty;

    [Required]
    [Column("first_name")]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [Column("last_name")]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [Column("company_id")]
    public int CompanyId { get; set; }

    [Column("home_depot_id")]
    public int? HomeDepotId { get; set; }

    [Column("region_id")]
    public int? RegionId { get; set; }

    [Required]
    [Column("license_number")]
    public string LicenseNumber { get; set; } = string.Empty;

    [Required]
    [Column("license_expiry")]
    public DateTime LicenseExpiry { get; set; }

    [Column("hazmat_certified")]
    public bool HazmatCertified { get; set; } = true;

    [Column("break_rule_id")]
    public int? BreakRuleId { get; set; }

    [Column("active")]
    public bool Active { get; set; } = true;

    [Column("status")]
    public string Status { get; set; } = "Available";

    [Column("mobile_number")]
    public string? MobileNumber { get; set; }

    [Column("email")]
    public string? Email { get; set; }

    [Column("metadata", TypeName = "jsonb")]
    public JsonDocument? Metadata { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [Column("deleted_at")]
    public DateTime? DeletedAt { get; set; }

    // Navigation properties
    [ForeignKey("CompanyId")]
    public virtual Company Company { get; set; } = null!;

    [ForeignKey("HomeDepotId")]
    public virtual Depot? HomeDepot { get; set; }

    [ForeignKey("RegionId")]
    public virtual Region? Region { get; set; }

    [ForeignKey("BreakRuleId")]
    public virtual BreakRule? BreakRule { get; set; }

    public virtual ICollection<DriverShift> DriverShifts { get; set; } = new List<DriverShift>();
    public virtual ICollection<DriverTimeOff> DriverTimeOffs { get; set; } = new List<DriverTimeOff>();
    public virtual ICollection<TractorSchedule> TractorSchedules { get; set; } = new List<TractorSchedule>();
}