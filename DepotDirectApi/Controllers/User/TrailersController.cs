using DepotDirectApi.Controllers;
using DepotDirectApi.Models.DTOs;
using DepotDirectApi.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DepotDirectApi.Controllers.User;

[Authorize]
[Route("api/user/trailers")]
[ApiController]
public class TrailersController : BaseController
{
    private readonly ITrailerRepository _trailerRepository;
    private readonly ILogger<TrailersController> _logger;

    public TrailersController(
        ITrailerRepository trailerRepository,
        ILogger<TrailersController> logger)
    {
        _trailerRepository = trailerRepository;
        _logger = logger;
    }

    /// <summary>
    /// Get all trailers
    /// </summary>
    /// <returns>List of trailers</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TrailerListItemDto>>> GetAll()
    {
        try
        {
            var trailers = await _trailerRepository.GetAllAsync();
            _logger.LogInformation("Retrieved {Count} trailers", trailers.Count());
            return Ok(trailers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving trailers");
            return StatusCode(500, "An error occurred while retrieving trailers");
        }
    }

    /// <summary>
    /// Get trailer by ID
    /// </summary>
    /// <param name="id">Trailer ID</param>
    /// <returns>Trailer details</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<TrailerResponseDto>> GetById(int id)
    {
        try
        {
            var trailer = await _trailerRepository.GetByIdAsync(id);
            if (trailer == null)
            {
                _logger.LogWarning("Trailer with ID {Id} not found", id);
                return NotFound($"Trailer with ID {id} not found");
            }

            return Ok(trailer);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving trailer with ID {Id}", id);
            return StatusCode(500, "An error occurred while retrieving the trailer");
        }
    }

    /// <summary>
    /// Create a new trailer
    /// </summary>
    /// <param name="createTrailerDto">Trailer creation data</param>
    /// <returns>Created trailer</returns>
    [HttpPost]
    public async Task<ActionResult<TrailerResponseDto>> Create([FromBody] CreateTrailerDto createTrailerDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetCurrentUserId();
            var trailer = await _trailerRepository.CreateAsync(createTrailerDto, userId);
            
            _logger.LogInformation("Created trailer with ID {Id} by user {UserId}", trailer.Id, userId);
            return CreatedAtAction(nameof(GetById), new { id = trailer.Id }, trailer);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument for trailer creation");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating trailer");
            return StatusCode(500, "An error occurred while creating the trailer");
        }
    }

    /// <summary>
    /// Update a trailer
    /// </summary>
    /// <param name="id">Trailer ID</param>
    /// <param name="updateTrailerDto">Trailer update data</param>
    /// <returns>Updated trailer</returns>
    [HttpPut("{id}")]
    public async Task<ActionResult<TrailerResponseDto>> Update(int id, [FromBody] UpdateTrailerDto updateTrailerDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetCurrentUserId();
            var trailer = await _trailerRepository.UpdateAsync(id, updateTrailerDto, userId);

            if (trailer == null)
            {
                _logger.LogWarning("Trailer with ID {Id} not found for update", id);
                return NotFound($"Trailer with ID {id} not found");
            }

            _logger.LogInformation("Updated trailer with ID {Id} by user {UserId}", id, userId);
            return Ok(trailer);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument for trailer update");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating trailer with ID {Id}", id);
            return StatusCode(500, "An error occurred while updating the trailer");
        }
    }

    /// <summary>
    /// Delete a trailer
    /// </summary>
    /// <param name="id">Trailer ID</param>
    /// <returns>Success or error</returns>
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        try
        {
            var deleted = await _trailerRepository.DeleteAsync(id);
            if (!deleted)
            {
                _logger.LogWarning("Trailer with ID {Id} not found for deletion", id);
                return NotFound($"Trailer with ID {id} not found");
            }

            var userId = GetCurrentUserId();
            _logger.LogInformation("Deleted trailer with ID {Id} by user {UserId}", id, userId);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Cannot delete trailer with ID {Id}", id);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting trailer with ID {Id}", id);
            return StatusCode(500, "An error occurred while deleting the trailer");
        }
    }

    /// <summary>
    /// Check if trailer exists
    /// </summary>
    /// <param name="id">Trailer ID</param>
    /// <returns>True if exists, false otherwise</returns>
    [HttpGet("{id}/exists")]
    public async Task<ActionResult<bool>> Exists(int id)
    {
        try
        {
            var exists = await _trailerRepository.ExistsAsync(id);
            return Ok(exists);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking existence of trailer with ID {Id}", id);
            return StatusCode(500, "An error occurred while checking trailer existence");
        }
    }

    /// <summary>
    /// Get trailers for a specific haulier
    /// </summary>
    /// <param name="haulierId">Haulier ID</param>
    /// <returns>List of trailers for the haulier</returns>
    [HttpGet("by-haulier/{haulierId}")]
    public async Task<ActionResult<IEnumerable<TrailerListItemDto>>> GetByHaulierId(int haulierId)
    {
        try
        {
            var trailers = await _trailerRepository.GetByHaulierIdAsync(haulierId);
            _logger.LogInformation("Retrieved {Count} trailers for haulier {HaulierId}", trailers.Count(), haulierId);
            return Ok(trailers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving trailers for haulier {HaulierId}", haulierId);
            return StatusCode(500, "An error occurred while retrieving haulier trailers");
        }
    }

    /// <summary>
    /// Get trailers for a specific region
    /// </summary>
    /// <param name="regionId">Region ID</param>
    /// <returns>List of trailers for the region</returns>
    [HttpGet("by-region/{regionId}")]
    public async Task<ActionResult<IEnumerable<TrailerListItemDto>>> GetByRegionId(int regionId)
    {
        try
        {
            var trailers = await _trailerRepository.GetByRegionIdAsync(regionId);
            _logger.LogInformation("Retrieved {Count} trailers for region {RegionId}", trailers.Count(), regionId);
            return Ok(trailers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving trailers for region {RegionId}", regionId);
            return StatusCode(500, "An error occurred while retrieving region trailers");
        }
    }

    /// <summary>
    /// Get trailers by status
    /// </summary>
    /// <param name="status">Status (Active, Maintenance, Inactive)</param>
    /// <returns>List of trailers with the specified status</returns>
    [HttpGet("by-status/{status}")]
    public async Task<ActionResult<IEnumerable<TrailerListItemDto>>> GetByStatus(string status)
    {
        try
        {
            var trailers = await _trailerRepository.GetByStatusAsync(status);
            _logger.LogInformation("Retrieved {Count} trailers with status '{Status}'", trailers.Count(), status);
            return Ok(trailers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving trailers with status '{Status}'", status);
            return StatusCode(500, "An error occurred while retrieving trailers by status");
        }
    }

    /// <summary>
    /// Search trailers
    /// </summary>
    /// <param name="query">Search term</param>
    /// <returns>List of matching trailers</returns>
    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<TrailerListItemDto>>> Search([FromQuery] string query)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest("Search query cannot be empty");
            }

            var trailers = await _trailerRepository.SearchAsync(query);
            _logger.LogInformation("Found {Count} trailers matching search query '{Query}'", trailers.Count(), query);
            return Ok(trailers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching trailers with query '{Query}'", query);
            return StatusCode(500, "An error occurred while searching trailers");
        }
    }

    /// <summary>
    /// Get available trailers for a date range
    /// </summary>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <returns>List of available trailers</returns>
    [HttpGet("available")]
    public async Task<ActionResult<IEnumerable<TrailerListItemDto>>> GetAvailable([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        try
        {
            if (startDate >= endDate)
            {
                return BadRequest("Start date must be before end date");
            }

            var trailers = await _trailerRepository.GetAvailableTrailersAsync(startDate, endDate);
            _logger.LogInformation("Retrieved {Count} available trailers for period {StartDate} to {EndDate}", 
                trailers.Count(), startDate, endDate);
            return Ok(trailers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving available trailers for period {StartDate} to {EndDate}", startDate, endDate);
            return StatusCode(500, "An error occurred while retrieving available trailers");
        }
    }

    /// <summary>
    /// Check if trailer is available for a date range
    /// </summary>
    /// <param name="id">Trailer ID</param>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <returns>True if available, false otherwise</returns>
    [HttpGet("{id}/available")]
    public async Task<ActionResult<bool>> IsTrailerAvailable(int id, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        try
        {
            if (startDate >= endDate)
            {
                return BadRequest("Start date must be before end date");
            }

            var isAvailable = await _trailerRepository.IsTrailerAvailableAsync(id, startDate, endDate);
            return Ok(isAvailable);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking availability of trailer {Id} for period {StartDate} to {EndDate}", id, startDate, endDate);
            return StatusCode(500, "An error occurred while checking trailer availability");
        }
    }
}