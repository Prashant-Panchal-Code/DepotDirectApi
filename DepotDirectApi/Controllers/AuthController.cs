using DepotDirectApi.Models.DTOs;
using DepotDirectApi.Repositories;
using DepotDirectApi.Services;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace DepotDirectApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IUserRepository userRepository, 
        IJwtTokenService jwtTokenService,
        ILogger<AuthController> logger)
    {
        _userRepository = userRepository;
        _jwtTokenService = jwtTokenService;
        _logger = logger;
    }

    /// <summary>
    /// Login with email and password - Returns JWT token
    /// </summary>
    /// <param name="request">Email and password</param>
    /// <returns>JWT token and user details if authentication successful</returns>
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] EmailLoginRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new LoginResponse
                {
                    Success = false,
                    Message = "Invalid email or password format"
                });
            }

            _logger.LogInformation("Login attempt for email: {Email}", request.Email);

            var user = await _userRepository.ValidateLoginAsync(request.Email, request.Password);

            if (user == null)
            {
                _logger.LogWarning("Failed login attempt for email: {Email}", request.Email);
                return Unauthorized(new LoginResponse
                {
                    Success = false,
                    Message = "Invalid email or password"
                });
            }

            // Generate JWT token with user claims
            var token = _jwtTokenService.GenerateToken(
                userId: user.Id,
                email: user.Email,
                fullName: user.FullName,
                roleId: user.RoleId,
                roleName: user.RoleName,
                companyId: user.CompanyId
            );

            var expiresAt = DateTime.UtcNow.AddHours(8); // Token expires in 8 hours

            _logger.LogInformation("Successful login for user: {UserId} ({Email})", user.Id, user.Email);

            return Ok(new LoginResponse
            {
                Success = true,
                Message = "Login successful",
                Token = token,
                TokenType = "Bearer",
                ExpiresAt = expiresAt,
                User = user
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for email: {Email}", request.Email);
            return StatusCode(500, new LoginResponse
            {
                Success = false,
                Message = "An error occurred during login"
            });
        }
    }

    /// <summary>
    /// Verify if user exists by email
    /// </summary>
    /// <param name="email">Email address</param>
    /// <returns>Whether user exists</returns>
    [HttpGet("check-email")]
    public async Task<ActionResult<bool>> CheckEmailExists([FromQuery, Required] string email)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return BadRequest("Email is required");
            }

            var exists = await _userRepository.ExistsByEmailAsync(email);
            return Ok(new { exists, email });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking email existence: {Email}", email);
            return StatusCode(500, "An error occurred while checking email");
        }
    }

    /// <summary>
    /// Get user details by email (without password verification) - for public user lookup
    /// </summary>
    /// <param name="email">Email address</param>
    /// <returns>User details without sensitive information</returns>
    [HttpGet("user-by-email")]
    public async Task<ActionResult<AuthUserDto>> GetUserByEmail([FromQuery, Required] string email)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return BadRequest("Email is required");
            }

            var user = await _userRepository.GetByEmailAsync(email);
            
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            // Return limited user info (no sensitive data)
            var authUser = new AuthUserDto
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                CompanyId = user.CompanyId,
                CompanyName = user.CompanyName,
                RoleId = user.RoleId,
                RoleName = user.RoleName,
                Active = user.Active
            };

            return Ok(authUser);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by email: {Email}", email);
            return StatusCode(500, "An error occurred while retrieving user");
        }
    }

    /// <summary>
    /// Create test users for authentication testing - Development only
    /// </summary>
    [HttpPost("create-test-users")]
    public async Task<ActionResult> CreateTestUsers()
    {
        try
        {
            var testUsers = new List<CreateUserDto>
            {
                new CreateUserDto
                {
                    Email = "admin@depotdirect.com",
                    Password = "admin123",
                    FullName = "Admin User",
                    RoleId = 1, // Assuming role ID 1 exists
                    Active = true
                },
                new CreateUserDto
                {
                    Email = "user@depotdirect.com", 
                    Password = "user123",
                    FullName = "Regular User",
                    RoleId = 2, // Assuming role ID 2 exists
                    Active = true
                },
                new CreateUserDto
                {
                    Email = "test@example.com",
                    Password = "test123",
                    FullName = "Test User",
                    RoleId = 2,
                    Active = true
                }
            };

            var createdUsers = new List<object>();

            foreach (var createUserDto in testUsers)
            {
                // Check if user already exists
                if (await _userRepository.ExistsByEmailAsync(createUserDto.Email))
                {
                    createdUsers.Add(new { email = createUserDto.Email, status = "already exists" });
                    continue;
                }

                try
                {
                    var user = await _userRepository.CreateAsync(createUserDto);
                    createdUsers.Add(new { 
                        email = createUserDto.Email, 
                        status = "created", 
                        id = user.Id 
                    });
                }
                catch (Exception ex)
                {
                    createdUsers.Add(new { 
                        email = createUserDto.Email, 
                        status = "failed", 
                        error = ex.Message 
                    });
                }
            }

            return Ok(new { 
                message = "Test user creation completed", 
                users = createdUsers 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating test users");
            return StatusCode(500, "An error occurred while creating test users");
        }
    }
}