using DepotDirectApi.Controllers;
using DepotDirectApi.Models.DTOs;
using DepotDirectApi.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DepotDirectApi.Controllers.User;

[Authorize]
[Route("api/user/break-rules")]
[ApiController]
public class BreakRulesController : BaseController
{
    private readonly IBreakRuleRepository _breakRuleRepository;
    private readonly ILogger<BreakRulesController> _logger;

    public BreakRulesController(
        IBreakRuleRepository breakRuleRepository,
        ILogger<BreakRulesController> logger)
    {
        _breakRuleRepository = breakRuleRepository;
        _logger = logger;
    }

    /// <summary>
    /// Get all break rules
    /// </summary>
    /// <returns>List of break rules</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<BreakRuleListItemDto>>> GetAll()
    {
        try
        {
            var breakRules = await _breakRuleRepository.GetAllAsync();
            _logger.LogInformation("Retrieved {Count} break rules", breakRules.Count());
            return Ok(breakRules);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving break rules");
            return StatusCode(500, "An error occurred while retrieving break rules");
        }
    }

    /// <summary>
    /// Get break rule by ID
    /// </summary>
    /// <param name="id">Break rule ID</param>
    /// <returns>Break rule details</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<BreakRuleResponseDto>> GetById(int id)
    {
        try
        {
            var breakRule = await _breakRuleRepository.GetByIdAsync(id);
            if (breakRule == null)
            {
                _logger.LogWarning("Break rule with ID {Id} not found", id);
                return NotFound($"Break rule with ID {id} not found");
            }

            return Ok(breakRule);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving break rule with ID {Id}", id);
            return StatusCode(500, "An error occurred while retrieving the break rule");
        }
    }

    /// <summary>
    /// Create a new break rule
    /// </summary>
    /// <param name="createBreakRuleDto">Break rule creation data</param>
    /// <returns>Created break rule</returns>
    [HttpPost]
    public async Task<ActionResult<BreakRuleResponseDto>> Create([FromBody] CreateBreakRuleDto createBreakRuleDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetCurrentUserId();
            var breakRule = await _breakRuleRepository.CreateAsync(createBreakRuleDto, userId);
            
            _logger.LogInformation("Created break rule with ID {Id} by user {UserId}", breakRule.Id, userId);
            return CreatedAtAction(nameof(GetById), new { id = breakRule.Id }, breakRule);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument for break rule creation");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating break rule");
            return StatusCode(500, "An error occurred while creating the break rule");
        }
    }

    /// <summary>
    /// Update a break rule
    /// </summary>
    /// <param name="id">Break rule ID</param>
    /// <param name="updateBreakRuleDto">Break rule update data</param>
    /// <returns>Updated break rule</returns>
    [HttpPut("{id}")]
    public async Task<ActionResult<BreakRuleResponseDto>> Update(int id, [FromBody] UpdateBreakRuleDto updateBreakRuleDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetCurrentUserId();
            var breakRule = await _breakRuleRepository.UpdateAsync(id, updateBreakRuleDto, userId);

            if (breakRule == null)
            {
                _logger.LogWarning("Break rule with ID {Id} not found for update", id);
                return NotFound($"Break rule with ID {id} not found");
            }

            _logger.LogInformation("Updated break rule with ID {Id} by user {UserId}", id, userId);
            return Ok(breakRule);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument for break rule update");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating break rule with ID {Id}", id);
            return StatusCode(500, "An error occurred while updating the break rule");
        }
    }

    /// <summary>
    /// Delete a break rule
    /// </summary>
    /// <param name="id">Break rule ID</param>
    /// <returns>Success or error</returns>
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        try
        {
            var deleted = await _breakRuleRepository.DeleteAsync(id);
            if (!deleted)
            {
                _logger.LogWarning("Break rule with ID {Id} not found for deletion", id);
                return NotFound($"Break rule with ID {id} not found");
            }

            var userId = GetCurrentUserId();
            _logger.LogInformation("Deleted break rule with ID {Id} by user {UserId}", id, userId);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Cannot delete break rule with ID {Id}", id);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting break rule with ID {Id}", id);
            return StatusCode(500, "An error occurred while deleting the break rule");
        }
    }

    /// <summary>
    /// Check if break rule exists
    /// </summary>
    /// <param name="id">Break rule ID</param>
    /// <returns>True if exists, false otherwise</returns>
    [HttpGet("{id}/exists")]
    public async Task<ActionResult<bool>> Exists(int id)
    {
        try
        {
            var exists = await _breakRuleRepository.ExistsAsync(id);
            return Ok(exists);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking existence of break rule with ID {Id}", id);
            return StatusCode(500, "An error occurred while checking break rule existence");
        }
    }

    /// <summary>
    /// Get break rules for a specific company
    /// </summary>
    /// <param name="companyId">Company ID</param>
    /// <returns>List of break rules for the company</returns>
    [HttpGet("by-company/{companyId}")]
    public async Task<ActionResult<IEnumerable<BreakRuleListItemDto>>> GetByCompanyId(int companyId)
    {
        try
        {
            var breakRules = await _breakRuleRepository.GetByCompanyIdAsync(companyId);
            _logger.LogInformation("Retrieved {Count} break rules for company {CompanyId}", breakRules.Count(), companyId);
            return Ok(breakRules);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving break rules for company {CompanyId}", companyId);
            return StatusCode(500, "An error occurred while retrieving company break rules");
        }
    }

    /// <summary>
    /// Search break rules
    /// </summary>
    /// <param name="query">Search term</param>
    /// <returns>List of matching break rules</returns>
    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<BreakRuleListItemDto>>> Search([FromQuery] string query)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest("Search query cannot be empty");
            }

            var breakRules = await _breakRuleRepository.SearchAsync(query);
            _logger.LogInformation("Found {Count} break rules matching search query '{Query}'", breakRules.Count(), query);
            return Ok(breakRules);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching break rules with query '{Query}'", query);
            return StatusCode(500, "An error occurred while searching break rules");
        }
    }
}