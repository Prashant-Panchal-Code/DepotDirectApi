using DepotDirectApi.Models.DTOs;
using DepotDirectApi.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DepotDirectApi.Controllers.User;

[Authorize]
[ApiController]
[Route("api/user/[controller]")]
public class NotesController : BaseController
{
    private readonly INoteRepository _noteRepository;

    public NotesController(INoteRepository noteRepository)
    {
        _noteRepository = noteRepository;
    }

    [HttpPost]
    public async Task<ActionResult<NoteDto>> Create([FromBody] CreateNoteDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = GetCurrentUserId();
        try
        {
            var note = await _noteRepository.CreateAsync(dto, userId);
            return CreatedAtAction(nameof(GetById), new { id = note.Id }, note);
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

    [HttpGet("{id}")]
    public async Task<ActionResult<NoteDto>> GetById(int id)
    {
        var note = await _noteRepository.GetByIdAsync(id);
        if (note == null)
            return NotFound(new { message = "Note not found" });
        return Ok(note);
    }

    [HttpGet("by-target")]
    public async Task<ActionResult<IEnumerable<NoteDto>>> GetByTarget([FromQuery] int? siteId, [FromQuery] int? depotId, [FromQuery] int? parkingId, [FromQuery] int? vehicleId)
    {
        var notes = await _noteRepository.GetByTargetAsync(siteId, depotId, parkingId, vehicleId);
        return Ok(notes);
    }

    // Convenience endpoints for fetching by specific target id
    [HttpGet("by-site/{siteId}")]
    public async Task<ActionResult<IEnumerable<NoteDto>>> GetBySite(int siteId)
    {
        var notes = await _noteRepository.GetByTargetAsync(siteId, null, null, null);
        return Ok(notes);
    }

    [HttpGet("by-depot/{depotId}")]
    public async Task<ActionResult<IEnumerable<NoteDto>>> GetByDepot(int depotId)
    {
        var notes = await _noteRepository.GetByTargetAsync(null, depotId, null, null);
        return Ok(notes);
    }

    [HttpGet("by-parking/{parkingId}")]
    public async Task<ActionResult<IEnumerable<NoteDto>>> GetByParking(int parkingId)
    {
        var notes = await _noteRepository.GetByTargetAsync(null, null, parkingId, null);
        return Ok(notes);
    }

    [HttpGet("by-vehicle/{vehicleId}")]
    public async Task<ActionResult<IEnumerable<NoteDto>>> GetByVehicle(int vehicleId)
    {
        var notes = await _noteRepository.GetByTargetAsync(null, null, null, vehicleId);
        return Ok(notes);
    }

    [HttpPut("{id}/status")]
    public async Task<ActionResult<NoteDto>> UpdateStatus(int id, [FromBody] UpdateNoteStatusDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = GetCurrentUserId();
        try
        {
            var note = await _noteRepository.UpdateStatusAsync(id, dto, userId);
            if (note == null)
                return NotFound(new { message = "Note not found" });
            return Ok(note);
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

    [HttpGet("by-company/{companyId}")]
    public async Task<ActionResult<IEnumerable<NoteDto>>> GetByCompany(int companyId)
    {
        var notes = await _noteRepository.GetByCompanyAsync(companyId);
        return Ok(notes);
    }
}
