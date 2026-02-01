using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace DepotDirectApi.Models.DTOs;

public class CreateDepotSiteDto
{
    [Required]
    public int DepotId { get; set; }

    [Required]
    public int SiteId { get; set; }

    [Required]
    [Range(0.01, 9999999.99, ErrorMessage = "Distance must be greater than 0")]
    public decimal DistanceKm { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Travel time must be at least 1 minute")]
    public int TravelTimeMins { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Return time must be a positive number")]
    public int? ReturnTimeMins { get; set; }

    public bool Active { get; set; } = true;

    public bool IsPrimary { get; set; } = false;

    [Range(0, 999999.99, ErrorMessage = "Transport rate must be a positive number")]
    public decimal? TransportRate { get; set; }

    public JsonDocument? Metadata { get; set; }
}

public class UpdateDepotSiteDto
{
    [Range(0.01, 9999999.99, ErrorMessage = "Distance must be greater than 0")]
    public decimal? DistanceKm { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Travel time must be at least 1 minute")]
    public int? TravelTimeMins { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Return time must be a positive number")]
    public int? ReturnTimeMins { get; set; }

    public bool? Active { get; set; }

    public bool? IsPrimary { get; set; }

    [Range(0, 999999.99, ErrorMessage = "Transport rate must be a positive number")]
    public decimal? TransportRate { get; set; }

    public JsonDocument? Metadata { get; set; }
}

public class DepotSiteDto
{
    public int Id { get; set; }
    public int DepotId { get; set; }
    public string DepotCode { get; set; } = string.Empty;
    public string DepotName { get; set; } = string.Empty;
    public int SiteId { get; set; }
    public string SiteCode { get; set; } = string.Empty;
    public string SiteName { get; set; } = string.Empty;
    public decimal DistanceKm { get; set; }
    public int TravelTimeMins { get; set; }
    public int? ReturnTimeMins { get; set; }
    public bool Active { get; set; }
    public bool IsPrimary { get; set; }
    public decimal? TransportRate { get; set; }
    public JsonDocument? Metadata { get; set; }
    public int? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}

public class DepotSiteListItemDto
{
    public int Id { get; set; }
    public int DepotId { get; set; }
    public string DepotCode { get; set; } = string.Empty;
    public string DepotName { get; set; } = string.Empty;
    public int SiteId { get; set; }
    public string SiteCode { get; set; } = string.Empty;
    public string SiteName { get; set; } = string.Empty;
    public decimal DistanceKm { get; set; }
    public int TravelTimeMins { get; set; }
    public int? ReturnTimeMins { get; set; }
    public bool Active { get; set; }
    public bool IsPrimary { get; set; }
    public decimal? TransportRate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class DepotSiteResponseDto
{
    public int Id { get; set; }
    public int DepotId { get; set; }
    public int SiteId { get; set; }
    public decimal DistanceKm { get; set; }
    public int TravelTimeMins { get; set; }
    public int? ReturnTimeMins { get; set; }
    public bool Active { get; set; }
    public bool IsPrimary { get; set; }
    public decimal? TransportRate { get; set; }
    public JsonDocument? Metadata { get; set; }
    public int? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    // Navigation properties
    public DepotSiteDepotDto? Depot { get; set; }
    public DepotSiteSiteDto? Site { get; set; }
}

public class DepotSiteDepotDto
{
    public int Id { get; set; }
    public string DepotCode { get; set; } = string.Empty;
    public string DepotName { get; set; } = string.Empty;
    public string? Town { get; set; }
    public bool Active { get; set; }
    public string Priority { get; set; } = "Medium";
    public int CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
}

public class DepotSiteSiteDto
{
    public int Id { get; set; }
    public string SiteCode { get; set; } = string.Empty;
    public string SiteName { get; set; } = string.Empty;
    public string? Town { get; set; }
    public bool Active { get; set; }
    public string Priority { get; set; } = "Medium";
    public int CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
}