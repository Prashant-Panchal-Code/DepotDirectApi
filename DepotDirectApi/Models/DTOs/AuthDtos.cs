using System.ComponentModel.DataAnnotations;

namespace DepotDirectApi.Models.DTOs;

/// <summary>
/// Login request with email and password
/// </summary>
public class EmailLoginRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// Login response with user details and JWT token
/// </summary>
public class LoginResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Token { get; set; }
    public string TokenType { get; set; } = "Bearer";
    public DateTime? ExpiresAt { get; set; }
    public UserDto? User { get; set; }
}

/// <summary>
/// Simplified user info for authentication responses
/// </summary>
public class AuthUserDto
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public int? CompanyId { get; set; }
    public string? CompanyName { get; set; }
    public int RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public bool Active { get; set; }
}