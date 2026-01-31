using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace DepotDirectApi.Models.DTOs;

public class DepotProductDto
{
    public int Id { get; set; }
    public int DepotId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductCode { get; set; } = string.Empty;
    public decimal? Density { get; set; }
    public decimal? PlanningTemperature { get; set; }
    public decimal LoadingRateLpm { get; set; }
    public bool ProductAvailable { get; set; }
    public decimal? CostPerLitre { get; set; }
    public bool OfftakeLimitActive { get; set; }
    public decimal? DailyMinLimitL { get; set; }
    public decimal? DailyMaxLimitL { get; set; }
    public JsonDocument? Metadata { get; set; }
    public int? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateDepotProductDto
{
    [Required]
    public int ProductId { get; set; }
    public decimal? Density { get; set; }
    public decimal? PlanningTemperature { get; set; }
    public decimal? LoadingRateLpm { get; set; }
    public bool? ProductAvailable { get; set; }
    public decimal? CostPerLitre { get; set; }
    public bool? OfftakeLimitActive { get; set; }
    public decimal? DailyMinLimitL { get; set; }
    public decimal? DailyMaxLimitL { get; set; }
    public JsonDocument? Metadata { get; set; }
}

public class UpdateDepotProductDto
{
    public decimal? Density { get; set; }
    public decimal? PlanningTemperature { get; set; }
    public decimal? LoadingRateLpm { get; set; }
    public bool? ProductAvailable { get; set; }
    public decimal? CostPerLitre { get; set; }
    public bool? OfftakeLimitActive { get; set; }
    public decimal? DailyMinLimitL { get; set; }
    public decimal? DailyMaxLimitL { get; set; }
    public JsonDocument? Metadata { get; set; }
}
