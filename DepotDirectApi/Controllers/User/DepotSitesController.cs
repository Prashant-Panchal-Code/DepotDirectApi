using DepotDirectApi.Controllers;
using DepotDirectApi.Models.DTOs;
using DepotDirectApi.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DepotDirectApi.Controllers.User;

[Authorize]
[Route("api/user/depot-sites")]
[ApiController]
public class DepotSitesController : BaseController
{
    private readonly IDepotSiteRepository _depotSiteRepository;
    private readonly ILogger<DepotSitesController> _logger;

    public DepotSitesController(
        IDepotSiteRepository depotSiteRepository,
        ILogger<DepotSitesController> logger)
    {
        _depotSiteRepository = depotSiteRepository;
        _logger = logger;
    }

    /// <summary>
    /// Get all depot-site routes
    /// </summary>
    /// <returns>List of depot-site routes</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<DepotSiteListItemDto>>> GetAll()
    {
        try
        {
            var routes = await _depotSiteRepository.GetAllAsync();
            _logger.LogInformation("Retrieved {Count} depot-site routes", routes.Count());
            return Ok(routes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving depot-site routes");
            return StatusCode(500, "An error occurred while retrieving depot-site routes");
        }
    }

    /// <summary>
    /// Get depot-site route by ID
    /// </summary>
    /// <param name="id">Route ID</param>
    /// <returns>Depot-site route details</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<DepotSiteResponseDto>> GetById(int id)
    {
        try
        {
            var route = await _depotSiteRepository.GetByIdAsync(id);
            if (route == null)
            {
                _logger.LogWarning("Depot-site route with ID {Id} not found", id);
                return NotFound($"Depot-site route with ID {id} not found");
            }

            return Ok(route);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving depot-site route with ID {Id}", id);
            return StatusCode(500, "An error occurred while retrieving the depot-site route");
        }
    }

    /// <summary>
    /// Create a new depot-site route
    /// </summary>
    /// <param name="createDepotSiteDto">Route creation data</param>
    /// <returns>Created route</returns>
    [HttpPost]
    public async Task<ActionResult<DepotSiteResponseDto>> Create([FromBody] CreateDepotSiteDto createDepotSiteDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetCurrentUserId();
            var route = await _depotSiteRepository.CreateAsync(createDepotSiteDto, userId);
            
            _logger.LogInformation("Created depot-site route with ID {Id} by user {UserId}", route.Id, userId);
            return CreatedAtAction(nameof(GetById), new { id = route.Id }, route);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument for depot-site route creation");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating depot-site route");
            return StatusCode(500, "An error occurred while creating the depot-site route");
        }
    }

    /// <summary>
    /// Update a depot-site route
    /// </summary>
    /// <param name="id">Route ID</param>
    /// <param name="updateDepotSiteDto">Route update data</param>
    /// <returns>Updated route</returns>
    [HttpPut("{id}")]
    public async Task<ActionResult<DepotSiteResponseDto>> Update(int id, [FromBody] UpdateDepotSiteDto updateDepotSiteDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetCurrentUserId();
            var route = await _depotSiteRepository.UpdateAsync(id, updateDepotSiteDto, userId);

            if (route == null)
            {
                _logger.LogWarning("Depot-site route with ID {Id} not found for update", id);
                return NotFound($"Depot-site route with ID {id} not found");
            }

            _logger.LogInformation("Updated depot-site route with ID {Id} by user {UserId}", id, userId);
            return Ok(route);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating depot-site route with ID {Id}", id);
            return StatusCode(500, "An error occurred while updating the depot-site route");
        }
    }

    /// <summary>
    /// Delete a depot-site route
    /// </summary>
    /// <param name="id">Route ID</param>
    /// <returns>Success or error</returns>
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        try
        {
            var deleted = await _depotSiteRepository.DeleteAsync(id);
            if (!deleted)
            {
                _logger.LogWarning("Depot-site route with ID {Id} not found for deletion", id);
                return NotFound($"Depot-site route with ID {id} not found");
            }

            var userId = GetCurrentUserId();
            _logger.LogInformation("Deleted depot-site route with ID {Id} by user {UserId}", id, userId);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting depot-site route with ID {Id}", id);
            return StatusCode(500, "An error occurred while deleting the depot-site route");
        }
    }

    /// <summary>
    /// Check if depot-site route exists
    /// </summary>
    /// <param name="id">Route ID</param>
    /// <returns>True if exists, false otherwise</returns>
    [HttpGet("{id}/exists")]
    public async Task<ActionResult<bool>> Exists(int id)
    {
        try
        {
            var exists = await _depotSiteRepository.ExistsAsync(id);
            return Ok(exists);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking existence of depot-site route with ID {Id}", id);
            return StatusCode(500, "An error occurred while checking route existence");
        }
    }

    /// <summary>
    /// Get routes from a specific depot
    /// </summary>
    /// <param name="depotId">Depot ID</param>
    /// <returns>List of routes from the depot</returns>
    [HttpGet("depot/{depotId}")]
    public async Task<ActionResult<IEnumerable<DepotSiteListItemDto>>> GetByDepotId(int depotId)
    {
        try
        {
            var routes = await _depotSiteRepository.GetByDepotIdAsync(depotId);
            _logger.LogInformation("Retrieved {Count} routes from depot {DepotId}", routes.Count(), depotId);
            return Ok(routes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving routes from depot {DepotId}", depotId);
            return StatusCode(500, "An error occurred while retrieving depot routes");
        }
    }

    /// <summary>
    /// Get routes to a specific site
    /// </summary>
    /// <param name="siteId">Site ID</param>
    /// <returns>List of routes to the site</returns>
    [HttpGet("site/{siteId}")]
    public async Task<ActionResult<IEnumerable<DepotSiteListItemDto>>> GetBySiteId(int siteId)
    {
        try
        {
            var routes = await _depotSiteRepository.GetBySiteIdAsync(siteId);
            _logger.LogInformation("Retrieved {Count} routes to site {SiteId}", routes.Count(), siteId);
            return Ok(routes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving routes to site {SiteId}", siteId);
            return StatusCode(500, "An error occurred while retrieving site routes");
        }
    }

    /// <summary>
    /// Get only active routes
    /// </summary>
    /// <returns>List of active routes</returns>
    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<DepotSiteListItemDto>>> GetActiveRoutes()
    {
        try
        {
            var routes = await _depotSiteRepository.GetActiveRoutesAsync();
            _logger.LogInformation("Retrieved {Count} active routes", routes.Count());
            return Ok(routes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active routes");
            return StatusCode(500, "An error occurred while retrieving active routes");
        }
    }

    /// <summary>
    /// Get only primary routes
    /// </summary>
    /// <returns>List of primary routes</returns>
    [HttpGet("primary")]
    public async Task<ActionResult<IEnumerable<DepotSiteListItemDto>>> GetPrimaryRoutes()
    {
        try
        {
            var routes = await _depotSiteRepository.GetPrimaryRoutesAsync();
            _logger.LogInformation("Retrieved {Count} primary routes", routes.Count());
            return Ok(routes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving primary routes");
            return StatusCode(500, "An error occurred while retrieving primary routes");
        }
    }

    /// <summary>
    /// Get routes for a specific company
    /// </summary>
    /// <param name="companyId">Company ID</param>
    /// <returns>List of routes for the company</returns>
    [HttpGet("company/{companyId}")]
    public async Task<ActionResult<IEnumerable<DepotSiteListItemDto>>> GetByCompanyId(int companyId)
    {
        try
        {
            var routes = await _depotSiteRepository.GetByCompanyIdAsync(companyId);
            _logger.LogInformation("Retrieved {Count} routes for company {CompanyId}", routes.Count(), companyId);
            return Ok(routes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving routes for company {CompanyId}", companyId);
            return StatusCode(500, "An error occurred while retrieving company routes");
        }
    }

    /// <summary>
    /// Search depot-site routes
    /// </summary>
    /// <param name="query">Search term</param>
    /// <returns>List of matching routes</returns>
    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<DepotSiteListItemDto>>> Search([FromQuery] string query)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest("Search query cannot be empty");
            }

            var routes = await _depotSiteRepository.SearchAsync(query);
            _logger.LogInformation("Found {Count} routes matching search query '{Query}'", routes.Count(), query);
            return Ok(routes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching routes with query '{Query}'", query);
            return StatusCode(500, "An error occurred while searching routes");
        }
    }

    /// <summary>
    /// Set primary depot for a site
    /// </summary>
    /// <param name="siteId">Site ID</param>
    /// <param name="depotId">Depot ID to set as primary</param>
    /// <returns>Updated route</returns>
    [HttpPut("site/{siteId}/primary-depot/{depotId}")]
    public async Task<ActionResult<DepotSiteResponseDto>> SetPrimaryDepotForSite(int siteId, int depotId)
    {
        try
        {
            var userId = GetCurrentUserId();
            var route = await _depotSiteRepository.SetPrimaryDepotForSiteAsync(siteId, depotId, userId);

            if (route == null)
            {
                _logger.LogWarning("Route from depot {DepotId} to site {SiteId} not found", depotId, siteId);
                return NotFound($"Route from depot {depotId} to site {siteId} not found");
            }

            _logger.LogInformation("Set depot {DepotId} as primary for site {SiteId} by user {UserId}", depotId, siteId, userId);
            return Ok(route);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument for setting primary depot");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting depot {DepotId} as primary for site {SiteId}", depotId, siteId);
            return StatusCode(500, "An error occurred while setting primary depot");
        }
    }
}