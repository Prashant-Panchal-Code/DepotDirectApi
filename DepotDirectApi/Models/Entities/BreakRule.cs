using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DepotDirectApi.Models.Entities;

[Table("break_rules", Schema = "depotdirect")]
public class BreakRule
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("rule_name")]
    public string RuleName { get; set; } = string.Empty;

    [Required]
    [Column("company_id")]
    public int CompanyId { get; set; }

    [Required]
    [Column("max_continuous_drive_mins")]
    public int MaxContinuousDriveMins { get; set; }

    [Required]
    [Column("min_break_duration_mins")]
    public int MinBreakDurationMins { get; set; }

    [Required]
    [Column("max_daily_drive_mins")]
    public int MaxDailyDriveMins { get; set; }

    [Required]
    [Column("min_daily_rest_mins")]
    public int MinDailyRestMins { get; set; }

    [Column("active")]
    public bool Active { get; set; } = true;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey("CompanyId")]
    public virtual Company Company { get; set; } = null!;

    public virtual ICollection<Driver> Drivers { get; set; } = new List<Driver>();
}