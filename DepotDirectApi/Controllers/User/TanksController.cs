using DepotDirectApi.Models.DTOs;
using DepotDirectApi.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DepotDirectApi.Controllers.User;

[Authorize]
[ApiController]
[Route("api/user/[controller]")]
public class TanksController : BaseController
{
    private readonly ITankRepository _tankRepository;
    private readonly ILogger<TanksController> _logger;

    public TanksController(ITankRepository tankRepository, ILogger<TanksController> logger)
    {
        _tankRepository = tankRepository;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<SiteTankDto>> Create([FromBody] CreateTankDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetCurrentUserId();
            var tank = await _tankRepository.CreateTankAsync(dto, userId);
            return CreatedAtAction(nameof(GetById), new { id = tank.Id }, tank);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation error creating tank");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating tank");
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<SiteTankDto>> Update(int id, [FromBody] UpdateTankDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetCurrentUserId();
            var tank = await _tankRepository.UpdateTankAsync(id, dto, userId);
            if (tank == null)
                return NotFound(new { message = "Tank not found" });

            return Ok(tank);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation error updating tank {TankId}", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating tank {TankId}", id);
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var success = await _tankRepository.DeleteTankAsync(id);
            if (!success)
                return NotFound(new { message = "Tank not found" });

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting tank {TankId}", id);
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    [HttpGet("site/{siteId}")]
    public async Task<ActionResult<IEnumerable<SiteTankDto>>> GetBySite(int siteId)
    {
        try
        {
            var tanks = await _tankRepository.GetTanksBySiteAsync(siteId);
            return Ok(tanks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tanks for site {SiteId}", siteId);
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SiteTankWithInventoryDto>> GetById(int id)
    {
        try
        {
            var tank = await _tankRepository.GetTankWithInventoryAsync(id);
            if (tank == null)
                return NotFound(new { message = "Tank not found" });

            return Ok(tank);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tank {TankId}", id);
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    // New: full details endpoint returns tank, last 10 readings, deliveries and sales patterns
    [HttpGet("{id}/full")]
    public async Task<ActionResult<SiteTankFullDto>> GetFullById(int id)
    {
        try
        {
            var tank = await _tankRepository.GetTankFullDetailsAsync(id);
            if (tank == null)
                return NotFound(new { message = "Tank not found" });

            return Ok(tank);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving full tank details {TankId}", id);
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    // New: full details for all tanks at a site
    [HttpGet("site/{siteId}/full")]
    public async Task<ActionResult<IEnumerable<SiteTankFullDto>>> GetFullBySite(int siteId)
    {
        try
        {
            var tanks = await _tankRepository.GetTanksFullBySiteAsync(siteId);
            return Ok(tanks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving full tank details for site {SiteId}", siteId);
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }
}
