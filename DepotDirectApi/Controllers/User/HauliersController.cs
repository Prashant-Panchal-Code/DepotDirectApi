using DepotDirectApi.Models.DTOs;
using DepotDirectApi.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DepotDirectApi.Controllers.User;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class HauliersController : BaseController
{
    private readonly IHaulierRepository _haulierRepository;
    private readonly ILogger<HauliersController> _logger;

    public HauliersController(IHaulierRepository haulierRepository, ILogger<HauliersController> logger)
    {
        _haulierRepository = haulierRepository;
        _logger = logger;
    }

    /// <summary>
    /// Get all hauliers
    /// </summary>
    /// <returns>List of all hauliers</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<HaulierListItemDto>>> GetAll()
    {
        try
        {
            var hauliers = await _haulierRepository.GetAllAsync();
            _logger.LogInformation("Retrieved {Count} hauliers", hauliers.Count());
            return Ok(hauliers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving hauliers");
            return StatusCode(500, "An error occurred while retrieving hauliers");
        }
    }

    /// <summary>
    /// Get haulier by ID
    /// </summary>
    /// <param name="id">Haulier ID</param>
    /// <returns>Haulier details</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<HaulierResponseDto>> GetById(int id)
    {
        try
        {
            var haulier = await _haulierRepository.GetByIdAsync(id);
            if (haulier == null)
            {
                _logger.LogWarning("Haulier with ID {Id} not found", id);
                return NotFound($"Haulier with ID {id} not found");
            }

            _logger.LogInformation("Retrieved haulier with ID {Id}", id);
            return Ok(haulier);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving haulier with ID {Id}", id);
            return StatusCode(500, "An error occurred while retrieving the haulier");
        }
    }

    /// <summary>
    /// Create a new haulier
    /// </summary>
    /// <param name="createHaulierDto">Haulier creation data</param>
    /// <returns>Created haulier</returns>
    [HttpPost]
    public async Task<ActionResult<HaulierResponseDto>> Create([FromBody] CreateHaulierDto createHaulierDto)
    {
        try
        {
            var userId = GetCurrentUserId();
            var haulier = await _haulierRepository.CreateAsync(createHaulierDto, userId);
            
            _logger.LogInformation("Created new haulier with ID {Id} for region {RegionId}", haulier.Id, haulier.RegionId);
            return CreatedAtAction(nameof(GetById), new { id = haulier.Id }, haulier);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid data provided for haulier creation: {Message}", ex.Message);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating haulier");
            return StatusCode(500, "An error occurred while creating the haulier");
        }
    }

    /// <summary>
    /// Update an existing haulier
    /// </summary>
    /// <param name="id">Haulier ID</param>
    /// <param name="updateHaulierDto">Haulier update data</param>
    /// <returns>Updated haulier</returns>
    [HttpPut("{id}")]
    public async Task<ActionResult<HaulierResponseDto>> Update(int id, [FromBody] UpdateHaulierDto updateHaulierDto)
    {
        try
        {
            var userId = GetCurrentUserId();
            var haulier = await _haulierRepository.UpdateAsync(id, updateHaulierDto, userId);
            
            if (haulier == null)
            {
                _logger.LogWarning("Haulier with ID {Id} not found for update", id);
                return NotFound($"Haulier with ID {id} not found");
            }

            _logger.LogInformation("Updated haulier with ID {Id}", id);
            return Ok(haulier);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid data provided for haulier update: {Message}", ex.Message);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating haulier with ID {Id}", id);
            return StatusCode(500, "An error occurred while updating the haulier");
        }
    }

    /// <summary>
    /// Delete a haulier
    /// </summary>
    /// <param name="id">Haulier ID</param>
    /// <returns>Success status</returns>
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        try
        {
            var deleted = await _haulierRepository.DeleteAsync(id);
            if (!deleted)
            {
                _logger.LogWarning("Haulier with ID {Id} not found for deletion", id);
                return NotFound($"Haulier with ID {id} not found");
            }

            _logger.LogInformation("Deleted haulier with ID {Id}", id);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Cannot delete haulier with ID {Id}: {Message}", id, ex.Message);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting haulier with ID {Id}", id);
            return StatusCode(500, "An error occurred while deleting the haulier");
        }
    }

    /// <summary>
    /// Check if haulier exists
    /// </summary>
    /// <param name="id">Haulier ID</param>
    /// <returns>True if exists</returns>
    [HttpHead("{id}")]
    public async Task<ActionResult> Exists(int id)
    {
        try
        {
            var exists = await _haulierRepository.ExistsAsync(id);
            if (!exists)
            {
                return NotFound();
            }

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if haulier with ID {Id} exists", id);
            return StatusCode(500, "An error occurred while checking haulier existence");
        }
    }

    /// <summary>
    /// Get hauliers for a specific region
    /// </summary>
    /// <param name="regionId">Region ID</param>
    /// <returns>List of hauliers for the region</returns>
    [HttpGet("by-region/{regionId}")]
    public async Task<ActionResult<IEnumerable<HaulierListItemDto>>> GetByRegionId(int regionId)
    {
        try
        {
            var hauliers = await _haulierRepository.GetByRegionIdAsync(regionId);
            _logger.LogInformation("Retrieved {Count} hauliers for region {RegionId}", hauliers.Count(), regionId);
            return Ok(hauliers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving hauliers for region {RegionId}", regionId);
            return StatusCode(500, "An error occurred while retrieving region hauliers");
        }
    }

    /// <summary>
    /// Search hauliers
    /// </summary>
    /// <param name="query">Search query</param>
    /// <returns>List of matching hauliers</returns>
    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<HaulierListItemDto>>> Search([FromQuery] string query)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest("Search query cannot be empty");
            }

            var hauliers = await _haulierRepository.SearchAsync(query);
            _logger.LogInformation("Search for '{Query}' returned {Count} hauliers", query, hauliers.Count());
            return Ok(hauliers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching hauliers with query '{Query}'", query);
            return StatusCode(500, "An error occurred while searching hauliers");
        }
    }

    /// <summary>
    /// Get active hauliers
    /// </summary>
    /// <returns>List of active hauliers</returns>
    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<HaulierListItemDto>>> GetActive()
    {
        try
        {
            var hauliers = await _haulierRepository.GetActiveHauliersAsync();
            _logger.LogInformation("Retrieved {Count} active hauliers", hauliers.Count());
            return Ok(hauliers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active hauliers");
            return StatusCode(500, "An error occurred while retrieving active hauliers");
        }
    }

    /// <summary>
    /// Get hauliers by contract expiry date range
    /// </summary>
    /// <param name="fromDate">From date</param>
    /// <param name="toDate">To date</param>
    /// <returns>List of hauliers with contracts expiring in the date range</returns>
    [HttpGet("contract-expiry")]
    public async Task<ActionResult<IEnumerable<HaulierListItemDto>>> GetByContractExpiry([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
    {
        try
        {
            if (fromDate > toDate)
            {
                return BadRequest("From date cannot be greater than to date");
            }

            var hauliers = await _haulierRepository.GetByContractExpiryDateAsync(fromDate, toDate);
            _logger.LogInformation("Retrieved {Count} hauliers with contract expiry between {FromDate} and {ToDate}", 
                hauliers.Count(), fromDate, toDate);
            return Ok(hauliers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving hauliers by contract expiry date range");
            return StatusCode(500, "An error occurred while retrieving hauliers by contract expiry");
        }
    }
}