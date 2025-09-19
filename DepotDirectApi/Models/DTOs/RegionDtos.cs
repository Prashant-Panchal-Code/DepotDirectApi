using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace DepotDirectApi.Models.DTOs;

public class RegionListItemDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? RegionCode { get; set; }
    public int CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class RegionResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? RegionCode { get; set; }
    public int CompanyId { get; set; }
    public JsonDocument? Metadata { get; set; }
    public int? CreatedBy { get; set; }
    public int? LastUpdatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    // Navigation properties
    public CompanyDto? Company { get; set; }
}

public class CreateRegionDto
{
    [Required]
    [StringLength(255, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;

    [StringLength(50)]
    public string? RegionCode { get; set; }

    [Required]
    public int CompanyId { get; set; }

    public JsonDocument? Metadata { get; set; }
}

public class UpdateRegionDto
{
    [StringLength(255, MinimumLength = 1)]
    public string? Name { get; set; }

    [StringLength(50)]
    public string? RegionCode { get; set; }

    public int? CompanyId { get; set; }

    public JsonDocument? Metadata { get; set; }
}

public class RegionDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? RegionCode { get; set; }
    public int CompanyId { get; set; }
    public object? Metadata { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int? CreatedBy { get; set; }
    public int? LastUpdatedBy { get; set; }
}