using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace DepotDirectApi.Models.DTOs;

public class CreateHaulierDto
{
    [Required]
    public int RegionId { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string HaulierCode { get; set; } = string.Empty;

    [Required]
    [StringLength(255, MinimumLength = 1)]
    public string HaulierName { get; set; } = string.Empty;

    [StringLength(50)]
    public string? TaxId { get; set; }

    [StringLength(100)]
    public string? ContractNumber { get; set; }

    public DateTime? ContractExpiry { get; set; }

    [StringLength(100)]
    public string? ContactName { get; set; }

    [EmailAddress]
    [StringLength(255)]
    public string? ContactEmail { get; set; }

    [StringLength(20)]
    public string? ContactPhone { get; set; }

    public bool? Active { get; set; }

    public JsonDocument? Metadata { get; set; }
}

public class UpdateHaulierDto
{
    [StringLength(100)]
    public string? HaulierCode { get; set; }

    [StringLength(255)]
    public string? HaulierName { get; set; }

    [StringLength(50)]
    public string? TaxId { get; set; }

    [StringLength(100)]
    public string? ContractNumber { get; set; }

    public DateTime? ContractExpiry { get; set; }

    [StringLength(100)]
    public string? ContactName { get; set; }

    [EmailAddress]
    [StringLength(255)]
    public string? ContactEmail { get; set; }

    [StringLength(20)]
    public string? ContactPhone { get; set; }

    public bool? Active { get; set; }

    public JsonDocument? Metadata { get; set; }
}

public class HaulierResponseDto
{
    public int Id { get; set; }
    public int RegionId { get; set; }
    public string HaulierCode { get; set; } = string.Empty;
    public string HaulierName { get; set; } = string.Empty;
    public string? TaxId { get; set; }
    public string? ContractNumber { get; set; }
    public DateTime? ContractExpiry { get; set; }
    public string? ContactName { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public bool Active { get; set; }
    public JsonDocument? Metadata { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    public RegionDto? Region { get; set; }
}

public class HaulierListItemDto
{
    public int Id { get; set; }
    public int RegionId { get; set; }
    public string RegionName { get; set; } = string.Empty;
    public string HaulierCode { get; set; } = string.Empty;
    public string HaulierName { get; set; } = string.Empty;
    public string? TaxId { get; set; }
    public string? ContractNumber { get; set; }
    public DateTime? ContractExpiry { get; set; }
    public string? ContactName { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public bool Active { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}