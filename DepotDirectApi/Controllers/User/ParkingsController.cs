using DepotDirectApi.Controllers;
using DepotDirectApi.Models.DTOs;
using DepotDirectApi.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DepotDirectApi.Controllers.User;

[Authorize]
[Route("api/user/parkings")]
[ApiController]
public class ParkingsController : BaseController
{
    private readonly IParkingRepository _parkingRepository;
    private readonly ILogger<ParkingsController> _logger;

    public ParkingsController(
        IParkingRepository parkingRepository,
        ILogger<ParkingsController> logger)
    {
        _parkingRepository = parkingRepository;
        _logger = logger;
    }

    /// <summary>
    /// Get all parkings
    /// </summary>
    /// <returns>List of parkings</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ParkingListItemDto>>> GetAll()
    {
        try
        {
            var parkings = await _parkingRepository.GetAllAsync();
            _logger.LogInformation("Retrieved {Count} parkings", parkings.Count());
            return Ok(parkings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving parkings");
            return StatusCode(500, "An error occurred while retrieving parkings");
        }
    }

    /// <summary>
    /// Get parking by ID
    /// </summary>
    /// <param name="id">Parking ID</param>
    /// <returns>Parking details</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<ParkingResponseDto>> GetById(int id)
    {
        try
        {
            var parking = await _parkingRepository.GetByIdAsync(id);
            if (parking == null)
            {
                _logger.LogWarning("Parking with ID {Id} not found", id);
                return NotFound($"Parking with ID {id} not found");
            }

            return Ok(parking);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving parking with ID {Id}", id);
            return StatusCode(500, "An error occurred while retrieving the parking");
        }
    }

    /// <summary>
    /// Create a new parking
    /// </summary>
    /// <param name="createParkingDto">Parking creation data</param>
    /// <returns>Created parking</returns>
    [HttpPost]
    public async Task<ActionResult<ParkingResponseDto>> Create([FromBody] CreateParkingDto createParkingDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetCurrentUserId();
            var parking = await _parkingRepository.CreateAsync(createParkingDto, userId);
            
            _logger.LogInformation("Created parking with ID {Id} by user {UserId}", parking.Id, userId);
            return CreatedAtAction(nameof(GetById), new { id = parking.Id }, parking);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument for parking creation");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating parking");
            return StatusCode(500, "An error occurred while creating the parking");
        }
    }

    /// <summary>
    /// Update a parking
    /// </summary>
    /// <param name="id">Parking ID</param>
    /// <param name="updateParkingDto">Parking update data</param>
    /// <returns>Updated parking</returns>
    [HttpPut("{id}")]
    public async Task<ActionResult<ParkingResponseDto>> Update(int id, [FromBody] UpdateParkingDto updateParkingDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetCurrentUserId();
            var parking = await _parkingRepository.UpdateAsync(id, updateParkingDto, userId);

            if (parking == null)
            {
                _logger.LogWarning("Parking with ID {Id} not found for update", id);
                return NotFound($"Parking with ID {id} not found");
            }

            _logger.LogInformation("Updated parking with ID {Id} by user {UserId}", id, userId);
            return Ok(parking);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument for parking update");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating parking with ID {Id}", id);
            return StatusCode(500, "An error occurred while updating the parking");
        }
    }

    /// <summary>
    /// Delete a parking
    /// </summary>
    /// <param name="id">Parking ID</param>
    /// <returns>Success or error</returns>
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        try
        {
            var deleted = await _parkingRepository.DeleteAsync(id);
            if (!deleted)
            {
                _logger.LogWarning("Parking with ID {Id} not found for deletion", id);
                return NotFound($"Parking with ID {id} not found");
            }

            var userId = GetCurrentUserId();
            _logger.LogInformation("Deleted parking with ID {Id} by user {UserId}", id, userId);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting parking with ID {Id}", id);
            return StatusCode(500, "An error occurred while deleting the parking");
        }
    }

    /// <summary>
    /// Check if parking exists
    /// </summary>
    /// <param name="id">Parking ID</param>
    /// <returns>True if exists, false otherwise</returns>
    [HttpGet("{id}/exists")]
    public async Task<ActionResult<bool>> Exists(int id)
    {
        try
        {
            var exists = await _parkingRepository.ExistsAsync(id);
            return Ok(exists);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking existence of parking with ID {Id}", id);
            return StatusCode(500, "An error occurred while checking parking existence");
        }
    }

    /// <summary>
    /// Get parkings for a specific company
    /// </summary>
    /// <param name="companyId">Company ID</param>
    /// <returns>List of parkings for the company</returns>
    [HttpGet("by-company/{companyId}")]
    public async Task<ActionResult<IEnumerable<ParkingListItemDto>>> GetByCompanyId(int companyId)
    {
        try
        {
            var parkings = await _parkingRepository.GetByCompanyIdAsync(companyId);
            _logger.LogInformation("Retrieved {Count} parkings for company {CompanyId}", parkings.Count(), companyId);
            return Ok(parkings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving parkings for company {CompanyId}", companyId);
            return StatusCode(500, "An error occurred while retrieving company parkings");
        }
    }

    /// <summary>
    /// Get parkings for a specific country
    /// </summary>
    /// <param name="countryId">Country ID</param>
    /// <returns>List of parkings in the country</returns>
    [HttpGet("by-country/{countryId}")]
    public async Task<ActionResult<IEnumerable<ParkingListItemDto>>> GetByCountryId(int countryId)
    {
        try
        {
            var parkings = await _parkingRepository.GetByCountryIdAsync(countryId);
            _logger.LogInformation("Retrieved {Count} parkings for country {CountryId}", parkings.Count(), countryId);
            return Ok(parkings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving parkings for country {CountryId}", countryId);
            return StatusCode(500, "An error occurred while retrieving country parkings");
        }
    }

    /// <summary>
    /// Get parkings for a specific region
    /// </summary>
    /// <param name="regionId">Region ID</param>
    /// <returns>List of parkings in the region</returns>
    [HttpGet("by-region/{regionId}")]
    public async Task<ActionResult<IEnumerable<ParkingListItemDto>>> GetByRegionId(int regionId)
    {
        try
        {
            var parkings = await _parkingRepository.GetByRegionIdAsync(regionId);
            _logger.LogInformation("Retrieved {Count} parkings for region {RegionId}", parkings.Count(), regionId);
            return Ok(parkings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving parkings for region {RegionId}", regionId);
            return StatusCode(500, "An error occurred while retrieving region parkings");
        }
    }

    /// <summary>
    /// Search parkings
    /// </summary>
    /// <param name="query">Search term</param>
    /// <returns>List of matching parkings</returns>
    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<ParkingListItemDto>>> Search([FromQuery] string query)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest("Search query cannot be empty");
            }

            var parkings = await _parkingRepository.SearchAsync(query);
            _logger.LogInformation("Found {Count} parkings matching search query '{Query}'", parkings.Count(), query);
            return Ok(parkings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching parkings with query '{Query}'", query);
            return StatusCode(500, "An error occurred while searching parkings");
        }
    }

    /// <summary>
    /// Assign parking to region
    /// </summary>
    /// <param name="parkingId">Parking ID</param>
    /// <param name="assignDto">Assignment data</param>
    /// <returns>Created assignment</returns>
    [HttpPost("{parkingId}/regions")]
    public async Task<ActionResult<RegionParkingDto>> AssignToRegion(int parkingId, [FromBody] AssignParkingToRegionDto assignDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetCurrentUserId();
            var assignment = await _parkingRepository.AssignParkingToRegionAsync(parkingId, assignDto.RegionId, assignDto.ParkingCode, userId);
            
            _logger.LogInformation("Assigned parking {ParkingId} to region {RegionId} by user {UserId}", parkingId, assignDto.RegionId, userId);
            return Ok(assignment);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument for parking assignment");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning parking {ParkingId} to region {RegionId}", parkingId, assignDto.RegionId);
            return StatusCode(500, "An error occurred while assigning parking to region");
        }
    }

    /// <summary>
    /// Remove parking from region
    /// </summary>
    /// <param name="parkingId">Parking ID</param>
    /// <param name="regionId">Region ID</param>
    /// <returns>Success or error</returns>
    [HttpDelete("{parkingId}/regions/{regionId}")]
    public async Task<ActionResult> RemoveFromRegion(int parkingId, int regionId)
    {
        try
        {
            var removed = await _parkingRepository.RemoveParkingFromRegionAsync(parkingId, regionId);
            if (!removed)
            {
                _logger.LogWarning("Parking {ParkingId} not assigned to region {RegionId}", parkingId, regionId);
                return NotFound($"Parking {parkingId} is not assigned to region {regionId}");
            }

            var userId = GetCurrentUserId();
            _logger.LogInformation("Removed parking {ParkingId} from region {RegionId} by user {UserId}", parkingId, regionId, userId);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing parking {ParkingId} from region {RegionId}", parkingId, regionId);
            return StatusCode(500, "An error occurred while removing parking from region");
        }
    }

    /// <summary>
    /// Check if parking is assigned to region
    /// </summary>
    /// <param name="parkingId">Parking ID</param>
    /// <param name="regionId">Region ID</param>
    /// <returns>True if assigned, false otherwise</returns>
    [HttpGet("{parkingId}/regions/{regionId}/exists")]
    public async Task<ActionResult<bool>> IsParkingAssignedToRegion(int parkingId, int regionId)
    {
        try
        {
            var assigned = await _parkingRepository.IsParkingAssignedToRegionAsync(parkingId, regionId);
            return Ok(assigned);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking parking {ParkingId} assignment to region {RegionId}", parkingId, regionId);
            return StatusCode(500, "An error occurred while checking parking assignment");
        }
    }
}