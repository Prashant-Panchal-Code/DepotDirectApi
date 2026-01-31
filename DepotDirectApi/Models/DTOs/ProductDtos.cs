using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace DepotDirectApi.Models.DTOs;

public class ProductListItemDto
{
    public int Id { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string? ShortName { get; set; }
    public int CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public int RegionId { get; set; }
    public string RegionName { get; set; } = string.Empty;
    public bool Active { get; set; }
    public bool IsHazardous { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
