using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace DepotDirectApi.Models.Entities;

[Table("users", Schema = "depotdirect")]
public class User
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("company_id")]
    public int? CompanyId { get; set; }

    [Required]
    [Column("role_id")]
    public int RoleId { get; set; }

    [Required]
    [Column("email")]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [Column("password_hash")]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    [Column("full_name")]
    public string FullName { get; set; } = string.Empty;

    [Column("phone")]
    public string? Phone { get; set; }

    [Column("active")]
    public bool Active { get; set; } = true;

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
    [ForeignKey("CompanyId")]
    public virtual Company? Company { get; set; }

    [ForeignKey("RoleId")]
    public virtual Role Role { get; set; } = null!;
}