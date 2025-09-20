using DepotDirectApi.Models.DTOs;
using DepotDirectApi.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DepotDirectApi.Controllers.Admin;

[Authorize]
[ApiController]
[Route("api/admin/[controller]")]
public class UserRegionsController : BaseController
{
    private readonly IUserRegionRepository _userRegionRepository;
    private readonly IUserRepository _userRepository;
    private readonly IRegionRepository _regionRepository;
    private readonly ILogger<UserRegionsController> _logger;

    public UserRegionsController(
        IUserRegionRepository userRegionRepository,
        IUserRepository userRepository,
        IRegionRepository regionRepository,
        ILogger<UserRegionsController> logger)
    {
        _userRegionRepository = userRegionRepository;
        _userRepository = userRepository;
        _regionRepository = regionRepository;
        _logger = logger;
    }

    /// <summary>
    /// Get all user-region assignments
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserRegionDto>>> GetAll()
    {
        try
        {
            var userRegions = await _userRegionRepository.GetAllUserRegionsAsync();
            return Ok(userRegions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user-region assignments");
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    /// <summary>
    /// Get a specific user-region assignment
    /// </summary>
    [HttpGet("{userId}/{regionId}")]
    public async Task<ActionResult<UserRegionDto>> GetUserRegion(int userId, int regionId)
    {
        try
        {
            var userRegion = await _userRegionRepository.GetUserRegionAsync(userId, regionId);
            
            if (userRegion == null)
                return NotFound(new { message = "User-region assignment not found" });
                
            return Ok(userRegion);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user-region assignment for user {UserId} and region {RegionId}", userId, regionId);
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    /// <summary>
    /// Get all regions assigned to a user
    /// </summary>
    [HttpGet("user/{userId}/regions")]
    public async Task<ActionResult<UserWithRegionsDto>> GetUserRegions(int userId)
    {
        try
        {
            var userWithRegions = await _userRegionRepository.GetUserWithRegionsAsync(userId);
            
            if (userWithRegions == null)
                return NotFound(new { message = "User not found" });
                
            return Ok(userWithRegions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving regions for user {UserId}", userId);
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    /// <summary>
    /// Get all users assigned to a region
    /// </summary>
    [HttpGet("region/{regionId}/users")]
    public async Task<ActionResult<RegionWithUsersDto>> GetRegionUsers(int regionId)
    {
        try
        {
            var regionWithUsers = await _userRegionRepository.GetRegionWithUsersAsync(regionId);
            
            if (regionWithUsers == null)
                return NotFound(new { message = "Region not found" });
                
            return Ok(regionWithUsers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving users for region {RegionId}", regionId);
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    /// <summary>
    /// Assign a user to a region
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<UserRegionDto>> AssignUserToRegion([FromBody] UserRegionCreateDto createDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Validate user exists
            var userExists = await _userRepository.ExistsAsync(createDto.UserId);
            if (!userExists)
                return BadRequest(new { message = "User not found" });

            // Validate region exists
            var regionExists = await _regionRepository.ExistsAsync(createDto.RegionId);
            if (!regionExists)
                return BadRequest(new { message = "Region not found" });

            var currentUserId = GetCurrentUserId();
            var userRegion = await _userRegionRepository.AssignUserToRegionAsync(
                createDto.UserId, 
                createDto.RegionId, 
                currentUserId);

            var result = await _userRegionRepository.GetUserRegionAsync(createDto.UserId, createDto.RegionId);
            
            _logger.LogInformation("User {UserId} assigned to region {RegionId} by user {CurrentUserId}", 
                createDto.UserId, createDto.RegionId, currentUserId);

            return CreatedAtAction(nameof(GetUserRegion), 
                new { userId = createDto.UserId, regionId = createDto.RegionId }, result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning user {UserId} to region {RegionId}", createDto.UserId, createDto.RegionId);
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    /// <summary>
    /// Assign a user to a region (alternative endpoint for user-centric operations)
    /// </summary>
    [HttpPost("user/{userId}/regions")]
    public async Task<ActionResult<UserRegionDto>> AssignRegionToUser(int userId, [FromBody] AssignUserToRegionDto assignDto)
    {
        try
        {
            var createDto = new UserRegionCreateDto
            {
                UserId = userId,
                RegionId = assignDto.RegionId
            };

            return await AssignUserToRegion(createDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning region {RegionId} to user {UserId}", assignDto.RegionId, userId);
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    /// <summary>
    /// Assign a region to a user (alternative endpoint for region-centric operations)
    /// </summary>
    [HttpPost("region/{regionId}/users")]
    public async Task<ActionResult<UserRegionDto>> AssignUserToSpecificRegion(int regionId, [FromBody] AssignRegionToUserDto assignDto)
    {
        try
        {
            var createDto = new UserRegionCreateDto
            {
                UserId = assignDto.UserId,
                RegionId = regionId
            };

            return await AssignUserToRegion(createDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning user {UserId} to region {RegionId}", assignDto.UserId, regionId);
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    /// <summary>
    /// Update user-region assignment metadata
    /// </summary>
    [HttpPut("{userId}/{regionId}")]
    public async Task<ActionResult<UserRegionDto>> UpdateUserRegion(int userId, int regionId, [FromBody] UserRegionUpdateDto updateDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var currentUserId = GetCurrentUserId();
            var userRegion = await _userRegionRepository.UpdateUserRegionAsync(userId, regionId, updateDto, currentUserId);
            
            if (userRegion == null)
                return NotFound(new { message = "User-region assignment not found" });

            var result = await _userRegionRepository.GetUserRegionAsync(userId, regionId);
            
            _logger.LogInformation("User-region assignment updated for user {UserId} and region {RegionId} by user {CurrentUserId}", 
                userId, regionId, currentUserId);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user-region assignment for user {UserId} and region {RegionId}", userId, regionId);
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    /// <summary>
    /// Remove user from region
    /// </summary>
    [HttpDelete("{userId}/{regionId}")]
    public async Task<ActionResult> RemoveUserFromRegion(int userId, int regionId)
    {
        try
        {
            var success = await _userRegionRepository.RemoveUserFromRegionAsync(userId, regionId);
            
            if (!success)
                return NotFound(new { message = "User-region assignment not found" });

            var currentUserId = GetCurrentUserId();
            _logger.LogInformation("User {UserId} removed from region {RegionId} by user {CurrentUserId}", 
                userId, regionId, currentUserId);
                
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing user {UserId} from region {RegionId}", userId, regionId);
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    /// <summary>
    /// Remove user from region (alternative endpoint for user-centric operations)
    /// </summary>
    [HttpDelete("user/{userId}/regions/{regionId}")]
    public async Task<ActionResult> RemoveRegionFromUser(int userId, int regionId)
    {
        return await RemoveUserFromRegion(userId, regionId);
    }

    /// <summary>
    /// Remove user from region (alternative endpoint for region-centric operations)
    /// </summary>
    [HttpDelete("region/{regionId}/users/{userId}")]
    public async Task<ActionResult> RemoveUserFromSpecificRegion(int regionId, int userId)
    {
        return await RemoveUserFromRegion(userId, regionId);
    }

    /// <summary>
    /// Check if user is assigned to region
    /// </summary>
    [HttpGet("{userId}/{regionId}/exists")]
    public async Task<ActionResult<bool>> CheckUserRegionAssignment(int userId, int regionId)
    {
        try
        {
            var exists = await _userRegionRepository.IsUserAssignedToRegionAsync(userId, regionId);
            return Ok(new { exists });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking user-region assignment for user {UserId} and region {RegionId}", userId, regionId);
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    /// <summary>
    /// Validate if a user can be assigned to a region (same country validation)
    /// </summary>
    [HttpGet("{userId}/{regionId}/validate")]
    public async Task<ActionResult<bool>> ValidateUserRegionAssignment(int userId, int regionId)
    {
        try
        {
            var isValid = await _userRegionRepository.ValidateUserRegionAssignmentAsync(userId, regionId);
            return Ok(new { isValid });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating user-region assignment for user {UserId} and region {RegionId}", userId, regionId);
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }
}