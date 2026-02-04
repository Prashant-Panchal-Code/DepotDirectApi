using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace DepotDirectApi.Models.DTOs;

// Tractor DTOs
public class CreateTractorDto
{
    [Required]
    [StringLength(50, MinimumLength = 1)]
    public string TractorCode { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string TractorName { get; set; } = string.Empty;

    [Required]
    [StringLength(20, MinimumLength = 1)]
    public string LicensePlate { get; set; } = string.Empty;

    [Required]
    public int HaulierId { get; set; }

    public int? RegionId { get; set; }

    [StringLength(20)]
    public string? Status { get; set; }

    public bool? PumpAvailable { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Pump flow rate must be positive")]
    public decimal? PumpFlowRateLpm { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Curb weight must be positive")]
    public decimal? CurbWeightKg { get; set; }

    [Range(1, 20, ErrorMessage = "Number of axles must be between 1 and 20")]
    public int? NumberOfAxles { get; set; }

    public JsonDocument? AxleConfiguration { get; set; }
    public JsonDocument? Metadata { get; set; }
}

public class UpdateTractorDto
{
    [StringLength(50)]
    public string? TractorCode { get; set; }

    [StringLength(100)]
    public string? TractorName { get; set; }

    [StringLength(20)]
    public string? LicensePlate { get; set; }

    public int? RegionId { get; set; }

    [StringLength(20)]
    public string? Status { get; set; }

    public bool? PumpAvailable { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Pump flow rate must be positive")]
    public decimal? PumpFlowRateLpm { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Curb weight must be positive")]
    public decimal? CurbWeightKg { get; set; }

    [Range(1, 20, ErrorMessage = "Number of axles must be between 1 and 20")]
    public int? NumberOfAxles { get; set; }

    public JsonDocument? AxleConfiguration { get; set; }
    public JsonDocument? Metadata { get; set; }
}

public class TractorResponseDto
{
    public int Id { get; set; }
    public string TractorCode { get; set; } = string.Empty;
    public string TractorName { get; set; } = string.Empty;
    public string LicensePlate { get; set; } = string.Empty;
    public int HaulierId { get; set; }
    public int? RegionId { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool PumpAvailable { get; set; }
    public decimal? PumpFlowRateLpm { get; set; }
    public decimal? CurbWeightKg { get; set; }
    public int? NumberOfAxles { get; set; }
    public JsonDocument? AxleConfiguration { get; set; }
    public JsonDocument? Metadata { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    public HaulierListItemDto? Haulier { get; set; }
    public RegionDto? Region { get; set; }
}

public class TractorListItemDto
{
    public int Id { get; set; }
    public string TractorCode { get; set; } = string.Empty;
    public string TractorName { get; set; } = string.Empty;
    public string LicensePlate { get; set; } = string.Empty;
    public int HaulierId { get; set; }
    public string HaulierName { get; set; } = string.Empty;
    public int? RegionId { get; set; }
    public string? RegionName { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool PumpAvailable { get; set; }
    public decimal? PumpFlowRateLpm { get; set; }
    public decimal? CurbWeightKg { get; set; }
    public int? NumberOfAxles { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

// Trailer DTOs
public class CreateTrailerDto
{
    [Required]
    [StringLength(50, MinimumLength = 1)]
    public string TrailerCode { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string TrailerName { get; set; } = string.Empty;

    [Required]
    [StringLength(20, MinimumLength = 1)]
    public string LicensePlate { get; set; } = string.Empty;

    [Required]
    public int HaulierId { get; set; }

    public int? RegionId { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Unladen weight must be positive")]
    public decimal? UnladenWeightKg { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Max payload must be positive")]
    public decimal? MaxPayloadKg { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Max payload liters must be positive")]
    public decimal? MaxPayloadLiters { get; set; }

    [Range(1, 20, ErrorMessage = "Number of axles must be between 1 and 20")]
    public int? NumberOfAxles { get; set; }

    [StringLength(20)]
    public string? Status { get; set; }

    public JsonDocument? AxleConfiguration { get; set; }
    public JsonDocument? Metadata { get; set; }
}

public class UpdateTrailerDto
{
    [StringLength(50)]
    public string? TrailerCode { get; set; }

    [StringLength(100)]
    public string? TrailerName { get; set; }

    [StringLength(20)]
    public string? LicensePlate { get; set; }

    public int? RegionId { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Unladen weight must be positive")]
    public decimal? UnladenWeightKg { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Max payload must be positive")]
    public decimal? MaxPayloadKg { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Max payload liters must be positive")]
    public decimal? MaxPayloadLiters { get; set; }

    [Range(1, 20, ErrorMessage = "Number of axles must be between 1 and 20")]
    public int? NumberOfAxles { get; set; }

    [StringLength(20)]
    public string? Status { get; set; }

    public JsonDocument? AxleConfiguration { get; set; }
    public JsonDocument? Metadata { get; set; }
}

public class TrailerResponseDto
{
    public int Id { get; set; }
    public string TrailerCode { get; set; } = string.Empty;
    public string TrailerName { get; set; } = string.Empty;
    public string LicensePlate { get; set; } = string.Empty;
    public int HaulierId { get; set; }
    public int? RegionId { get; set; }
    public decimal? UnladenWeightKg { get; set; }
    public decimal? MaxPayloadKg { get; set; }
    public decimal? MaxPayloadLiters { get; set; }
    public int? NumberOfAxles { get; set; }
    public string Status { get; set; } = string.Empty;
    public JsonDocument? AxleConfiguration { get; set; }
    public JsonDocument? Metadata { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    public HaulierListItemDto? Haulier { get; set; }
    public RegionDto? Region { get; set; }
    public List<TrailerCompartmentResponseDto>? TrailerCompartments { get; set; }
}

public class TrailerListItemDto
{
    public int Id { get; set; }
    public string TrailerCode { get; set; } = string.Empty;
    public string TrailerName { get; set; } = string.Empty;
    public string LicensePlate { get; set; } = string.Empty;
    public int HaulierId { get; set; }
    public string HaulierName { get; set; } = string.Empty;
    public int? RegionId { get; set; }
    public string? RegionName { get; set; }
    public decimal? UnladenWeightKg { get; set; }
    public decimal? MaxPayloadKg { get; set; }
    public decimal? MaxPayloadLiters { get; set; }
    public int? NumberOfAxles { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

// Trailer Compartment DTOs
public class CreateTrailerCompartmentDto
{
    [Required]
    public int TrailerId { get; set; }

    [Required]
    [Range(1, 100, ErrorMessage = "Compartment number must be between 1 and 100")]
    public int CompartmentNumber { get; set; }

    [Required]
    [Range(0.1, double.MaxValue, ErrorMessage = "Capacity must be greater than 0")]
    public decimal CapacityL { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Min volume must be positive")]
    public decimal? MinVolumeL { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Safe fill must be positive")]
    public decimal? SafeFillL { get; set; }

    public bool? MustUse { get; set; }
    public bool? PartialLoadAllowed { get; set; }
    public JsonDocument? Metadata { get; set; }
}

public class UpdateTrailerCompartmentDto
{
    [Range(1, 100, ErrorMessage = "Compartment number must be between 1 and 100")]
    public int? CompartmentNumber { get; set; }

    [Range(0.1, double.MaxValue, ErrorMessage = "Capacity must be greater than 0")]
    public decimal? CapacityL { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Min volume must be positive")]
    public decimal? MinVolumeL { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Safe fill must be positive")]
    public decimal? SafeFillL { get; set; }

    public bool? MustUse { get; set; }
    public bool? PartialLoadAllowed { get; set; }
    public JsonDocument? Metadata { get; set; }
}

public class TrailerCompartmentResponseDto
{
    public int Id { get; set; }
    public int TrailerId { get; set; }
    public int CompartmentNumber { get; set; }
    public decimal CapacityL { get; set; }
    public decimal? MinVolumeL { get; set; }
    public decimal? SafeFillL { get; set; }
    public bool MustUse { get; set; }
    public bool PartialLoadAllowed { get; set; }
    public JsonDocument? Metadata { get; set; }

    public TrailerListItemDto? Trailer { get; set; }
    public List<ProductListItemDto>? AllowedProducts { get; set; }
}