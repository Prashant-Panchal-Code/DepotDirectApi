using DepotDirectApi.Data;
using DepotDirectApi.Models.DTOs;
using DepotDirectApi.Models.Entities;
using DepotDirectApi.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
    private readonly DepotDirectDbContext _context;

    public UserRegionsController(
        IUserRegionRepository userRegionRepository,
        IUserRepository userRepository,
        IRegionRepository regionRepository,
        ILogger<UserRegionsController> logger,
        DepotDirectDbContext context)
    {
        _userRegionRepository = userRegionRepository;
        _userRepository = userRepository;
        _regionRepository = regionRepository;
        _logger = logger;
        _context = context;
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

            _logger.LogInformation("Attempting to assign user {UserId} to region {RegionId}", createDto.UserId, createDto.RegionId);

            // Validate user exists
            var userExists = await _userRepository.ExistsAsync(createDto.UserId);
            if (!userExists)
            {
                _logger.LogWarning("User {UserId} not found", createDto.UserId);
                return BadRequest(new { message = $"User with ID {createDto.UserId} not found" });
            }

            // Validate region exists
            var regionExists = await _regionRepository.ExistsAsync(createDto.RegionId);
            if (!regionExists)
            {
                _logger.LogWarning("Region {RegionId} not found", createDto.RegionId);
                return BadRequest(new { message = $"Region with ID {createDto.RegionId} not found" });
            }

            // Validate assignment is allowed (same country)
            var isValidAssignment = await _userRegionRepository.ValidateUserRegionAssignmentAsync(createDto.UserId, createDto.RegionId);
            if (!isValidAssignment)
            {
                _logger.LogWarning("User {UserId} cannot be assigned to region {RegionId} - different countries", createDto.UserId, createDto.RegionId);
                return BadRequest(new { message = "User cannot be assigned to a region in a different country than their company" });
            }

            var currentUserId = GetCurrentUserId();
            _logger.LogInformation("Creating user-region assignment with createdBy: {CurrentUserId}", currentUserId);

            var userRegion = await _userRegionRepository.AssignUserToRegionAsync(
                createDto.UserId, 
                createDto.RegionId, 
                currentUserId);

            var result = await _userRegionRepository.GetUserRegionAsync(createDto.UserId, createDto.RegionId);
            
            _logger.LogInformation("Successfully assigned user {UserId} to region {RegionId} by user {CurrentUserId}", 
                createDto.UserId, createDto.RegionId, currentUserId);

            return CreatedAtAction(nameof(GetUserRegion), 
                new { userId = createDto.UserId, regionId = createDto.RegionId }, result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation error when assigning user {UserId} to region {RegionId}: {Message}", 
                createDto.UserId, createDto.RegionId, ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Database operation failed when assigning user {UserId} to region {RegionId}: {Message}", 
                createDto.UserId, createDto.RegionId, ex.Message);
            return StatusCode(500, new { message = "Database operation failed", details = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error assigning user {UserId} to region {RegionId}", createDto.UserId, createDto.RegionId);
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

    /// <summary>
    /// Debug endpoint to check user and region details before assignment
    /// </summary>
    [HttpGet("{userId}/{regionId}/debug")]
    public async Task<ActionResult> DebugUserRegionAssignment(int userId, int regionId)
    {
        try
        {
            // Check if user exists
            var userExists = await _userRepository.ExistsAsync(userId);
            var user = userExists ? await _userRepository.GetByIdAsync(userId) : null;

            // Check if region exists
            var regionExists = await _regionRepository.ExistsAsync(regionId);
            var region = regionExists ? await _regionRepository.GetByIdAsync(regionId) : null;

            // Check if already assigned
            var alreadyAssigned = await _userRegionRepository.IsUserAssignedToRegionAsync(userId, regionId);

            // Check validation
            var isValidAssignment = await _userRegionRepository.ValidateUserRegionAssignmentAsync(userId, regionId);

            var debugInfo = new
            {
                UserId = userId,
                RegionId = regionId,
                UserExists = userExists,
                RegionExists = regionExists,
                AlreadyAssigned = alreadyAssigned,
                IsValidAssignment = isValidAssignment,
                User = user != null ? new
                {
                    user.Id,
                    user.FullName,
                    user.Email,
                    user.Active,
                    user.CompanyId,
                    user.CompanyName
                } : null,
                Region = region != null ? new
                {
                    region.Id,
                    region.Name,
                    region.CompanyId
                } : null
            };

            return Ok(debugInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error debugging user-region assignment for user {UserId} and region {RegionId}", userId, regionId);
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    /// <summary>
    /// Simple test assignment - minimal validation for debugging
    /// </summary>
    [HttpPost("{userId}/{regionId}/test-assign")]
    public async Task<ActionResult> TestAssignUserToRegion(int userId, int regionId)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            
            _logger.LogInformation("Test assignment: User {UserId} to Region {RegionId} by {CurrentUserId}", 
                userId, regionId, currentUserId);

            // Very basic check - just see if they exist in database
            var userCount = await _context.Users.CountAsync(u => u.Id == userId);
            var regionCount = await _context.Regions.CountAsync(r => r.Id == regionId);
            
            if (userCount == 0)
            {
                return BadRequest(new { message = $"User {userId} not found in database" });
            }
            
            if (regionCount == 0)
            {
                return BadRequest(new { message = $"Region {regionId} not found in database" });
            }

            // Try direct assignment with minimal entity
            var userRegion = new UserRegion
            {
                UserId = userId,
                RegionId = regionId,
                CreatedBy = currentUserId
            };

            _context.UserRegions.Add(userRegion);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Test assignment successful: User {UserId} assigned to Region {RegionId}", userId, regionId);
            
            return Ok(new { 
                message = "Assignment successful",
                userRegionId = userRegion.Id,
                userId = userRegion.UserId,
                regionId = userRegion.RegionId,
                createdBy = userRegion.CreatedBy
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Test assignment failed for user {UserId} and region {RegionId}", userId, regionId);
            return StatusCode(500, new { 
                message = "Test assignment failed", 
                details = ex.Message,
                innerException = ex.InnerException?.Message
            });
        }
    }

    /// <summary>
    /// Apply database fix for user-regions trigger - Admin only
    /// </summary>
    [HttpPost("fix-database-trigger")]
    public async Task<ActionResult> FixDatabaseTrigger()
    {
        try
        {
            _logger.LogInformation("Applying database trigger fix for user-regions");

            var fixSql = @"
                -- Drop the existing trigger and function first
                DROP TRIGGER IF EXISTS trg_validate_user_region_matches_company_country ON depotdirect.user_regions;
                DROP FUNCTION IF EXISTS depotdirect.fn_validate_user_region_matches_company_country();

                -- Create corrected trigger function that follows the proper relationship
                CREATE OR REPLACE FUNCTION depotdirect.fn_validate_user_region_matches_company_country()
                RETURNS trigger LANGUAGE plpgsql AS $$
                DECLARE
                  reg_country integer;
                  comp_country integer;
                  comp_id integer;
                BEGIN
                  IF TG_OP NOT IN ('INSERT','UPDATE') THEN
                    RETURN NEW;
                  END IF;

                  -- Get region's country through company relationship: region -> company -> country
                  SELECT c.country_id INTO reg_country 
                  FROM depotdirect.regions r 
                  JOIN depotdirect.companies c ON r.company_id = c.id 
                  WHERE r.id = NEW.region_id;
                  
                  IF reg_country IS NULL THEN
                    RAISE EXCEPTION 'region id % does not exist or its company has no country_id', NEW.region_id;
                  END IF;

                  -- get user's company_id
                  SELECT company_id INTO comp_id FROM depotdirect.users WHERE id = NEW.user_id;
                  IF comp_id IS NULL THEN
                    RAISE EXCEPTION 'user id % has no company_id; assign a company before adding regions', NEW.user_id;
                  END IF;

                  -- get company's country_id
                  SELECT country_id INTO comp_country FROM depotdirect.companies WHERE id = comp_id;
                  IF comp_country IS NULL THEN
                    RAISE EXCEPTION 'company id % does not exist or has no country_id', comp_id;
                  END IF;

                  IF reg_country <> comp_country THEN
                    RAISE EXCEPTION 'cannot assign region (id=%, country=%) to user (id=%) who belongs to company (id=%, country=%)', NEW.region_id, reg_country, NEW.user_id, comp_id, comp_country;
                  END IF;

                  RETURN NEW;
                END;
                $$;

                -- Recreate the trigger
                CREATE TRIGGER trg_validate_user_region_matches_company_country
                  BEFORE INSERT OR UPDATE ON depotdirect.user_regions
                  FOR EACH ROW EXECUTE FUNCTION depotdirect.fn_validate_user_region_matches_company_country();

                -- Also ensure the sequence permissions are correct
                GRANT USAGE, SELECT ON SEQUENCE depotdirect.user_regions_id_seq TO depotdirect_user;
                GRANT INSERT, SELECT, UPDATE, DELETE ON TABLE depotdirect.user_regions TO depotdirect_user;";

            await _context.Database.ExecuteSqlRawAsync(fixSql);

            _logger.LogInformation("Database trigger fix applied successfully");
            
            return Ok(new { 
                message = "Database trigger fix applied successfully",
                details = "The user-regions validation trigger has been corrected to use the proper relationship: Region -> Company -> Country"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to apply database trigger fix");
            return StatusCode(500, new { 
                message = "Failed to apply database trigger fix", 
                details = ex.Message,
                innerException = ex.InnerException?.Message
            });
        }
    }
}