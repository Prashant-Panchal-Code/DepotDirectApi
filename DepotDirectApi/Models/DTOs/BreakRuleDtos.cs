using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace DepotDirectApi.Models.DTOs;

// Break Rule DTOs
public class CreateBreakRuleDto
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string RuleName { get; set; } = string.Empty;

    [Required]
    public int CompanyId { get; set; }

    [Required]
    [Range(1, 480, ErrorMessage = "Max continuous drive must be between 1 and 480 minutes")]
    public int MaxContinuousDriveMins { get; set; }

    [Required]
    [Range(1, 120, ErrorMessage = "Min break duration must be between 1 and 120 minutes")]
    public int MinBreakDurationMins { get; set; }

    [Required]
    [Range(1, 720, ErrorMessage = "Max daily drive must be between 1 and 720 minutes")]
    public int MaxDailyDriveMins { get; set; }

    [Required]
    [Range(1, 1440, ErrorMessage = "Min daily rest must be between 1 and 1440 minutes")]
    public int MinDailyRestMins { get; set; }

    public bool? Active { get; set; }
}

public class UpdateBreakRuleDto
{
    [StringLength(100)]
    public string? RuleName { get; set; }

    [Range(1, 480, ErrorMessage = "Max continuous drive must be between 1 and 480 minutes")]
    public int? MaxContinuousDriveMins { get; set; }

    [Range(1, 120, ErrorMessage = "Min break duration must be between 1 and 120 minutes")]
    public int? MinBreakDurationMins { get; set; }

    [Range(1, 720, ErrorMessage = "Max daily drive must be between 1 and 720 minutes")]
    public int? MaxDailyDriveMins { get; set; }

    [Range(1, 1440, ErrorMessage = "Min daily rest must be between 1 and 1440 minutes")]
    public int? MinDailyRestMins { get; set; }

    public bool? Active { get; set; }
}

public class BreakRuleResponseDto
{
    public int Id { get; set; }
    public string RuleName { get; set; } = string.Empty;
    public int CompanyId { get; set; }
    public int MaxContinuousDriveMins { get; set; }
    public int MinBreakDurationMins { get; set; }
    public int MaxDailyDriveMins { get; set; }
    public int MinDailyRestMins { get; set; }
    public bool Active { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    public CompanyDto Company { get; set; } = new CompanyDto();
}

public class BreakRuleListItemDto
{
    public int Id { get; set; }
    public string RuleName { get; set; } = string.Empty;
    public int CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public int MaxContinuousDriveMins { get; set; }
    public int MinBreakDurationMins { get; set; }
    public int MaxDailyDriveMins { get; set; }
    public int MinDailyRestMins { get; set; }
    public bool Active { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}