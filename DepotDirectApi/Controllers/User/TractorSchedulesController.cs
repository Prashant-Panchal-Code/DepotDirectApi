using DepotDirectApi.Controllers;
using DepotDirectApi.Models.DTOs;
using DepotDirectApi.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DepotDirectApi.Controllers.User;

[Authorize]
[Route("api/user/tractor-schedules")]
[ApiController]
public class TractorSchedulesController : BaseController
{
    private readonly ITractorScheduleRepository _tractorScheduleRepository;
    private readonly ILogger<TractorSchedulesController> _logger;

    public TractorSchedulesController(
        ITractorScheduleRepository tractorScheduleRepository,
        ILogger<TractorSchedulesController> logger)
    {
        _tractorScheduleRepository = tractorScheduleRepository;
        _logger = logger;
    }

    /// <summary>
    /// Get all tractor schedules
    /// </summary>
    /// <returns>List of tractor schedules</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TractorScheduleListItemDto>>> GetAll()
    {
        try
        {
            var schedules = await _tractorScheduleRepository.GetAllAsync();
            _logger.LogInformation("Retrieved {Count} tractor schedules", schedules.Count());
            return Ok(schedules);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tractor schedules");
            return StatusCode(500, "An error occurred while retrieving tractor schedules");
        }
    }

    /// <summary>
    /// Get tractor schedule by ID
    /// </summary>
    /// <param name="id">Tractor schedule ID</param>
    /// <returns>Tractor schedule details</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<TractorScheduleResponseDto>> GetById(int id)
    {
        try
        {
            var schedule = await _tractorScheduleRepository.GetByIdAsync(id);
            if (schedule == null)
            {
                _logger.LogWarning("Tractor schedule with ID {Id} not found", id);
                return NotFound($"Tractor schedule with ID {id} not found");
            }

            return Ok(schedule);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tractor schedule with ID {Id}", id);
            return StatusCode(500, "An error occurred while retrieving the tractor schedule");
        }
    }

