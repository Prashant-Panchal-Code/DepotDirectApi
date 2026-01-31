using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace DepotDirectApi.Models.DTOs;

public class CreateNoteDto
{
    [Required]
    [RegularExpression("^(General|Maintenance|Safety|Delivery Operations)$")]
    public string Category { get; set; } = string.Empty;

    [RegularExpression("^(High|Medium|Low)$")]
    public string Priority { get; set; } = "Medium";

    [Required]
    [StringLength(4000)]
    public string Comment { get; set; } = string.Empty;

    // One of these target ids must be supplied
    public int? SiteId { get; set; }
    public int? DepotId { get; set; }
    public int? ParkingId { get; set; }
    public int? VehicleId { get; set; }

    [Required]
    public int CompanyId { get; set; }
}

public class UpdateNoteStatusDto
{
    [Required]
    [RegularExpression("^(Open|In Review|Closed)$")]
    public string Status { get; set; } = "Open";

    // Required only when setting status to Closed
    public string? ClosingComment { get; set; }
}

public class NoteDto
{
    public int Id { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Priority { get; set; } = "Medium";
    public string Comment { get; set; } = string.Empty;
    public string Status { get; set; } = "Open";
    public string? ClosingComment { get; set; }
    public DateTime? ClosedAt { get; set; }
    public int? ClosedBy { get; set; }
    public int? SiteId { get; set; }
    public int? DepotId { get; set; }
    public int? ParkingId { get; set; }
    public int? VehicleId { get; set; }
    public int CompanyId { get; set; }
    public int? CreatedBy { get; set; }
    public string? CreatedByName { get; set; }
    public string? ClosedByName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}
