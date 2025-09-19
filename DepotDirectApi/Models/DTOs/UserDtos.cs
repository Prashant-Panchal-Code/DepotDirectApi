using System.Text.Json;

namespace DepotDirectApi.Models.DTOs;

public class UserDto
{
    public int Id { get; set; }
    public int? CompanyId { get; set; }
    public string? CompanyName { get; set; }
    public int RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public bool Active { get; set; }
    public JsonDocument? Metadata { get; set; }
    public int? CreatedBy { get; set; }
    public int? LastUpdatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}

public class CreateUserDto
{
    public int? CompanyId { get; set; }
    public int RoleId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public bool Active { get; set; } = true;
    public JsonDocument? Metadata { get; set; }
}

public class UpdateUserDto
{
    public int? CompanyId { get; set; }
    public int? RoleId { get; set; }
    public string? Email { get; set; }
    public string? Password { get; set; }
    public string? FullName { get; set; }
    public string? Phone { get; set; }
    public bool? Active { get; set; }
    public JsonDocument? Metadata { get; set; }
}