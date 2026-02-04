using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace DepotDirectApi.Models.DTOs;

// Driver DTOs
public class CreateDriverDto
{
    [Required]
    [StringLength(50, MinimumLength = 1)]
    public string DriverCode { get; set; } = string.Empty;

    [Required]
    [StringLength(50, MinimumLength = 1)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(50, MinimumLength = 1)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    public int CompanyId { get; set; }

    public int? HomeDepotId { get; set; }

    public int? RegionId { get; set; }

    [Required]
    [StringLength(30, MinimumLength = 1)]
    public string LicenseNumber { get; set; } = string.Empty;

    [Required]
    public DateTime LicenseExpiry { get; set; }

    public bool? HazmatCertified { get; set; }

    public int? BreakRuleId { get; set; }

    public bool? Active { get; set; }

    [StringLength(20)]
    public string? Status { get; set; }

    [StringLength(20)]
    public string? MobileNumber { get; set; }

    [EmailAddress]
    [StringLength(100)]
    public string? Email { get; set; }

    public JsonDocument? Metadata { get; set; }
}

public class UpdateDriverDto
{
    [StringLength(50)]
    public string? DriverCode { get; set; }

    [StringLength(50)]
    public string? FirstName { get; set; }

    [StringLength(50)]
    public string? LastName { get; set; }

    public int? HomeDepotId { get; set; }

    public int? RegionId { get; set; }

    [StringLength(30)]
    public string? LicenseNumber { get; set; }

    public DateTime? LicenseExpiry { get; set; }

    public bool? HazmatCertified { get; set; }

    public int? BreakRuleId { get; set; }

    public bool? Active { get; set; }

    [StringLength(20)]
    public string? Status { get; set; }

    [StringLength(20)]
    public string? MobileNumber { get; set; }

    [EmailAddress]
    [StringLength(100)]
    public string? Email { get; set; }

    public JsonDocument? Metadata { get; set; }
}

public class DriverResponseDto
{
    public int Id { get; set; }
    public string DriverCode { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public int CompanyId { get; set; }
    public int? HomeDepotId { get; set; }
    public int? RegionId { get; set; }
    public string LicenseNumber { get; set; } = string.Empty;
    public DateTime LicenseExpiry { get; set; }
    public bool HazmatCertified { get; set; }
    public int? BreakRuleId { get; set; }
    public bool Active { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? MobileNumber { get; set; }
    public string? Email { get; set; }
    public JsonDocument? Metadata { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    public CompanyDto? Company { get; set; }
    public DepotListItemDto? HomeDepot { get; set; }
    public RegionDto? Region { get; set; }
    public BreakRuleResponseDto? BreakRule { get; set; }
}

public class DriverListItemDto
{
    public int Id { get; set; }
    public string DriverCode { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public int CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public int? HomeDepotId { get; set; }
    public string? HomeDepotName { get; set; }
    public int? RegionId { get; set; }
    public string? RegionName { get; set; }
    public string LicenseNumber { get; set; } = string.Empty;
    public DateTime LicenseExpiry { get; set; }
    public bool HazmatCertified { get; set; }
    public bool Active { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? MobileNumber { get; set; }
    public string? Email { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

// Driver Shift DTOs
public class CreateDriverShiftDto
{
    [Required]
    public int DriverId { get; set; }

    [Range(0, 6, ErrorMessage = "Day of week must be between 0 (Sunday) and 6 (Saturday)")]
    public int? DayOfWeek { get; set; }

    [Required]
    public TimeSpan StartTime { get; set; }

    [Required]
    public TimeSpan EndTime { get; set; }

    public int? StartDepotId { get; set; }

    public bool? Active { get; set; }
}

public class UpdateDriverShiftDto
{
    [Range(0, 6, ErrorMessage = "Day of week must be between 0 (Sunday) and 6 (Saturday)")]
    public int? DayOfWeek { get; set; }

    public TimeSpan? StartTime { get; set; }

    public TimeSpan? EndTime { get; set; }

    public int? StartDepotId { get; set; }

    public bool? Active { get; set; }
}

public class DriverShiftResponseDto
{
    public int Id { get; set; }
    public int DriverId { get; set; }
    public int? DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public int? StartDepotId { get; set; }
    public bool Active { get; set; }

    public DriverListItemDto? Driver { get; set; }
    public DepotListItemDto? StartDepot { get; set; }
}

// Driver Time Off DTOs
public class CreateDriverTimeOffDto
{
    [Required]
    public int DriverId { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    public string? Reason { get; set; }
}

public class UpdateDriverTimeOffDto
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Reason { get; set; }
}

public class DriverTimeOffResponseDto
{
    public int Id { get; set; }
    public int DriverId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? Reason { get; set; }
    public DateTime CreatedAt { get; set; }

    public DriverListItemDto? Driver { get; set; }
}