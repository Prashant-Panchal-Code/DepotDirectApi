using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace DepotDirectApi.Models.Entities;

[Table("notes", Schema = "depotdirect")]
public class Note
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("category")]
    public string Category { get; set; } = string.Empty; // General, Maintenance, Safety, Delivery Operations

    [Required]
    [Column("priority")]
    public string Priority { get; set; } = "Medium"; // High, Medium, Low

    [Required]
    [Column("comment")]
    public string Comment { get; set; } = string.Empty;

    [Required]
    [Column("status")]
    public string Status { get; set; } = "Open"; // Open, In Review, Closed

    [Column("closing_comment")]
    public string? ClosingComment { get; set; }

    [Column("closed_at")]
    public DateTime? ClosedAt { get; set; }

    [Column("closed_by")]
    public int? ClosedBy { get; set; }

    // Polymorphic targets - only one should be set per DB constraint
    [Column("site_id")]
    public int? SiteId { get; set; }

    [Column("depot_id")]
    public int? DepotId { get; set; }

    [Column("parking_id")]
    public int? ParkingId { get; set; }

    [Column("vehicle_id")]
    public int? VehicleId { get; set; }

    [Required]
    [Column("company_id")]
    public int CompanyId { get; set; }

    [Column("created_by")]
    public int? CreatedBy { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [Column("deleted_at")]
    public DateTime? DeletedAt { get; set; }

    // Navigation properties (optional)
}
