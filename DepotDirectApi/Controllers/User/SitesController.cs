using DepotDirectApi.Models.DTOs;
using DepotDirectApi.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DepotDirectApi.Controllers.User;

[Authorize]
[ApiController]
[Route("api/user/[controller]")]
public class SitesController : BaseController
{
    private readonly ISiteRepository _siteRepository;
    private readonly ILogger<SitesController> _logger;

    public SitesController(ISiteRepository siteRepository, ILogger<SitesController> logger)
    {
        _siteRepository = siteRepository;
        _logger = logger;
    }

    /// <summary>
    /// Get all sites
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<SiteListItemDto>>> GetAll()
    {
        try
        {
            var sites = await _siteRepository.GetAllAsync();
            return Ok(sites);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving sites");
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    /// <summary>
    /// Get a specific site by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<SiteResponseDto>> GetById(int id)
    {
        try
        {
            var site = await _siteRepository.GetByIdAsync(id);

            if (site == null)
                return NotFound(new { message = "Site not found" });

            return Ok(site);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving site {SiteId}", id);
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    /// <summary>
    /// Get sites by company ID
    /// </summary>
    [HttpGet("by-company/{companyId}")]
    public async Task<ActionResult<IEnumerable<SiteListItemDto>>> GetByCompanyId(int companyId)
    {
        try
        {
            var sites = await _siteRepository.GetByCompanyIdAsync(companyId);
            return Ok(sites);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving sites for company {CompanyId}", companyId);
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    /// <summary>
    /// Get sites by country ID
    /// </summary>
    [HttpGet("by-country/{countryId}")]
    public async Task<ActionResult<IEnumerable<SiteListItemDto>>> GetByCountryId(int countryId)
    {
        try
        {
            var sites = await _siteRepository.GetByCountryIdAsync(countryId);
            return Ok(sites);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving sites for country {CountryId}", countryId);
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    /// <summary>
    /// Get sites by region ID
    /// </summary>
    [HttpGet("by-region/{regionId}")]
    public async Task<ActionResult<IEnumerable<SiteListItemDto>>> GetByRegionId(int regionId)
    {
        try
        {
            var sites = await _siteRepository.GetByRegionIdAsync(regionId);
            return Ok(sites);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving sites for region {RegionId}", regionId);
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    /// <summary>
    /// Search sites by code, name, or town
    /// </summary>
    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<SiteListItemDto>>> Search([FromQuery] string query)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(query))
                return BadRequest(new { message = "Search query cannot be empty" });

            var sites = await _siteRepository.SearchAsync(query);
            return Ok(sites);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching sites");
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    /// <summary>
    /// Create a new site (User module - simplified creation with site_code, site_name, region_id)
    /// Company and Country are fetched from the region automatically
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<SiteResponseDto>> Create([FromBody] CreateSiteDto createSiteDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetCurrentUserId();
            _logger.LogInformation("Creating site with code {SiteCode}, name {SiteName}, region {RegionId} by user {UserId}",
                createSiteDto.SiteCode, createSiteDto.SiteName, createSiteDto.RegionId, userId);

            var site = await _siteRepository.CreateAsync(createSiteDto, userId);

            _logger.LogInformation("Site created successfully with ID {SiteId}", site.Id);
            return CreatedAtAction(nameof(GetById), new { id = site.Id }, site);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation error creating site");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating site");
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    /// <summary>
    /// Update an existing site (Update rest of the fields)
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<SiteResponseDto>> Update(int id, [FromBody] UpdateSiteDto updateSiteDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetCurrentUserId();
            _logger.LogInformation("Updating site {SiteId} by user {UserId}", id, userId);

            var site = await _siteRepository.UpdateAsync(id, updateSiteDto, userId);

            if (site == null)
                return NotFound(new { message = "Site not found" });

            _logger.LogInformation("Site {SiteId} updated successfully", id);
            return Ok(site);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation error updating site {SiteId}", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating site {SiteId}", id);
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    /// <summary>
    /// Delete a site (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("Deleting site {SiteId} by user {UserId}", id, userId);

            var success = await _siteRepository.DeleteAsync(id);

            if (!success)
                return NotFound(new { message = "Site not found" });

            _logger.LogInformation("Site {SiteId} deleted successfully", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting site {SiteId}", id);
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    /// <summary>
    /// Check if a site exists
    /// </summary>
    [HttpGet("{id}/exists")]
    public async Task<ActionResult<bool>> Exists(int id)
    {
        try
        {
            var exists = await _siteRepository.ExistsAsync(id);
            return Ok(new { exists });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking site existence {SiteId}", id);
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    /// <summary>
    /// Assign a region to a site
    /// </summary>
    [HttpPost("{siteId}/regions")]
    public async Task<ActionResult<RegionSiteDto>> AssignRegionToSite(int siteId, [FromBody] AssignRegionToSiteDto assignDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetCurrentUserId();
            _logger.LogInformation("Assigning region {RegionId} to site {SiteId} by user {UserId}",
                assignDto.RegionId, siteId, userId);

            var regionSite = await _siteRepository.AssignSiteToRegionAsync(
                siteId,
                assignDto.RegionId,
                assignDto.SiteCode,
                userId);

            _logger.LogInformation("Region {RegionId} assigned to site {SiteId} successfully",
                assignDto.RegionId, siteId);

            return Ok(regionSite);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation error assigning region to site {SiteId}", siteId);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning region to site {SiteId}", siteId);
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    /// <summary>
    /// Remove a region from a site
    /// </summary>
    [HttpDelete("{siteId}/regions/{regionId}")]
    public async Task<ActionResult> RemoveRegionFromSite(int siteId, int regionId)
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("Removing region {RegionId} from site {SiteId} by user {UserId}",
                regionId, siteId, userId);

            var success = await _siteRepository.RemoveSiteFromRegionAsync(siteId, regionId);

            if (!success)
                return NotFound(new { message = "Site-region assignment not found" });

            _logger.LogInformation("Region {RegionId} removed from site {SiteId} successfully",
                regionId, siteId);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing region {RegionId} from site {SiteId}", regionId, siteId);
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    /// <summary>
    /// Check if a site is assigned to a region
    /// </summary>
    [HttpGet("{siteId}/regions/{regionId}/exists")]
    public async Task<ActionResult<bool>> CheckSiteRegionAssignment(int siteId, int regionId)
    {
        try
        {
            var exists = await _siteRepository.IsSiteAssignedToRegionAsync(siteId, regionId);
            return Ok(new { exists });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking site-region assignment for site {SiteId} and region {RegionId}",
                siteId, regionId);
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }
}
