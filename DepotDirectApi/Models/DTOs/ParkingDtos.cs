using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace DepotDirectApi.Models.DTOs;

public class CreateParkingDto
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string ParkingCode { get; set; } = string.Empty;

    [Required]
    [StringLength(255, MinimumLength = 1)]
    public string ParkingName { get; set; } = string.Empty;

    [Required]
    public int RegionId { get; set; }
}

public class UpdateParkingDto
{
    [StringLength(100)]
    public string? ParkingCode { get; set; }

    [StringLength(255)]
    public string? ParkingName { get; set; }

    [StringLength(50)]
    public string? Shortcode { get; set; }

    [Range(-90.0, 90.0, ErrorMessage = "Latitude must be between -90 and 90")]
    public decimal? Latitude { get; set; }

    [Range(-180.0, 180.0, ErrorMessage = "Longitude must be between -180 and 180")]
    public decimal? Longitude { get; set; }

    [StringLength(500)]
    public string? Street { get; set; }

    [StringLength(20)]
    public string? PostalCode { get; set; }

    [StringLength(100)]
    public string? Town { get; set; }

    public bool? Active { get; set; }

    [StringLength(100)]
    public string? ManagerName { get; set; }

    [StringLength(20)]
    public string? ManagerPhone { get; set; }

    [EmailAddress]
    [StringLength(100)]
    public string? ManagerEmail { get; set; }

    [StringLength(200)]
    public string? EmergencyContact { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Parking spaces must be at least 1")]
    public int? ParkingSpaces { get; set; }

    public JsonDocument? Metadata { get; set; }
}

public class ParkingResponseDto
{
    public int Id { get; set; }
    public string ParkingCode { get; set; } = string.Empty;
    public string ParkingName { get; set; } = string.Empty;
    public string? Shortcode { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public string? LatLong { get; set; }
    public string? Street { get; set; }
    public string? PostalCode { get; set; }
    public string? Town { get; set; }
    public bool Active { get; set; }
    public string? ManagerName { get; set; }
    public string? ManagerPhone { get; set; }
    public string? ManagerEmail { get; set; }
    public string? EmergencyContact { get; set; }
    public int? ParkingSpaces { get; set; }
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

public class ParkingListItemDto
{
    public int Id { get; set; }
    public string ParkingCode { get; set; } = string.Empty;
    public string ParkingName { get; set; } = string.Empty;
    public string? Town { get; set; }
    public bool Active { get; set; }
    public int? ParkingSpaces { get; set; }
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

public class AssignParkingToRegionDto
{
    [Required]
    public int RegionId { get; set; }

    [StringLength(100)]
    public string? ParkingCode { get; set; }

    public JsonDocument? Metadata { get; set; }
}

public class RegionParkingDto
{
    public int Id { get; set; }
    public int ParkingId { get; set; }
    public string ParkingName { get; set; } = string.Empty;
    public string ParkingCode { get; set; } = string.Empty;
    public int RegionId { get; set; }
    public string RegionName { get; set; } = string.Empty;
    public string? RegionParkingCode { get; set; }
    public JsonDocument? Metadata { get; set; }
    public int? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}