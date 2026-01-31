using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace DepotDirectApi.Models.DTOs;

public class CreateTankDto
{
    [Required]
    public string TankCode { get; set; } = string.Empty;

    [Required]
    public int SiteId { get; set; }

    // Optional product association when creating a tank
    public int? ProductId { get; set; }
}

public class UpdateTankDto
{
    public int? ProductId { get; set; }
    public decimal? CapacityL { get; set; }
    public decimal? SafeFillL { get; set; }
    public decimal? DeadstockL { get; set; }
    public decimal? DischargeRateLpm { get; set; }
    public bool? Active { get; set; }
    public JsonDocument? Metadata { get; set; }
}

public class SiteTankDto
{
    public int Id { get; set; }
    public int SiteId { get; set; }
    public int? ProductId { get; set; }
    public string TankCode { get; set; } = string.Empty;
    public decimal CapacityL { get; set; }
    public decimal SafeFillL { get; set; }
    public decimal DeadstockL { get; set; }
    public decimal? DischargeRateLpm { get; set; }
    public bool Active { get; set; }
    public JsonDocument? Metadata { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class TankReadingDto
{
    public int Id { get; set; }
    public int TankId { get; set; }
    public DateTime ReadingTimestamp { get; set; }
    public string ReadingMethod { get; set; } = string.Empty;
    public decimal CurrentVolumeL { get; set; }
    public decimal? SalesSinceLastReadingL { get; set; }
    public decimal? AvgDailySalesL { get; set; }
}

public class CreateTankReadingDto
{
    [Required]
    public string ReadingMethod { get; set; } = string.Empty; // e.g. "ATG" or "Manual"

    [Required]
    public decimal CurrentVolumeL { get; set; }

    public DateTime? ReadingTimestamp { get; set; }

    public JsonDocument? Metadata { get; set; }
}

public class TankDeliveryDto
{
    public int Id { get; set; }
    public int TankId { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal? PlannedQuantityL { get; set; }
    public decimal? ConfirmedQuantityL { get; set; }
    public DateTime? ScheduledArrival { get; set; }
    public DateTime? ActualArrival { get; set; }
}

public class SalesPatternDto
{
    public int Id { get; set; }
    public int TankId { get; set; }
    public int DayOfWeek { get; set; }
    public int HourOfDay { get; set; }
    public decimal WeightFactor { get; set; }
    public decimal AvgHourlySalesL { get; set; }
}

public class SiteTankWithInventoryDto : SiteTankDto
{
    public List<TankReadingDto> Readings { get; set; } = new List<TankReadingDto>();
}

public class SiteTankFullDto : SiteTankDto
{
    public List<TankReadingDto> LastReadings { get; set; } = new List<TankReadingDto>();
    public List<TankDeliveryDto> Deliveries { get; set; } = new List<TankDeliveryDto>();
    public List<SalesPatternDto> SalesPatterns { get; set; } = new List<SalesPatternDto>();
}