    /// <summary>
    /// Create a new tractor schedule
    /// </summary>
    /// <param name="createTractorScheduleDto">Tractor schedule creation data</param>
    /// <returns>Created tractor schedule</returns>
    [HttpPost]
    public async Task<ActionResult<TractorScheduleResponseDto>> Create([FromBody] CreateTractorScheduleDto createTractorScheduleDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetCurrentUserId();
            var schedule = await _tractorScheduleRepository.CreateAsync(createTractorScheduleDto, userId);
            
            _logger.LogInformation("Created tractor schedule with ID {Id} by user {UserId}", schedule.Id, userId);
            return CreatedAtAction(nameof(GetById), new { id = schedule.Id }, schedule);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument for tractor schedule creation");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating tractor schedule");
            return StatusCode(500, "An error occurred while creating the tractor schedule");
        }
    }

    /// <summary>
    /// Update a tractor schedule
    /// </summary>
    /// <param name="id">Tractor schedule ID</param>
    /// <param name="updateTractorScheduleDto">Tractor schedule update data</param>
    /// <returns>Updated tractor schedule</returns>
    [HttpPut("{id}")]
    public async Task<ActionResult<TractorScheduleResponseDto>> Update(int id, [FromBody] UpdateTractorScheduleDto updateTractorScheduleDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetCurrentUserId();
            var schedule = await _tractorScheduleRepository.UpdateAsync(id, updateTractorScheduleDto, userId);

            if (schedule == null)
            {
                _logger.LogWarning("Tractor schedule with ID {Id} not found for update", id);
                return NotFound($"Tractor schedule with ID {id} not found");
            }

            _logger.LogInformation("Updated tractor schedule with ID {Id} by user {UserId}", id, userId);
            return Ok(schedule);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument for tractor schedule update");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating tractor schedule with ID {Id}", id);
            return StatusCode(500, "An error occurred while updating the tractor schedule");
        }
    }

    /// <summary>
    /// Delete a tractor schedule
    /// </summary>
    /// <param name="id">Tractor schedule ID</param>
    /// <returns>Success or error</returns>
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        try
        {
            var deleted = await _tractorScheduleRepository.DeleteAsync(id);
            if (!deleted)
            {
                _logger.LogWarning("Tractor schedule with ID {Id} not found for deletion", id);
                return NotFound($"Tractor schedule with ID {id} not found");
            }

            var userId = GetCurrentUserId();
            _logger.LogInformation("Deleted tractor schedule with ID {Id} by user {UserId}", id, userId);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting tractor schedule with ID {Id}", id);
            return StatusCode(500, "An error occurred while deleting the tractor schedule");
        }
    }

    /// <summary>
    /// Check if tractor schedule exists
    /// </summary>
    /// <param name="id">Tractor schedule ID</param>
    /// <returns>True if exists, false otherwise</returns>
    [HttpGet("{id}/exists")]
    public async Task<ActionResult<bool>> Exists(int id)
    {
        try
        {
            var exists = await _tractorScheduleRepository.ExistsAsync(id);
            return Ok(exists);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking existence of tractor schedule with ID {Id}", id);
            return StatusCode(500, "An error occurred while checking tractor schedule existence");
        }
    }

    /// <summary>
    /// Get schedules for a specific tractor
    /// </summary>
    /// <param name="tractorId">Tractor ID</param>
    /// <returns>List of schedules for the tractor</returns>
    [HttpGet("by-tractor/{tractorId}")]
    public async Task<ActionResult<IEnumerable<TractorScheduleListItemDto>>> GetByTractorId(int tractorId)
    {
        try
        {
            var schedules = await _tractorScheduleRepository.GetByTractorIdAsync(tractorId);
            _logger.LogInformation("Retrieved {Count} schedules for tractor {TractorId}", schedules.Count(), tractorId);
            return Ok(schedules);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving schedules for tractor {TractorId}", tractorId);
            return StatusCode(500, "An error occurred while retrieving tractor schedules");
        }
    }

    /// <summary>
    /// Get schedules for a specific driver
    /// </summary>
    /// <param name="driverId">Driver ID</param>
    /// <returns>List of schedules for the driver</returns>
    [HttpGet("by-driver/{driverId}")]
    public async Task<ActionResult<IEnumerable<TractorScheduleListItemDto>>> GetByDriverId(int driverId)
    {
        try
        {
            var schedules = await _tractorScheduleRepository.GetByDriverIdAsync(driverId);
            _logger.LogInformation("Retrieved {Count} schedules for driver {DriverId}", schedules.Count(), driverId);
            return Ok(schedules);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving schedules for driver {DriverId}", driverId);
            return StatusCode(500, "An error occurred while retrieving driver schedules");
        }
    }

    /// <summary>
    /// Get schedules for a specific day of week
    /// </summary>
    /// <param name="dayOfWeek">Day of week (0=Sunday, 6=Saturday)</param>
    /// <returns>List of schedules for the day</returns>
    [HttpGet("by-day/{dayOfWeek}")]
    public async Task<ActionResult<IEnumerable<TractorScheduleListItemDto>>> GetByDayOfWeek(int dayOfWeek)
    {
        try
        {
            if (dayOfWeek < 0 || dayOfWeek > 6)
            {
                return BadRequest("Day of week must be between 0 (Sunday) and 6 (Saturday)");
            }

            var schedules = await _tractorScheduleRepository.GetByDayOfWeekAsync(dayOfWeek);
            _logger.LogInformation("Retrieved {Count} schedules for day of week {DayOfWeek}", schedules.Count(), dayOfWeek);
            return Ok(schedules);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving schedules for day of week {DayOfWeek}", dayOfWeek);
            return StatusCode(500, "An error occurred while retrieving schedules by day");
        }
    }

    /// <summary>
    /// Get schedules for a specific depot
    /// </summary>
    /// <param name="depotId">Depot ID</param>
    /// <returns>List of schedules involving the depot</returns>
    [HttpGet("by-depot/{depotId}")]
    public async Task<ActionResult<IEnumerable<TractorScheduleListItemDto>>> GetByDepotId(int depotId)
    {
        try
        {
            var schedules = await _tractorScheduleRepository.GetByDepotIdAsync(depotId);
            _logger.LogInformation("Retrieved {Count} schedules for depot {DepotId}", schedules.Count(), depotId);
            return Ok(schedules);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving schedules for depot {DepotId}", depotId);
            return StatusCode(500, "An error occurred while retrieving depot schedules");
        }
    }

    /// <summary>
    /// Get schedules for a specific parking
    /// </summary>
    /// <param name="parkingId">Parking ID</param>
    /// <returns>List of schedules involving the parking</returns>
    [HttpGet("by-parking/{parkingId}")]
    public async Task<ActionResult<IEnumerable<TractorScheduleListItemDto>>> GetByParkingId(int parkingId)
    {
        try
        {
            var schedules = await _tractorScheduleRepository.GetByParkingIdAsync(parkingId);
            _logger.LogInformation("Retrieved {Count} schedules for parking {ParkingId}", schedules.Count(), parkingId);
            return Ok(schedules);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving schedules for parking {ParkingId}", parkingId);
            return StatusCode(500, "An error occurred while retrieving parking schedules");
        }
    }

    /// <summary>
    /// Get schedules for a date range
    /// </summary>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <returns>List of schedules in the date range</returns>
    [HttpGet("by-date-range")]
    public async Task<ActionResult<IEnumerable<TractorScheduleListItemDto>>> GetSchedulesForDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        try
        {
            if (startDate >= endDate)
            {
                return BadRequest("Start date must be before end date");
            }

            var schedules = await _tractorScheduleRepository.GetSchedulesForDateRangeAsync(startDate, endDate);
            _logger.LogInformation("Retrieved {Count} schedules for date range {StartDate} to {EndDate}", 
                schedules.Count(), startDate, endDate);
            return Ok(schedules);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving schedules for date range {StartDate} to {EndDate}", startDate, endDate);
            return StatusCode(500, "An error occurred while retrieving schedules by date range");
        }
    }

    /// <summary>
    /// Check for schedule conflicts
    /// </summary>
    /// <param name="tractorId">Tractor ID</param>
    /// <param name="dayOfWeek">Day of week</param>
    /// <param name="startTime">Start time</param>
    /// <param name="endTime">End time</param>
    /// <param name="excludeId">Schedule ID to exclude from conflict check</param>
    /// <returns>True if there are conflicts, false otherwise</returns>
    [HttpGet("check-tractor-conflict")]
    public async Task<ActionResult<bool>> CheckTractorConflict([FromQuery] int tractorId, [FromQuery] int dayOfWeek, 
        [FromQuery] TimeSpan startTime, [FromQuery] TimeSpan endTime, [FromQuery] int? excludeId = null)
    {
        try
        {
            if (dayOfWeek < 0 || dayOfWeek > 6)
            {
                return BadRequest("Day of week must be between 0 (Sunday) and 6 (Saturday)");
            }

            if (startTime >= endTime)
            {
                return BadRequest("Start time must be before end time");
            }

            var hasConflict = await _tractorScheduleRepository.HasScheduleConflictAsync(tractorId, dayOfWeek, startTime, endTime, excludeId);
            return Ok(hasConflict);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking tractor schedule conflict for tractor {TractorId}", tractorId);
            return StatusCode(500, "An error occurred while checking schedule conflict");
        }
    }

    /// <summary>
    /// Check for driver conflicts
    /// </summary>
    /// <param name="driverId">Driver ID</param>
    /// <param name="dayOfWeek">Day of week</param>
    /// <param name="startTime">Start time</param>
    /// <param name="endTime">End time</param>
    /// <param name="excludeId">Schedule ID to exclude from conflict check</param>
    /// <returns>True if there are conflicts, false otherwise</returns>
    [HttpGet("check-driver-conflict")]
    public async Task<ActionResult<bool>> CheckDriverConflict([FromQuery] int driverId, [FromQuery] int dayOfWeek, 
        [FromQuery] TimeSpan startTime, [FromQuery] TimeSpan endTime, [FromQuery] int? excludeId = null)
    {
        try
        {
            if (dayOfWeek < 0 || dayOfWeek > 6)
            {
                return BadRequest("Day of week must be between 0 (Sunday) and 6 (Saturday)");
            }

            if (startTime >= endTime)
            {
                return BadRequest("Start time must be before end time");
            }

            var hasConflict = await _tractorScheduleRepository.HasDriverConflictAsync(driverId, dayOfWeek, startTime, endTime, excludeId);
            return Ok(hasConflict);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking driver schedule conflict for driver {DriverId}", driverId);
            return StatusCode(500, "An error occurred while checking driver conflict");
        }
    }

    /// <summary>
    /// Get available schedules for a time slot
    /// </summary>
    /// <param name="dayOfWeek">Day of week</param>
    /// <param name="startTime">Start time</param>
    /// <param name="endTime">End time</param>
    /// <returns>List of available schedules</returns>
    [HttpGet("available-schedules")]
    public async Task<ActionResult<IEnumerable<TractorScheduleListItemDto>>> GetAvailableSchedules(
        [FromQuery] int dayOfWeek, [FromQuery] TimeSpan startTime, [FromQuery] TimeSpan endTime)
    {
        try
        {
            if (dayOfWeek < 0 || dayOfWeek > 6)
            {
                return BadRequest("Day of week must be between 0 (Sunday) and 6 (Saturday)");
            }

            if (startTime >= endTime)
            {
                return BadRequest("Start time must be before end time");
            }

            var schedules = await _tractorScheduleRepository.GetAvailableSchedulesAsync(dayOfWeek, startTime, endTime);
            _logger.LogInformation("Retrieved {Count} available schedules for day {DayOfWeek} from {StartTime} to {EndTime}", 
                schedules.Count(), dayOfWeek, startTime, endTime);
            return Ok(schedules);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving available schedules for day {DayOfWeek}", dayOfWeek);
            return StatusCode(500, "An error occurred while retrieving available schedules");
        }
    }

    /// <summary>
    /// Get available drivers for a time slot
    /// </summary>
    /// <param name="dayOfWeek">Day of week</param>
    /// <param name="startTime">Start time</param>
    /// <param name="endTime">End time</param>
    /// <returns>List of available drivers</returns>
    [HttpGet("available-drivers")]
    public async Task<ActionResult<IEnumerable<DriverListItemDto>>> GetAvailableDrivers(
        [FromQuery] int dayOfWeek, [FromQuery] TimeSpan startTime, [FromQuery] TimeSpan endTime)
    {
        try
        {
            if (dayOfWeek < 0 || dayOfWeek > 6)
            {
                return BadRequest("Day of week must be between 0 (Sunday) and 6 (Saturday)");
            }

            if (startTime >= endTime)
            {
                return BadRequest("Start time must be before end time");
            }

            var drivers = await _tractorScheduleRepository.GetAvailableDriversForScheduleAsync(dayOfWeek, startTime, endTime);
            _logger.LogInformation("Retrieved {Count} available drivers for day {DayOfWeek} from {StartTime} to {EndTime}", 
                drivers.Count(), dayOfWeek, startTime, endTime);
            return Ok(drivers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving available drivers for day {DayOfWeek}", dayOfWeek);
            return StatusCode(500, "An error occurred while retrieving available drivers");
        }
    }

    /// <summary>
    /// Get available tractors for a time slot
    /// </summary>
    /// <param name="dayOfWeek">Day of week</param>
    /// <param name="startTime">Start time</param>
    /// <param name="endTime">End time</param>
    /// <returns>List of available tractors</returns>
    [HttpGet("available-tractors")]
    public async Task<ActionResult<IEnumerable<TractorListItemDto>>> GetAvailableTractors(
        [FromQuery] int dayOfWeek, [FromQuery] TimeSpan startTime, [FromQuery] TimeSpan endTime)
    {
        try
        {
            if (dayOfWeek < 0 || dayOfWeek > 6)
            {
                return BadRequest("Day of week must be between 0 (Sunday) and 6 (Saturday)");
            }

            if (startTime >= endTime)
            {
                return BadRequest("Start time must be before end time");
            }

            var tractors = await _tractorScheduleRepository.GetAvailableTractorsForScheduleAsync(dayOfWeek, startTime, endTime);
            _logger.LogInformation("Retrieved {Count} available tractors for day {DayOfWeek} from {StartTime} to {EndTime}", 
                tractors.Count(), dayOfWeek, startTime, endTime);
            return Ok(tractors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving available tractors for day {DayOfWeek}", dayOfWeek);
            return StatusCode(500, "An error occurred while retrieving available tractors");
        }
    }
}