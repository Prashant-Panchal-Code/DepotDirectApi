using DepotDirectApi.Controllers;
using DepotDirectApi.Models.DTOs;
using DepotDirectApi.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DepotDirectApi.Controllers.User;

[Authorize]
[Route("api/user/tractors")]
[ApiController]
public class TractorsController : BaseController
{
    private readonly ITractorRepository _tractorRepository;
    private readonly ILogger<TractorsController> _logger;

    public TractorsController(
        ITractorRepository tractorRepository,
        ILogger<TractorsController> logger)
    {
        _tractorRepository = tractorRepository;
        _logger = logger;
    }

    /// <summary>
    /// Get all tractors
    /// </summary>
    /// <returns>List of tractors</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TractorListItemDto>>> GetAll()
    {
        try
        {
            var tractors = await _tractorRepository.GetAllAsync();
            _logger.LogInformation("Retrieved {Count} tractors", tractors.Count());
            return Ok(tractors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tractors");
            return StatusCode(500, "An error occurred while retrieving tractors");
        }
    }

    /// <summary>
    /// Get tractor by ID
    /// </summary>
    /// <param name="id">Tractor ID</param>
    /// <returns>Tractor details</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<TractorResponseDto>> GetById(int id)
    {
        try
        {
            var tractor = await _tractorRepository.GetByIdAsync(id);
            if (tractor == null)
            {
                _logger.LogWarning("Tractor with ID {Id} not found", id);
                return NotFound($"Tractor with ID {id} not found");
            }

            return Ok(tractor);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tractor with ID {Id}", id);
            return StatusCode(500, "An error occurred while retrieving the tractor");
        }
    }

    /// <summary>
    /// Create a new tractor
    /// </summary>
    /// <param name="createTractorDto">Tractor creation data</param>
    /// <returns>Created tractor</returns>
    [HttpPost]
    public async Task<ActionResult<TractorResponseDto>> Create([FromBody] CreateTractorDto createTractorDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetCurrentUserId();
            var tractor = await _tractorRepository.CreateAsync(createTractorDto, userId);
            
            _logger.LogInformation("Created tractor with ID {Id} by user {UserId}", tractor.Id, userId);
            return CreatedAtAction(nameof(GetById), new { id = tractor.Id }, tractor);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument for tractor creation");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating tractor");
            return StatusCode(500, "An error occurred while creating the tractor");
        }
    }

    /// <summary>
    /// Update a tractor
    /// </summary>
    /// <param name="id">Tractor ID</param>
    /// <param name="updateTractorDto">Tractor update data</param>
    /// <returns>Updated tractor</returns>
    [HttpPut("{id}")]
    public async Task<ActionResult<TractorResponseDto>> Update(int id, [FromBody] UpdateTractorDto updateTractorDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetCurrentUserId();
            var tractor = await _tractorRepository.UpdateAsync(id, updateTractorDto, userId);

            if (tractor == null)
            {
                _logger.LogWarning("Tractor with ID {Id} not found for update", id);
                return NotFound($"Tractor with ID {id} not found");
            }

            _logger.LogInformation("Updated tractor with ID {Id} by user {UserId}", id, userId);
            return Ok(tractor);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument for tractor update");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating tractor with ID {Id}", id);
            return StatusCode(500, "An error occurred while updating the tractor");
        }
    }

    /// <summary>
    /// Delete a tractor
    /// </summary>
    /// <param name="id">Tractor ID</param>
    /// <returns>Success or error</returns>
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        try
        {
            var deleted = await _tractorRepository.DeleteAsync(id);
            if (!deleted)
            {
                _logger.LogWarning("Tractor with ID {Id} not found for deletion", id);
                return NotFound($"Tractor with ID {id} not found");
            }

            var userId = GetCurrentUserId();
            _logger.LogInformation("Deleted tractor with ID {Id} by user {UserId}", id, userId);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Cannot delete tractor with ID {Id}", id);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting tractor with ID {Id}", id);
            return StatusCode(500, "An error occurred while deleting the tractor");
        }
    }

    /// <summary>
    /// Check if tractor exists
    /// </summary>
    /// <param name="id">Tractor ID</param>
    /// <returns>True if exists, false otherwise</returns>
    [HttpGet("{id}/exists")]
    public async Task<ActionResult<bool>> Exists(int id)
    {
        try
        {
            var exists = await _tractorRepository.ExistsAsync(id);
            return Ok(exists);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking existence of tractor with ID {Id}", id);
            return StatusCode(500, "An error occurred while checking tractor existence");
        }
    }

    /// <summary>
    /// Get tractors for a specific haulier
    /// </summary>
    /// <param name="haulierId">Haulier ID</param>
    /// <returns>List of tractors for the haulier</returns>
    [HttpGet("by-haulier/{haulierId}")]
    public async Task<ActionResult<IEnumerable<TractorListItemDto>>> GetByHaulierId(int haulierId)
    {
        try
        {
            var tractors = await _tractorRepository.GetByHaulierIdAsync(haulierId);
            _logger.LogInformation("Retrieved {Count} tractors for haulier {HaulierId}", tractors.Count(), haulierId);
            return Ok(tractors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tractors for haulier {HaulierId}", haulierId);
            return StatusCode(500, "An error occurred while retrieving haulier tractors");
        }
    }

    /// <summary>
    /// Get tractors for a specific region
    /// </summary>
    /// <param name="regionId">Region ID</param>
    /// <returns>List of tractors for the region</returns>
    [HttpGet("by-region/{regionId}")]
    public async Task<ActionResult<IEnumerable<TractorListItemDto>>> GetByRegionId(int regionId)
    {
        try
        {
            var tractors = await _tractorRepository.GetByRegionIdAsync(regionId);
            _logger.LogInformation("Retrieved {Count} tractors for region {RegionId}", tractors.Count(), regionId);
            return Ok(tractors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tractors for region {RegionId}", regionId);
            return StatusCode(500, "An error occurred while retrieving region tractors");
        }
    }

    /// <summary>
    /// Get tractors by status
    /// </summary>
    /// <param name="status">Status (Active, Maintenance, Inactive)</param>
    /// <returns>List of tractors with the specified status</returns>
    [HttpGet("by-status/{status}")]
    public async Task<ActionResult<IEnumerable<TractorListItemDto>>> GetByStatus(string status)
    {
        try
        {
            var tractors = await _tractorRepository.GetByStatusAsync(status);
            _logger.LogInformation("Retrieved {Count} tractors with status '{Status}'", tractors.Count(), status);
            return Ok(tractors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tractors with status '{Status}'", status);
            return StatusCode(500, "An error occurred while retrieving tractors by status");
        }
    }

    /// <summary>
    /// Get tractors with pump capability
    /// </summary>
    /// <returns>List of tractors with pump capability</returns>
    [HttpGet("with-pump")]
    public async Task<ActionResult<IEnumerable<TractorListItemDto>>> GetWithPump()
    {
        try
        {
            var tractors = await _tractorRepository.GetWithPumpAsync();
            _logger.LogInformation("Retrieved {Count} tractors with pump capability", tractors.Count());
            return Ok(tractors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tractors with pump capability");
            return StatusCode(500, "An error occurred while retrieving tractors with pump");
        }
    }

    /// <summary>
    /// Search tractors
    /// </summary>
    /// <param name="query">Search term</param>
    /// <returns>List of matching tractors</returns>
    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<TractorListItemDto>>> Search([FromQuery] string query)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest("Search query cannot be empty");
            }

            var tractors = await _tractorRepository.SearchAsync(query);
            _logger.LogInformation("Found {Count} tractors matching search query '{Query}'", tractors.Count(), query);
            return Ok(tractors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching tractors with query '{Query}'", query);
            return StatusCode(500, "An error occurred while searching tractors");
        }
    }

    /// <summary>
    /// Get available tractors for a date range
    /// </summary>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <returns>List of available tractors</returns>
    [HttpGet("available")]
    public async Task<ActionResult<IEnumerable<TractorListItemDto>>> GetAvailable([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        try
        {
            if (startDate >= endDate)
            {
                return BadRequest("Start date must be before end date");
            }

            var tractors = await _tractorRepository.GetAvailableTractorsAsync(startDate, endDate);
            _logger.LogInformation("Retrieved {Count} available tractors for period {StartDate} to {EndDate}", 
                tractors.Count(), startDate, endDate);
            return Ok(tractors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving available tractors for period {StartDate} to {EndDate}", startDate, endDate);
            return StatusCode(500, "An error occurred while retrieving available tractors");
        }
    }

    /// <summary>
    /// Check if tractor is available for a date range
    /// </summary>
    /// <param name="id">Tractor ID</param>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <returns>True if available, false otherwise</returns>
    [HttpGet("{id}/available")]
    public async Task<ActionResult<bool>> IsTractorAvailable(int id, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        try
        {
            if (startDate >= endDate)
            {
                return BadRequest("Start date must be before end date");
            }

            var isAvailable = await _tractorRepository.IsTractorAvailableAsync(id, startDate, endDate);
            return Ok(isAvailable);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking availability of tractor {Id} for period {StartDate} to {EndDate}", id, startDate, endDate);
            return StatusCode(500, "An error occurred while checking tractor availability");
        }
    }
}