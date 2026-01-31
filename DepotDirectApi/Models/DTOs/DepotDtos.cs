using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace DepotDirectApi.Models.DTOs;

public class CreateDepotDto
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string DepotCode { get; set; } = string.Empty;

    [Required]
    [StringLength(255, MinimumLength = 1)]
    public string DepotName { get; set; } = string.Empty;

    [Required]
    public int RegionId { get; set; }
}

public class UpdateDepotDto
{
    [StringLength(100)]
    public string? DepotCode { get; set; }

    [StringLength(255)]
    public string? DepotName { get; set; }

    [StringLength(50)]
    public string? Shortcode { get; set; }

    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }

    [StringLength(500)]
    public string? Street { get; set; }

    [StringLength(20)]
    public string? PostalCode { get; set; }

    [StringLength(100)]
    public string? Town { get; set; }

    public bool? Active { get; set; }

    [RegularExpression("^(High|Medium|Low)$", ErrorMessage = "Priority must be High, Medium, or Low")]
    public string? Priority { get; set; }

    public int? LoadingBays { get; set; }
    public JsonDocument? OperatingHours { get; set; }
    public string? ManagerName { get; set; }
    public string? ManagerPhone { get; set; }
    public string? ManagerEmail { get; set; }
    public string? EmergencyContact { get; set; }
    public int? AverageLoadingTime { get; set; }
    public string? MaxTruckSize { get; set; }
    public string? Certifications { get; set; }
    public JsonDocument? Metadata { get; set; }
}

public class DepotResponseDto
{
    public int Id { get; set; }
    public string DepotCode { get; set; } = string.Empty;
    public string DepotName { get; set; } = string.Empty;
    public string? Shortcode { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public string? LatLong { get; set; }
    public string? Street { get; set; }
    public string? PostalCode { get; set; }
    public string? Town { get; set; }
    public bool Active { get; set; }
    public string Priority { get; set; } = "Medium";
    public int? LoadingBays { get; set; }
    public JsonDocument? OperatingHours { get; set; }
    public string? ManagerName { get; set; }
    public string? ManagerPhone { get; set; }
    public string? ManagerEmail { get; set; }
    public string? EmergencyContact { get; set; }
    public int? AverageLoadingTime { get; set; }
    public string? MaxTruckSize { get; set; }
    public string? Certifications { get; set; }
    public int CountryId { get; set; }
    public int CompanyId { get; set; }
    public JsonDocument? Metadata { get; set; }
    public int? CreatedBy { get; set; }
    public int? LastUpdatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    public CountryDto? Country { get; set; }
    public CompanyDto? Company { get; set; }
    public List<RegionDto>? Regions { get; set; }
}

public class DepotListItemDto
{
    public int Id { get; set; }
    public string DepotCode { get; set; } = string.Empty;
    public string DepotName { get; set; } = string.Empty;
    public string? Town { get; set; }
    public bool Active { get; set; }
    public string Priority { get; set; } = "Medium";
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public string? LatLong { get; set; }
    public string? Street { get; set; }
    public string? PostalCode { get; set; }
    public int CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public int CountryId { get; set; }
    public string CountryName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class AssignDepotToRegionDto
{
    [Required]
    public int RegionId { get; set; }

    [StringLength(100)]
    public string? DepotCode { get; set; }

    public JsonDocument? Metadata { get; set; }
}

public class RegionDepotDto
{
    public int Id { get; set; }
    public int DepotId { get; set; }
    public string DepotName { get; set; } = string.Empty;
    public string DepotCode { get; set; } = string.Empty;
    public int RegionId { get; set; }
    public string RegionName { get; set; } = string.Empty;
    public string? RegionDepotCode { get; set; }
    public JsonDocument? Metadata { get; set; }
    public int? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
