using DepotDirectApi.Models.DTOs;
using DepotDirectApi.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DepotDirectApi.Controllers.Admin;

[Authorize]
[ApiController]
[Route("api/admin/[controller]")]
public class RegionsController : BaseController
{
    private readonly IRegionRepository _regionRepository;

    public RegionsController(IRegionRepository regionRepository)
    {
        _regionRepository = regionRepository;
    }

    /// <summary>
    /// Get all regions
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<RegionListItemDto>>> GetAll()
    {
        try
        {
            var regions = await _regionRepository.GetAllAsync();
            return Ok(regions);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    /// <summary>
    /// Get a specific region by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<RegionResponseDto>> GetById(int id)
    {
        try
        {
            var region = await _regionRepository.GetByIdAsync(id);
            
            if (region == null)
                return NotFound(new { message = "Region not found" });
                
            return Ok(region);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    /// <summary>
    /// Get regions by company ID
    /// </summary>
    [HttpGet("by-company/{companyId}")]
    public async Task<ActionResult<IEnumerable<RegionListItemDto>>> GetByCompanyId(int companyId)
    {
        try
        {
            var regions = await _regionRepository.GetByCompanyIdAsync(companyId);
            return Ok(regions);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    /// <summary>
    /// Search regions by name or code
    /// </summary>
    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<RegionListItemDto>>> Search([FromQuery] string query)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(query))
                return BadRequest(new { message = "Search query cannot be empty" });

            var regions = await _regionRepository.SearchAsync(query);
            return Ok(regions);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    /// <summary>
    /// Create a new region
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<RegionResponseDto>> Create([FromBody] CreateRegionDto createRegionDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetCurrentUserId();
            var region = await _regionRepository.CreateAsync(createRegionDto, userId);
            
            return CreatedAtAction(nameof(GetById), new { id = region.Id }, region);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    /// <summary>
    /// Update an existing region
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<RegionResponseDto>> Update(int id, [FromBody] UpdateRegionDto updateRegionDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetCurrentUserId();
            var region = await _regionRepository.UpdateAsync(id, updateRegionDto, userId);
            
            if (region == null)
                return NotFound(new { message = "Region not found" });
                
            return Ok(region);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    /// <summary>
    /// Delete a region
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        try
        {
            var success = await _regionRepository.DeleteAsync(id);
            
            if (!success)
                return NotFound(new { message = "Region not found" });
                
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }
}