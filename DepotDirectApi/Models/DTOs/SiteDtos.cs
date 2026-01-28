using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace DepotDirectApi.Models.DTOs;

/// <summary>
/// DTO for creating a new site (User module)
/// </summary>
public class CreateSiteDto
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string SiteCode { get; set; } = string.Empty;

    [Required]
    [StringLength(255, MinimumLength = 1)]
    public string SiteName { get; set; } = string.Empty;

    [Required]
    public int RegionId { get; set; }
}

/// <summary>
/// DTO for updating an existing site
/// </summary>
public class UpdateSiteDto
{
    [StringLength(100)]
    public string? SiteCode { get; set; }

    [StringLength(255)]
    public string? SiteName { get; set; }

    [StringLength(50)]
    public string? Shortcode { get; set; }

    [Range(-90, 90)]
    public decimal? Latitude { get; set; }

    [Range(-180, 180)]
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

    [StringLength(100)]
    public string? ContactPerson { get; set; }

    [StringLength(50)]
    public string? Phone { get; set; }

    [EmailAddress]
    [StringLength(255)]
    public string? Email { get; set; }

    public JsonDocument? OperatingHours { get; set; }

    public int? DepotId { get; set; }

    public bool? DeliveryStopped { get; set; }

    public bool? PumpedRequired { get; set; }

    public JsonDocument? Metadata { get; set; }
}

/// <summary>
/// Full site response DTO
/// </summary>
public class SiteResponseDto
{
    public int Id { get; set; }
    public string SiteCode { get; set; } = string.Empty;
    public string SiteName { get; set; } = string.Empty;
    public string? Shortcode { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public string? LatLong { get; set; }
    public string? Street { get; set; }
    public string? PostalCode { get; set; }
    public string? Town { get; set; }
    public bool Active { get; set; }
    public string Priority { get; set; } = "Medium";
    public string? ContactPerson { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public JsonDocument? OperatingHours { get; set; }
    public int? DepotId { get; set; }
    public bool DeliveryStopped { get; set; }
    public bool PumpedRequired { get; set; }
    public int CountryId { get; set; }
    public int CompanyId { get; set; }
    public JsonDocument? Metadata { get; set; }
    public int? CreatedBy { get; set; }
    public int? LastUpdatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    // Navigation properties
    public CountryDto? Country { get; set; }
    public CompanyDto? Company { get; set; }
    public List<RegionDto>? Regions { get; set; }
}

/// <summary>
/// Simplified site list item DTO
/// </summary>
public class SiteListItemDto
{
    public int Id { get; set; }
    public string SiteCode { get; set; } = string.Empty;
    public string SiteName { get; set; } = string.Empty;
    public string? Town { get; set; }
    public bool Active { get; set; }
    public string Priority { get; set; } = "Medium";
    public int CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public int CountryId { get; set; }
    public string CountryName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Simplified site DTO
/// </summary>
public class SiteDto
{
    public int Id { get; set; }
    public string SiteCode { get; set; } = string.Empty;
    public string SiteName { get; set; } = string.Empty;
    public string? Shortcode { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public string? Town { get; set; }
    public bool Active { get; set; }
    public string Priority { get; set; } = "Medium";
    public int CountryId { get; set; }
    public int CompanyId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// DTO for assigning a region to a site
/// </summary>
public class AssignRegionToSiteDto
{
    [Required]
    public int RegionId { get; set; }

    [StringLength(100)]
    public string? SiteCode { get; set; }

    public JsonDocument? Metadata { get; set; }
}

/// <summary>
/// DTO for assigning a site to a region
/// </summary>
public class AssignSiteToRegionDto
{
    [Required]
    public int SiteId { get; set; }

    [StringLength(100)]
    public string? SiteCode { get; set; }

    public JsonDocument? Metadata { get; set; }
}

/// <summary>
/// DTO for region-site mapping
/// </summary>
public class RegionSiteDto
{
    public int Id { get; set; }
    public int SiteId { get; set; }
    public string SiteName { get; set; } = string.Empty;
    public string SiteCode { get; set; } = string.Empty;
    public int RegionId { get; set; }
    public string RegionName { get; set; } = string.Empty;
    public string? RegionSiteCode { get; set; }
    public JsonDocument? Metadata { get; set; }
    public int? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
