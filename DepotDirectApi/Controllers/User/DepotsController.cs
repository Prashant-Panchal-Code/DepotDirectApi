using DepotDirectApi.Models.DTOs;
using DepotDirectApi.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DepotDirectApi.Controllers.User;

[Authorize]
[ApiController]
[Route("api/user/[controller]")]
public class DepotsController : BaseController
{
    private readonly IDepotRepository _depotRepository;
    private readonly ILogger<DepotsController> _logger;

    public DepotsController(IDepotRepository depotRepository, ILogger<DepotsController> logger)
    {
        _depotRepository = depotRepository;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DepotListItemDto>>> GetAll()
    {
        try
        {
            var depots = await _depotRepository.GetAllAsync();
            return Ok(depots);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving depots");
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<DepotResponseDto>> GetById(int id)
    {
        try
        {
            var depot = await _depotRepository.GetByIdAsync(id);
            if (depot == null)
                return NotFound(new { message = "Depot not found" });

            return Ok(depot);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving depot {DepotId}", id);
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    [HttpGet("by-company/{companyId}")]
    public async Task<ActionResult<IEnumerable<DepotListItemDto>>> GetByCompanyId(int companyId)
    {
        try
        {
            var depots = await _depotRepository.GetByCompanyIdAsync(companyId);
            return Ok(depots);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving depots for company {CompanyId}", companyId);
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    [HttpGet("by-country/{countryId}")]
    public async Task<ActionResult<IEnumerable<DepotListItemDto>>> GetByCountryId(int countryId)
    {
        try
        {
            var depots = await _depotRepository.GetByCountryIdAsync(countryId);
            return Ok(depots);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving depots for country {CountryId}", countryId);
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    [HttpGet("by-region/{regionId}")]
    public async Task<ActionResult<IEnumerable<DepotListItemDto>>> GetByRegionId(int regionId)
    {
        try
        {
            var depots = await _depotRepository.GetByRegionIdAsync(regionId);
            return Ok(depots);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving depots for region {RegionId}", regionId);
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<DepotListItemDto>>> Search([FromQuery] string query)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(query))
                return BadRequest(new { message = "Search query cannot be empty" });

            var depots = await _depotRepository.SearchAsync(query);
            return Ok(depots);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching depots");
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult<DepotResponseDto>> Create([FromBody] CreateDepotDto createDepotDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetCurrentUserId();
            _logger.LogInformation("Creating depot with code {DepotCode}, name {DepotName}, region {RegionId} by user {UserId}",
                createDepotDto.DepotCode, createDepotDto.DepotName, createDepotDto.RegionId, userId);

            var depot = await _depotRepository.CreateAsync(createDepotDto, userId);

            _logger.LogInformation("Depot created successfully with ID {DepotId}", depot.Id);
            return CreatedAtAction(nameof(GetById), new { id = depot.Id }, depot);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation error creating depot");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating depot");
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<DepotResponseDto>> Update(int id, [FromBody] UpdateDepotDto updateDepotDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetCurrentUserId();
            _logger.LogInformation("Updating depot {DepotId} by user {UserId}", id, userId);

            var depot = await _depotRepository.UpdateAsync(id, updateDepotDto, userId);

            if (depot == null)
                return NotFound(new { message = "Depot not found" });

            _logger.LogInformation("Depot {DepotId} updated successfully", id);
            return Ok(depot);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation error updating depot {DepotId}", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating depot {DepotId}", id);
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("Deleting depot {DepotId} by user {UserId}", id, userId);

            var success = await _depotRepository.DeleteAsync(id);

            if (!success)
                return NotFound(new { message = "Depot not found" });

            _logger.LogInformation("Depot {DepotId} deleted successfully", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting depot {DepotId}", id);
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    [HttpGet("{id}/exists")]
    public async Task<ActionResult<bool>> Exists(int id)
    {
        try
        {
            var exists = await _depotRepository.ExistsAsync(id);
            return Ok(new { exists });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking depot existence {DepotId}", id);
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    [HttpPost("{depotId}/regions")]
    public async Task<ActionResult<RegionDepotDto>> AssignRegionToDepot(int depotId, [FromBody] AssignDepotToRegionDto assignDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetCurrentUserId();
            _logger.LogInformation("Assigning region {RegionId} to depot {DepotId} by user {UserId}",
                assignDto.RegionId, depotId, userId);

            var regionDepot = await _depotRepository.AssignDepotToRegionAsync(
                depotId,
                assignDto.RegionId,
                assignDto.DepotCode,
                userId);

            _logger.LogInformation("Region {RegionId} assigned to depot {DepotId} successfully", assignDto.RegionId, depotId);

            return Ok(regionDepot);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation error assigning region to depot {DepotId}", depotId);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning region to depot {DepotId}", depotId);
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    [HttpDelete("{depotId}/regions/{regionId}")]
    public async Task<ActionResult> RemoveRegionFromDepot(int depotId, int regionId)
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("Removing region {RegionId} from depot {DepotId} by user {UserId}", regionId, depotId, userId);

            var success = await _depotRepository.RemoveDepotFromRegionAsync(depotId, regionId);

            if (!success)
                return NotFound(new { message = "Depot-region assignment not found" });

            _logger.LogInformation("Region {RegionId} removed from depot {DepotId} successfully", regionId, depotId);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing region {RegionId} from depot {DepotId}", regionId, depotId);
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    [HttpGet("{depotId}/regions/{regionId}/exists")]
    public async Task<ActionResult<bool>> CheckDepotRegionAssignment(int depotId, int regionId)
    {
        try
        {
            var exists = await _depotRepository.IsDepotAssignedToRegionAsync(depotId, regionId);
            return Ok(new { exists });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking depot-region assignment for depot {DepotId} and region {RegionId}", depotId, regionId);
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }
}
