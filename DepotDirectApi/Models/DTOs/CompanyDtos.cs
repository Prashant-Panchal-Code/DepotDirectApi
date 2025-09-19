using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace DepotDirectApi.Models.DTOs;

public class CreateCompanyDto
{
    [Required]
    [StringLength(255, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;

    [StringLength(50)]
    public string? CompanyCode { get; set; }

    [Required]
    public int CountryId { get; set; }

    [StringLength(1000)]
    public string? Description { get; set; }

    public JsonDocument? Metadata { get; set; }
}

public class UpdateCompanyDto
{
    [StringLength(255, MinimumLength = 1)]
    public string? Name { get; set; }

    [StringLength(50)]
    public string? CompanyCode { get; set; }

    public int? CountryId { get; set; }

    [StringLength(1000)]
    public string? Description { get; set; }

    public JsonDocument? Metadata { get; set; }
}

public class CompanyResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? CompanyCode { get; set; }
    public int CountryId { get; set; }
    public string? Description { get; set; }
    public JsonDocument? Metadata { get; set; }
    public int? CreatedBy { get; set; }
    public int? LastUpdatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    // Navigation properties
    public CountryDto? Country { get; set; }
}

public class CompanyListItemDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? CompanyCode { get; set; }
    public int CountryId { get; set; }
    public string CountryName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CompanyDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? CompanyCode { get; set; }
    public int CountryId { get; set; }
    public string? Description { get; set; }
    public object? Metadata { get; set; }
    public int? CreatedBy { get; set; }
    public int? LastUpdatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}