using DepotDirectApi.Controllers;
using DepotDirectApi.Models.DTOs;
using DepotDirectApi.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DepotDirectApi.Controllers.User;

[Authorize]
[Route("api/user/vehicle-combinations")]
[ApiController]
public class VehicleCombinationsController : BaseController
{
    private readonly IVehicleCombinationRepository _vehicleCombinationRepository;
    private readonly ILogger<VehicleCombinationsController> _logger;

    public VehicleCombinationsController(
        IVehicleCombinationRepository vehicleCombinationRepository,
        ILogger<VehicleCombinationsController> logger)
    {
        _vehicleCombinationRepository = vehicleCombinationRepository;
        _logger = logger;
    }

    /// <summary>
    /// Get all vehicle combinations
    /// </summary>
    /// <returns>List of vehicle combinations</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<VehicleCombinationListItemDto>>> GetAll()
    {
        try
        {
            var combinations = await _vehicleCombinationRepository.GetAllAsync();
            _logger.LogInformation("Retrieved {Count} vehicle combinations", combinations.Count());
            return Ok(combinations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving vehicle combinations");
            return StatusCode(500, "An error occurred while retrieving vehicle combinations");
        }
    }

    /// <summary>
    /// Get vehicle combination by ID
    /// </summary>
    /// <param name="id">Vehicle combination ID</param>
    /// <returns>Vehicle combination details</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<VehicleCombinationResponseDto>> GetById(int id)
    {
        try
        {
            var combination = await _vehicleCombinationRepository.GetByIdAsync(id);
            if (combination == null)
            {
                _logger.LogWarning("Vehicle combination with ID {Id} not found", id);
                return NotFound($"Vehicle combination with ID {id} not found");
            }

            return Ok(combination);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving vehicle combination with ID {Id}", id);
            return StatusCode(500, "An error occurred while retrieving the vehicle combination");
        }
    }

    /// <summary>
    /// Create a new vehicle combination
    /// </summary>
    /// <param name="createVehicleCombinationDto">Vehicle combination creation data</param>
    /// <returns>Created vehicle combination</returns>
    [HttpPost]
    public async Task<ActionResult<VehicleCombinationResponseDto>> Create([FromBody] CreateVehicleCombinationDto createVehicleCombinationDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetCurrentUserId();
            var combination = await _vehicleCombinationRepository.CreateAsync(createVehicleCombinationDto, userId);
            
            _logger.LogInformation("Created vehicle combination with ID {Id} by user {UserId}", combination.Id, userId);
            return CreatedAtAction(nameof(GetById), new { id = combination.Id }, combination);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument for vehicle combination creation");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating vehicle combination");
            return StatusCode(500, "An error occurred while creating the vehicle combination");
        }
    }

    /// <summary>
    /// Update a vehicle combination
    /// </summary>
    /// <param name="id">Vehicle combination ID</param>
    /// <param name="updateVehicleCombinationDto">Vehicle combination update data</param>
    /// <returns>Updated vehicle combination</returns>
    [HttpPut("{id}")]
    public async Task<ActionResult<VehicleCombinationResponseDto>> Update(int id, [FromBody] UpdateVehicleCombinationDto updateVehicleCombinationDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetCurrentUserId();
            var combination = await _vehicleCombinationRepository.UpdateAsync(id, updateVehicleCombinationDto, userId);

            if (combination == null)
            {
                _logger.LogWarning("Vehicle combination with ID {Id} not found for update", id);
                return NotFound($"Vehicle combination with ID {id} not found");
            }

            _logger.LogInformation("Updated vehicle combination with ID {Id} by user {UserId}", id, userId);
            return Ok(combination);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument for vehicle combination update");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating vehicle combination with ID {Id}", id);
            return StatusCode(500, "An error occurred while updating the vehicle combination");
        }
    }

    /// <summary>
    /// Delete a vehicle combination
    /// </summary>
    /// <param name="id">Vehicle combination ID</param>
    /// <returns>Success or error</returns>
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        try
        {
            var deleted = await _vehicleCombinationRepository.DeleteAsync(id);
            if (!deleted)
            {
                _logger.LogWarning("Vehicle combination with ID {Id} not found for deletion", id);
                return NotFound($"Vehicle combination with ID {id} not found");
            }

            var userId = GetCurrentUserId();
            _logger.LogInformation("Deleted vehicle combination with ID {Id} by user {UserId}", id, userId);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting vehicle combination with ID {Id}", id);
            return StatusCode(500, "An error occurred while deleting the vehicle combination");
        }
    }

    /// <summary>
    /// Check if vehicle combination exists
    /// </summary>
    /// <param name="id">Vehicle combination ID</param>
    /// <returns>True if exists, false otherwise</returns>
    [HttpGet("{id}/exists")]
    public async Task<ActionResult<bool>> Exists(int id)
    {
        try
        {
            var exists = await _vehicleCombinationRepository.ExistsAsync(id);
            return Ok(exists);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking existence of vehicle combination with ID {Id}", id);
            return StatusCode(500, "An error occurred while checking vehicle combination existence");
        }
    }

    /// <summary>
    /// Get vehicle combinations for a specific tractor
    /// </summary>
    /// <param name="tractorId">Tractor ID</param>
    /// <returns>List of vehicle combinations for the tractor</returns>
    [HttpGet("by-tractor/{tractorId}")]
    public async Task<ActionResult<IEnumerable<VehicleCombinationListItemDto>>> GetByTractorId(int tractorId)
    {
        try
        {
            var combinations = await _vehicleCombinationRepository.GetByTractorIdAsync(tractorId);
            _logger.LogInformation("Retrieved {Count} vehicle combinations for tractor {TractorId}", combinations.Count(), tractorId);
            return Ok(combinations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving vehicle combinations for tractor {TractorId}", tractorId);
            return StatusCode(500, "An error occurred while retrieving tractor combinations");
        }
    }

    /// <summary>
    /// Search vehicle combinations
    /// </summary>
    /// <param name="query">Search term</param>
    /// <returns>List of matching vehicle combinations</returns>
    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<VehicleCombinationListItemDto>>> Search([FromQuery] string query)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest("Search query cannot be empty");
            }

            var combinations = await _vehicleCombinationRepository.SearchAsync(query);
            _logger.LogInformation("Found {Count} vehicle combinations matching search query '{Query}'", combinations.Count(), query);
            return Ok(combinations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching vehicle combinations with query '{Query}'", query);
            return StatusCode(500, "An error occurred while searching vehicle combinations");
        }
    }

    /// <summary>
    /// Add trailer to vehicle combination
    /// </summary>
    /// <param name="id">Vehicle combination ID</param>
    /// <param name="addTrailerDto">Trailer addition data</param>
    /// <returns>Created trailer association</returns>
    [HttpPost("{id}/trailers")]
    public async Task<ActionResult<VehicleCombinationTrailerResponseDto>> AddTrailer(int id, [FromBody] AddTrailerToCombinationDto addTrailerDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetCurrentUserId();
            var association = await _vehicleCombinationRepository.AddTrailerToCombinationAsync(id, addTrailerDto, userId);
            
            _logger.LogInformation("Added trailer {TrailerId} to combination {CombinationId} by user {UserId}", 
                addTrailerDto.TrailerId, id, userId);
            return Ok(association);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument for trailer addition");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding trailer {TrailerId} to combination {CombinationId}", addTrailerDto.TrailerId, id);
            return StatusCode(500, "An error occurred while adding trailer to combination");
        }
    }

    /// <summary>
    /// Remove trailer from vehicle combination
    /// </summary>
    /// <param name="id">Vehicle combination ID</param>
    /// <param name="trailerId">Trailer ID</param>
    /// <returns>Success or error</returns>
    [HttpDelete("{id}/trailers/{trailerId}")]
    public async Task<ActionResult> RemoveTrailer(int id, int trailerId)
    {
        try
        {
            var removed = await _vehicleCombinationRepository.RemoveTrailerFromCombinationAsync(id, trailerId);
            if (!removed)
            {
                _logger.LogWarning("Trailer {TrailerId} not found in combination {CombinationId}", trailerId, id);
                return NotFound($"Trailer {trailerId} is not part of combination {id}");
            }

            var userId = GetCurrentUserId();
            _logger.LogInformation("Removed trailer {TrailerId} from combination {CombinationId} by user {UserId}", 
                trailerId, id, userId);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing trailer {TrailerId} from combination {CombinationId}", trailerId, id);
            return StatusCode(500, "An error occurred while removing trailer from combination");
        }
    }

    /// <summary>
    /// Check if trailer is in vehicle combination
    /// </summary>
    /// <param name="id">Vehicle combination ID</param>
    /// <param name="trailerId">Trailer ID</param>
    /// <returns>True if trailer is in combination, false otherwise</returns>
    [HttpGet("{id}/trailers/{trailerId}/exists")]
    public async Task<ActionResult<bool>> IsTrailerInCombination(int id, int trailerId)
    {
        try
        {
            var isInCombination = await _vehicleCombinationRepository.IsTrailerInCombinationAsync(id, trailerId);
            return Ok(isInCombination);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if trailer {TrailerId} is in combination {CombinationId}", trailerId, id);
            return StatusCode(500, "An error occurred while checking trailer in combination");
        }
    }

    /// <summary>
    /// Get trailers in vehicle combination
    /// </summary>
    /// <param name="id">Vehicle combination ID</param>
    /// <returns>List of trailers in the combination</returns>
    [HttpGet("{id}/trailers")]
    public async Task<ActionResult<IEnumerable<TrailerListItemDto>>> GetTrailersInCombination(int id)
    {
        try
        {
            var trailers = await _vehicleCombinationRepository.GetTrailersInCombinationAsync(id);
            _logger.LogInformation("Retrieved {Count} trailers for combination {CombinationId}", trailers.Count(), id);
            return Ok(trailers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving trailers for combination {CombinationId}", id);
            return StatusCode(500, "An error occurred while retrieving combination trailers");
        }
    }

    /// <summary>
    /// Get combinations that include a specific trailer
    /// </summary>
    /// <param name="trailerId">Trailer ID</param>
    /// <returns>List of combinations that include the trailer</returns>
    [HttpGet("by-trailer/{trailerId}")]
    public async Task<ActionResult<IEnumerable<VehicleCombinationListItemDto>>> GetCombinationsWithTrailer(int trailerId)
    {
        try
        {
            var combinations = await _vehicleCombinationRepository.GetCombinationsWithTrailerAsync(trailerId);
            _logger.LogInformation("Retrieved {Count} combinations containing trailer {TrailerId}", combinations.Count(), trailerId);
            return Ok(combinations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving combinations for trailer {TrailerId}", trailerId);
            return StatusCode(500, "An error occurred while retrieving trailer combinations");
        }
    }

    /// <summary>
    /// Get default combination for a tractor
    /// </summary>
    /// <param name="tractorId">Tractor ID</param>
    /// <returns>Default combination for the tractor</returns>
    [HttpGet("default-for-tractor/{tractorId}")]
    public async Task<ActionResult<VehicleCombinationResponseDto>> GetDefaultCombinationForTractor(int tractorId)
    {
        try
        {
            var combination = await _vehicleCombinationRepository.GetDefaultCombinationForTractorAsync(tractorId);
            if (combination == null)
            {
                _logger.LogInformation("No default combination found for tractor {TractorId}", tractorId);
                return NotFound($"No default combination found for tractor {tractorId}");
            }

            return Ok(combination);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving default combination for tractor {TractorId}", tractorId);
            return StatusCode(500, "An error occurred while retrieving default combination");
        }
    }

    /// <summary>
    /// Set combination as default for its tractor
    /// </summary>
    /// <param name="id">Vehicle combination ID</param>
    /// <returns>Success or error</returns>
    [HttpPost("{id}/set-default")]
    public async Task<ActionResult> SetAsDefault(int id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var success = await _vehicleCombinationRepository.SetDefaultCombinationAsync(id, userId);
            if (!success)
            {
                _logger.LogWarning("Vehicle combination with ID {Id} not found", id);
                return NotFound($"Vehicle combination with ID {id} not found");
            }

            _logger.LogInformation("Set combination {CombinationId} as default by user {UserId}", id, userId);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting combination {CombinationId} as default", id);
            return StatusCode(500, "An error occurred while setting default combination");
        }
    }

    /// <summary>
    /// Remove default status from tractor
    /// </summary>
    /// <param name="tractorId">Tractor ID</param>
    /// <returns>Success or error</returns>
    [HttpDelete("default-for-tractor/{tractorId}")]
    public async Task<ActionResult> RemoveDefault(int tractorId)
    {
        try
        {
            var userId = GetCurrentUserId();
            var success = await _vehicleCombinationRepository.RemoveDefaultCombinationAsync(tractorId, userId);
            if (!success)
            {
                _logger.LogInformation("No default combination found for tractor {TractorId}", tractorId);
                return NotFound($"No default combination found for tractor {tractorId}");
            }

            _logger.LogInformation("Removed default combination for tractor {TractorId} by user {UserId}", tractorId, userId);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing default combination for tractor {TractorId}", tractorId);
            return StatusCode(500, "An error occurred while removing default combination");
        }
    }
}