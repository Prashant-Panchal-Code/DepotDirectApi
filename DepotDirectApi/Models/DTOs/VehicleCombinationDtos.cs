using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace DepotDirectApi.Models.DTOs;

// Vehicle Combination DTOs
public class CreateVehicleCombinationDto
{
    [Required]
    [StringLength(50, MinimumLength = 1)]
    public string CombinationCode { get; set; } = string.Empty;

    [Required]
    public int TractorId { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Gross weight limit must be positive")]
    public decimal? GrossWeightLimitKg { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Total capacity must be positive")]
    public decimal? TotalCapacityL { get; set; }

    public bool? Active { get; set; }
    public bool? IsDefault { get; set; }
}

public class UpdateVehicleCombinationDto
{
    [StringLength(50)]
    public string? CombinationCode { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Gross weight limit must be positive")]
    public decimal? GrossWeightLimitKg { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Total capacity must be positive")]
    public decimal? TotalCapacityL { get; set; }

    public bool? Active { get; set; }
    public bool? IsDefault { get; set; }
}

public class VehicleCombinationResponseDto
{
    public int Id { get; set; }
    public string CombinationCode { get; set; } = string.Empty;
    public int TractorId { get; set; }
    public decimal? GrossWeightLimitKg { get; set; }
    public decimal? TotalCapacityL { get; set; }
    public bool Active { get; set; }
    public bool IsDefault { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    public TractorListItemDto? Tractor { get; set; }
    public List<TrailerListItemDto>? Trailers { get; set; }
}

public class VehicleCombinationListItemDto
{
    public int Id { get; set; }
    public string CombinationCode { get; set; } = string.Empty;
    public int TractorId { get; set; }
    public string TractorName { get; set; } = string.Empty;
    public string TractorCode { get; set; } = string.Empty;
    public decimal? GrossWeightLimitKg { get; set; }
    public decimal? TotalCapacityL { get; set; }
    public bool Active { get; set; }
    public bool IsDefault { get; set; }
    public int TrailerCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

// Add/Remove Trailer to/from Vehicle Combination DTOs
public class AddTrailerToCombinationDto
{
    [Required]
    public int TrailerId { get; set; }

    [Range(1, 10, ErrorMessage = "Sequence number must be between 1 and 10")]
    public int SequenceNumber { get; set; } = 1;
}

public class VehicleCombinationTrailerResponseDto
{
    public int CombinationId { get; set; }
    public int TrailerId { get; set; }
    public int SequenceNumber { get; set; }

    public VehicleCombinationListItemDto? VehicleCombination { get; set; }
    public TrailerListItemDto? Trailer { get; set; }
}

// Tractor Schedule DTOs
public class CreateTractorScheduleDto
{
    [Required]
    public int TractorId { get; set; }

    public int? DriverId { get; set; }

    [Required]
    [Range(0, 6, ErrorMessage = "Day of week must be between 0 (Sunday) and 6 (Saturday)")]
    public int DayOfWeek { get; set; }

    [Required]
    public TimeSpan ShiftStartTime { get; set; }

    [Required]
    public TimeSpan ShiftEndTime { get; set; }

    public int? StartDepotId { get; set; }
    public int? StartParkingId { get; set; }
    public int? EndDepotId { get; set; }
    public int? EndParkingId { get; set; }
    public bool? IsOvertime { get; set; }
    public bool? Active { get; set; }
}

public class UpdateTractorScheduleDto
{
    public int? DriverId { get; set; }

    [Range(0, 6, ErrorMessage = "Day of week must be between 0 (Sunday) and 6 (Saturday)")]
    public int? DayOfWeek { get; set; }

    public TimeSpan? ShiftStartTime { get; set; }
    public TimeSpan? ShiftEndTime { get; set; }
    public int? StartDepotId { get; set; }
    public int? StartParkingId { get; set; }
    public int? EndDepotId { get; set; }
    public int? EndParkingId { get; set; }
    public bool? IsOvertime { get; set; }
    public bool? Active { get; set; }
}

public class TractorScheduleResponseDto
{
    public int Id { get; set; }
    public int TractorId { get; set; }
    public int? DriverId { get; set; }
    public int DayOfWeek { get; set; }
    public string DayOfWeekName => GetDayOfWeekName(DayOfWeek);
    public TimeSpan ShiftStartTime { get; set; }
    public TimeSpan ShiftEndTime { get; set; }
    public int? StartDepotId { get; set; }
    public int? StartParkingId { get; set; }
    public int? EndDepotId { get; set; }
    public int? EndParkingId { get; set; }
    public bool IsOvertime { get; set; }
    public bool Active { get; set; }
    public int? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    public TractorListItemDto? Tractor { get; set; }
    public DriverListItemDto? Driver { get; set; }
    public DepotListItemDto? StartDepot { get; set; }
    public ParkingListItemDto? StartParking { get; set; }
    public DepotListItemDto? EndDepot { get; set; }
    public ParkingListItemDto? EndParking { get; set; }

    private static string GetDayOfWeekName(int dayOfWeek)
    {
        return dayOfWeek switch
        {
            0 => "Sunday",
            1 => "Monday",
            2 => "Tuesday",
            3 => "Wednesday",
            4 => "Thursday",
            5 => "Friday",
            6 => "Saturday",
            _ => "Unknown"
        };
    }
}

public class TractorScheduleListItemDto
{
    public int Id { get; set; }
    public int TractorId { get; set; }
    public string TractorName { get; set; } = string.Empty;
    public string TractorCode { get; set; } = string.Empty;
    public int? DriverId { get; set; }
    public string? DriverName { get; set; }
    public int DayOfWeek { get; set; }
    public string DayOfWeekName => GetDayOfWeekName(DayOfWeek);
    public TimeSpan ShiftStartTime { get; set; }
    public TimeSpan ShiftEndTime { get; set; }
    public string? StartLocationName { get; set; }
    public string? EndLocationName { get; set; }
    public bool IsOvertime { get; set; }
    public bool Active { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    private static string GetDayOfWeekName(int dayOfWeek)
    {
        return dayOfWeek switch
        {
            0 => "Sunday",
            1 => "Monday",
            2 => "Tuesday",
            3 => "Wednesday",
            4 => "Thursday",
            5 => "Friday",
            6 => "Saturday",
            _ => "Unknown"
        };
    }
}

// Compartment Allowed Product DTOs
public class AssignProductToCompartmentDto
{
    [Required]
    public int ProductId { get; set; }
}

public class CompartmentAllowedProductResponseDto
{
    public int CompartmentId { get; set; }
    public int ProductId { get; set; }

    public TrailerCompartmentResponseDto? Compartment { get; set; }
    public ProductListItemDto? Product { get; set; }
}