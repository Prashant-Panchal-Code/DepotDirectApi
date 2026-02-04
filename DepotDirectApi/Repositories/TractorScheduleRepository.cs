using DepotDirectApi.Data;
using DepotDirectApi.Models.DTOs;
using DepotDirectApi.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace DepotDirectApi.Repositories;

public class TractorScheduleRepository : ITractorScheduleRepository
{
    private readonly DepotDirectDbContext _context;
    private readonly ILogger<TractorScheduleRepository> _logger;

    public TractorScheduleRepository(DepotDirectDbContext context, ILogger<TractorScheduleRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<TractorScheduleListItemDto>> GetAllAsync()
    {
        return await _context.TractorSchedules
            .Include(ts => ts.Tractor)
            .Include(ts => ts.Driver)
            .Include(ts => ts.StartDepot)
            .Include(ts => ts.StartParking)
            .Include(ts => ts.EndDepot)
            .Include(ts => ts.EndParking)
            .Where(ts => ts.DeletedAt == null)
            .Select(ts => new TractorScheduleListItemDto
            {
                Id = ts.Id,
                TractorId = ts.TractorId,
                TractorName = ts.Tractor.TractorName,
                TractorCode = ts.Tractor.TractorCode,
                DriverId = ts.DriverId,
                DriverName = ts.Driver != null ? $"{ts.Driver.FirstName} {ts.Driver.LastName}" : null,
                DayOfWeek = ts.DayOfWeek,
                ShiftStartTime = ts.ShiftStartTime,
                ShiftEndTime = ts.ShiftEndTime,
                StartLocationName = ts.StartDepot != null ? ts.StartDepot.DepotName : ts.StartParking!.ParkingName,
                EndLocationName = ts.EndDepot != null ? ts.EndDepot.DepotName : ts.EndParking!.ParkingName,
                IsOvertime = ts.IsOvertime,
                Active = ts.Active,
                CreatedAt = ts.CreatedAt,
                UpdatedAt = ts.UpdatedAt
            })
            .OrderBy(ts => ts.TractorName)
            .ThenBy(ts => ts.DayOfWeek)
            .ThenBy(ts => ts.ShiftStartTime)
            .ToListAsync();
    }

    public async Task<TractorScheduleResponseDto?> GetByIdAsync(int id)
    {
        return await _context.TractorSchedules
            .Include(ts => ts.Tractor)
                .ThenInclude(t => t.Haulier)
            .Include(ts => ts.Driver)
                .ThenInclude(d => d.Company)
            .Include(ts => ts.StartDepot)
                .ThenInclude(d => d.Company)
            .Include(ts => ts.StartParking)
                .ThenInclude(p => p.Company)
            .Include(ts => ts.EndDepot)
                .ThenInclude(d => d.Company)
            .Include(ts => ts.EndParking)
                .ThenInclude(p => p.Company)
            .Where(ts => ts.Id == id && ts.DeletedAt == null)
            .Select(ts => new TractorScheduleResponseDto
            {
                Id = ts.Id,
                TractorId = ts.TractorId,
                DriverId = ts.DriverId,
                DayOfWeek = ts.DayOfWeek,
                ShiftStartTime = ts.ShiftStartTime,
                ShiftEndTime = ts.ShiftEndTime,
                StartDepotId = ts.StartDepotId,
                StartParkingId = ts.StartParkingId,
                EndDepotId = ts.EndDepotId,
                EndParkingId = ts.EndParkingId,
                IsOvertime = ts.IsOvertime,
                Active = ts.Active,
                CreatedBy = ts.CreatedBy,
                CreatedAt = ts.CreatedAt,
                UpdatedAt = ts.UpdatedAt,
                DeletedAt = ts.DeletedAt,
                Tractor = new TractorListItemDto
                {
                    Id = ts.Tractor.Id,
                    TractorCode = ts.Tractor.TractorCode,
                    TractorName = ts.Tractor.TractorName,
                    LicensePlate = ts.Tractor.LicensePlate,
                    HaulierId = ts.Tractor.HaulierId,
                    HaulierName = ts.Tractor.Haulier.HaulierName,
                    Status = ts.Tractor.Status,
                    PumpAvailable = ts.Tractor.PumpAvailable,
                    PumpFlowRateLpm = ts.Tractor.PumpFlowRateLpm,
                    CurbWeightKg = ts.Tractor.CurbWeightKg,
                    NumberOfAxles = ts.Tractor.NumberOfAxles,
                    CreatedAt = ts.Tractor.CreatedAt,
                    UpdatedAt = ts.Tractor.UpdatedAt
                },
                Driver = ts.Driver != null ? new DriverListItemDto
                {
                    Id = ts.Driver.Id,
                    DriverCode = ts.Driver.DriverCode,
                    FirstName = ts.Driver.FirstName,
                    LastName = ts.Driver.LastName,
                    CompanyId = ts.Driver.CompanyId,
                    CompanyName = ts.Driver.Company.Name,
                    LicenseNumber = ts.Driver.LicenseNumber,
                    LicenseExpiry = ts.Driver.LicenseExpiry,
                    HazmatCertified = ts.Driver.HazmatCertified,
                    Active = ts.Driver.Active,
                    Status = ts.Driver.Status,
                    MobileNumber = ts.Driver.MobileNumber,
                    Email = ts.Driver.Email,
                    CreatedAt = ts.Driver.CreatedAt,
                    UpdatedAt = ts.Driver.UpdatedAt
                } : null,
                StartDepot = ts.StartDepot != null ? new DepotListItemDto
                {
                    Id = ts.StartDepot.Id,
                    DepotCode = ts.StartDepot.DepotCode,
                    DepotName = ts.StartDepot.DepotName,
                    Town = ts.StartDepot.Town,
                    Active = ts.StartDepot.Active,
                    Priority = ts.StartDepot.Priority,
                    Latitude = ts.StartDepot.Latitude,
                    Longitude = ts.StartDepot.Longitude,
                    LatLong = ts.StartDepot.LatLong,
                    Street = ts.StartDepot.Street,
                    PostalCode = ts.StartDepot.PostalCode,
                    CompanyId = ts.StartDepot.CompanyId,
                    CompanyName = ts.StartDepot.Company.Name,
                    CountryId = ts.StartDepot.CountryId,
                    CountryName = ts.StartDepot.Country.Name,
                    CreatedAt = ts.StartDepot.CreatedAt,
                    UpdatedAt = ts.StartDepot.UpdatedAt
                } : null,
                StartParking = ts.StartParking != null ? new ParkingListItemDto
                {
                    Id = ts.StartParking.Id,
                    ParkingCode = ts.StartParking.ParkingCode,
                    ParkingName = ts.StartParking.ParkingName,
                    Town = ts.StartParking.Town,
                    Active = ts.StartParking.Active,
                    ParkingSpaces = ts.StartParking.ParkingSpaces,
                    Latitude = ts.StartParking.Latitude,
                    Longitude = ts.StartParking.Longitude,
                    LatLong = ts.StartParking.LatLong,
                    Street = ts.StartParking.Street,
                    PostalCode = ts.StartParking.PostalCode,
                    CompanyId = ts.StartParking.CompanyId,
                    CompanyName = ts.StartParking.Company.Name,
                    CountryId = ts.StartParking.CountryId,
                    CountryName = ts.StartParking.Country.Name,
                    CreatedAt = ts.StartParking.CreatedAt,
                    UpdatedAt = ts.StartParking.UpdatedAt
                } : null,
                EndDepot = ts.EndDepot != null ? new DepotListItemDto
                {
                    Id = ts.EndDepot.Id,
                    DepotCode = ts.EndDepot.DepotCode,
                    DepotName = ts.EndDepot.DepotName,
                    Town = ts.EndDepot.Town,
                    Active = ts.EndDepot.Active,
                    Priority = ts.EndDepot.Priority,
                    Latitude = ts.EndDepot.Latitude,
                    Longitude = ts.EndDepot.Longitude,
                    LatLong = ts.EndDepot.LatLong,
                    Street = ts.EndDepot.Street,
                    PostalCode = ts.EndDepot.PostalCode,
                    CompanyId = ts.EndDepot.CompanyId,
                    CompanyName = ts.EndDepot.Company.Name,
                    CountryId = ts.EndDepot.CountryId,
                    CountryName = ts.EndDepot.Country.Name,
                    CreatedAt = ts.EndDepot.CreatedAt,
                    UpdatedAt = ts.EndDepot.UpdatedAt
                } : null,
                EndParking = ts.EndParking != null ? new ParkingListItemDto
                {
                    Id = ts.EndParking.Id,
                    ParkingCode = ts.EndParking.ParkingCode,
                    ParkingName = ts.EndParking.ParkingName,
                    Town = ts.EndParking.Town,
                    Active = ts.EndParking.Active,
                    ParkingSpaces = ts.EndParking.ParkingSpaces,
                    Latitude = ts.EndParking.Latitude,
                    Longitude = ts.EndParking.Longitude,
                    LatLong = ts.EndParking.LatLong,
                    Street = ts.EndParking.Street,
                    PostalCode = ts.EndParking.PostalCode,
                    CompanyId = ts.EndParking.CompanyId,
                    CompanyName = ts.EndParking.Company.Name,
                    CountryId = ts.EndParking.CountryId,
                    CountryName = ts.EndParking.Country.Name,
                    CreatedAt = ts.EndParking.CreatedAt,
                    UpdatedAt = ts.EndParking.UpdatedAt
                } : null
            })
            .FirstOrDefaultAsync();
    }

    public async Task<TractorScheduleResponseDto> CreateAsync(CreateTractorScheduleDto createTractorScheduleDto, int? createdBy = null)
    {
        // Validate location constraints
        ValidateLocationConstraints(createTractorScheduleDto.StartDepotId, createTractorScheduleDto.StartParkingId, 
                                  createTractorScheduleDto.EndDepotId, createTractorScheduleDto.EndParkingId);

        // Check for tractor schedule conflicts
        if (await HasScheduleConflictAsync(createTractorScheduleDto.TractorId, createTractorScheduleDto.DayOfWeek, 
                                         createTractorScheduleDto.ShiftStartTime, createTractorScheduleDto.ShiftEndTime))
        {
            throw new ArgumentException("Schedule conflicts with existing tractor schedule");
        }

        // Check for driver conflicts if driver is assigned
        if (createTractorScheduleDto.DriverId.HasValue &&
            await HasDriverConflictAsync(createTractorScheduleDto.DriverId.Value, createTractorScheduleDto.DayOfWeek, 
                                       createTractorScheduleDto.ShiftStartTime, createTractorScheduleDto.ShiftEndTime))
        {
            throw new ArgumentException("Schedule conflicts with existing driver schedule");
        }

        var tractorSchedule = new TractorSchedule
        {
            TractorId = createTractorScheduleDto.TractorId,
            DriverId = createTractorScheduleDto.DriverId,
            DayOfWeek = createTractorScheduleDto.DayOfWeek,
            ShiftStartTime = createTractorScheduleDto.ShiftStartTime,
            ShiftEndTime = createTractorScheduleDto.ShiftEndTime,
            StartDepotId = createTractorScheduleDto.StartDepotId,
            StartParkingId = createTractorScheduleDto.StartParkingId,
            EndDepotId = createTractorScheduleDto.EndDepotId,
            EndParkingId = createTractorScheduleDto.EndParkingId,
            IsOvertime = createTractorScheduleDto.IsOvertime ?? false,
            Active = createTractorScheduleDto.Active ?? true,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.TractorSchedules.Add(tractorSchedule);
        await _context.SaveChangesAsync();

        return await GetByIdAsync(tractorSchedule.Id) ?? throw new InvalidOperationException("Failed to retrieve created tractor schedule");
    }

    public async Task<TractorScheduleResponseDto?> UpdateAsync(int id, UpdateTractorScheduleDto updateTractorScheduleDto, int? updatedBy = null)
    {
        var tractorSchedule = await _context.TractorSchedules.FindAsync(id);
        if (tractorSchedule == null || tractorSchedule.DeletedAt != null)
            return null;

        // Validate location constraints if any location fields are being updated
        if (updateTractorScheduleDto.StartDepotId.HasValue || updateTractorScheduleDto.StartParkingId.HasValue ||
            updateTractorScheduleDto.EndDepotId.HasValue || updateTractorScheduleDto.EndParkingId.HasValue)
        {
            ValidateLocationConstraints(
                updateTractorScheduleDto.StartDepotId ?? tractorSchedule.StartDepotId,
                updateTractorScheduleDto.StartParkingId ?? tractorSchedule.StartParkingId,
                updateTractorScheduleDto.EndDepotId ?? tractorSchedule.EndDepotId,
                updateTractorScheduleDto.EndParkingId ?? tractorSchedule.EndParkingId);
        }

        // Check for schedule conflicts if time or day is being updated
        if (updateTractorScheduleDto.DayOfWeek.HasValue || updateTractorScheduleDto.ShiftStartTime.HasValue || updateTractorScheduleDto.ShiftEndTime.HasValue)
        {
            var dayOfWeek = updateTractorScheduleDto.DayOfWeek ?? tractorSchedule.DayOfWeek;
            var startTime = updateTractorScheduleDto.ShiftStartTime ?? tractorSchedule.ShiftStartTime;
            var endTime = updateTractorScheduleDto.ShiftEndTime ?? tractorSchedule.ShiftEndTime;

            if (await HasScheduleConflictAsync(tractorSchedule.TractorId, dayOfWeek, startTime, endTime, id))
            {
                throw new ArgumentException("Schedule conflicts with existing tractor schedule");
            }

            // Check driver conflicts if driver is assigned or being changed
            var driverId = updateTractorScheduleDto.DriverId ?? tractorSchedule.DriverId;
            if (driverId.HasValue && await HasDriverConflictAsync(driverId.Value, dayOfWeek, startTime, endTime, id))
            {
                throw new ArgumentException("Schedule conflicts with existing driver schedule");
            }
        }

        // Update only provided fields
        if (updateTractorScheduleDto.DriverId.HasValue)
            tractorSchedule.DriverId = updateTractorScheduleDto.DriverId.Value;
        if (updateTractorScheduleDto.DayOfWeek.HasValue)
            tractorSchedule.DayOfWeek = updateTractorScheduleDto.DayOfWeek.Value;
        if (updateTractorScheduleDto.ShiftStartTime.HasValue)
            tractorSchedule.ShiftStartTime = updateTractorScheduleDto.ShiftStartTime.Value;
        if (updateTractorScheduleDto.ShiftEndTime.HasValue)
            tractorSchedule.ShiftEndTime = updateTractorScheduleDto.ShiftEndTime.Value;
        if (updateTractorScheduleDto.StartDepotId.HasValue)
        {
            tractorSchedule.StartDepotId = updateTractorScheduleDto.StartDepotId.Value;
            tractorSchedule.StartParkingId = null;
        }
        if (updateTractorScheduleDto.StartParkingId.HasValue)
        {
            tractorSchedule.StartParkingId = updateTractorScheduleDto.StartParkingId.Value;
            tractorSchedule.StartDepotId = null;
        }
        if (updateTractorScheduleDto.EndDepotId.HasValue)
        {
            tractorSchedule.EndDepotId = updateTractorScheduleDto.EndDepotId.Value;
            tractorSchedule.EndParkingId = null;
        }
        if (updateTractorScheduleDto.EndParkingId.HasValue)
        {
            tractorSchedule.EndParkingId = updateTractorScheduleDto.EndParkingId.Value;
            tractorSchedule.EndDepotId = null;
        }
        if (updateTractorScheduleDto.IsOvertime.HasValue)
            tractorSchedule.IsOvertime = updateTractorScheduleDto.IsOvertime.Value;
        if (updateTractorScheduleDto.Active.HasValue)
            tractorSchedule.Active = updateTractorScheduleDto.Active.Value;

        tractorSchedule.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return await GetByIdAsync(id);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var tractorSchedule = await _context.TractorSchedules.FindAsync(id);
        if (tractorSchedule == null || tractorSchedule.DeletedAt != null)
            return false;

        // Soft delete
        tractorSchedule.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.TractorSchedules.AnyAsync(ts => ts.Id == id && ts.DeletedAt == null);
    }

    public async Task<IEnumerable<TractorScheduleListItemDto>> GetByTractorIdAsync(int tractorId)
    {
        return await _context.TractorSchedules
            .Include(ts => ts.Tractor)
            .Include(ts => ts.Driver)
            .Include(ts => ts.StartDepot)
            .Include(ts => ts.StartParking)
            .Include(ts => ts.EndDepot)
            .Include(ts => ts.EndParking)
            .Where(ts => ts.TractorId == tractorId && ts.DeletedAt == null)
            .Select(ts => new TractorScheduleListItemDto
            {
                Id = ts.Id,
                TractorId = ts.TractorId,
                TractorName = ts.Tractor.TractorName,
                TractorCode = ts.Tractor.TractorCode,
                DriverId = ts.DriverId,
                DriverName = ts.Driver != null ? $"{ts.Driver.FirstName} {ts.Driver.LastName}" : null,
                DayOfWeek = ts.DayOfWeek,
                ShiftStartTime = ts.ShiftStartTime,
                ShiftEndTime = ts.ShiftEndTime,
                StartLocationName = ts.StartDepot != null ? ts.StartDepot.DepotName : ts.StartParking!.ParkingName,
                EndLocationName = ts.EndDepot != null ? ts.EndDepot.DepotName : ts.EndParking!.ParkingName,
                IsOvertime = ts.IsOvertime,
                Active = ts.Active,
                CreatedAt = ts.CreatedAt,
                UpdatedAt = ts.UpdatedAt
            })
            .OrderBy(ts => ts.DayOfWeek)
            .ThenBy(ts => ts.ShiftStartTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<TractorScheduleListItemDto>> GetByDriverIdAsync(int driverId)
    {
        return await _context.TractorSchedules
            .Include(ts => ts.Tractor)
            .Include(ts => ts.Driver)
            .Include(ts => ts.StartDepot)
            .Include(ts => ts.StartParking)
            .Include(ts => ts.EndDepot)
            .Include(ts => ts.EndParking)
            .Where(ts => ts.DriverId == driverId && ts.DeletedAt == null)
            .Select(ts => new TractorScheduleListItemDto
            {
                Id = ts.Id,
                TractorId = ts.TractorId,
                TractorName = ts.Tractor.TractorName,
                TractorCode = ts.Tractor.TractorCode,
                DriverId = ts.DriverId,
                DriverName = ts.Driver != null ? $"{ts.Driver.FirstName} {ts.Driver.LastName}" : null,
                DayOfWeek = ts.DayOfWeek,
                ShiftStartTime = ts.ShiftStartTime,
                ShiftEndTime = ts.ShiftEndTime,
                StartLocationName = ts.StartDepot != null ? ts.StartDepot.DepotName : ts.StartParking!.ParkingName,
                EndLocationName = ts.EndDepot != null ? ts.EndDepot.DepotName : ts.EndParking!.ParkingName,
                IsOvertime = ts.IsOvertime,
                Active = ts.Active,
                CreatedAt = ts.CreatedAt,
                UpdatedAt = ts.UpdatedAt
            })
            .OrderBy(ts => ts.DayOfWeek)
            .ThenBy(ts => ts.ShiftStartTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<TractorScheduleListItemDto>> GetByDayOfWeekAsync(int dayOfWeek)
    {
        return await _context.TractorSchedules
            .Include(ts => ts.Tractor)
            .Include(ts => ts.Driver)
            .Include(ts => ts.StartDepot)
            .Include(ts => ts.StartParking)
            .Include(ts => ts.EndDepot)
            .Include(ts => ts.EndParking)
            .Where(ts => ts.DayOfWeek == dayOfWeek && ts.DeletedAt == null)
            .Select(ts => new TractorScheduleListItemDto
            {
                Id = ts.Id,
                TractorId = ts.TractorId,
                TractorName = ts.Tractor.TractorName,
                TractorCode = ts.Tractor.TractorCode,
                DriverId = ts.DriverId,
                DriverName = ts.Driver != null ? $"{ts.Driver.FirstName} {ts.Driver.LastName}" : null,
                DayOfWeek = ts.DayOfWeek,
                ShiftStartTime = ts.ShiftStartTime,
                ShiftEndTime = ts.ShiftEndTime,
                StartLocationName = ts.StartDepot != null ? ts.StartDepot.DepotName : ts.StartParking!.ParkingName,
                EndLocationName = ts.EndDepot != null ? ts.EndDepot.DepotName : ts.EndParking!.ParkingName,
                IsOvertime = ts.IsOvertime,
                Active = ts.Active,
                CreatedAt = ts.CreatedAt,
                UpdatedAt = ts.UpdatedAt
            })
            .OrderBy(ts => ts.ShiftStartTime)
            .ThenBy(ts => ts.TractorName)
            .ToListAsync();
    }

    public async Task<IEnumerable<TractorScheduleListItemDto>> GetByDepotIdAsync(int depotId)
    {
        return await _context.TractorSchedules
            .Include(ts => ts.Tractor)
            .Include(ts => ts.Driver)
            .Include(ts => ts.StartDepot)
            .Include(ts => ts.StartParking)
            .Include(ts => ts.EndDepot)
            .Include(ts => ts.EndParking)
            .Where(ts => (ts.StartDepotId == depotId || ts.EndDepotId == depotId) && ts.DeletedAt == null)
            .Select(ts => new TractorScheduleListItemDto
            {
                Id = ts.Id,
                TractorId = ts.TractorId,
                TractorName = ts.Tractor.TractorName,
                TractorCode = ts.Tractor.TractorCode,
                DriverId = ts.DriverId,
                DriverName = ts.Driver != null ? $"{ts.Driver.FirstName} {ts.Driver.LastName}" : null,
                DayOfWeek = ts.DayOfWeek,
                ShiftStartTime = ts.ShiftStartTime,
                ShiftEndTime = ts.ShiftEndTime,
                StartLocationName = ts.StartDepot != null ? ts.StartDepot.DepotName : ts.StartParking!.ParkingName,
                EndLocationName = ts.EndDepot != null ? ts.EndDepot.DepotName : ts.EndParking!.ParkingName,
                IsOvertime = ts.IsOvertime,
                Active = ts.Active,
                CreatedAt = ts.CreatedAt,
                UpdatedAt = ts.UpdatedAt
            })
            .OrderBy(ts => ts.DayOfWeek)
            .ThenBy(ts => ts.ShiftStartTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<TractorScheduleListItemDto>> GetByParkingIdAsync(int parkingId)
    {
        return await _context.TractorSchedules
            .Include(ts => ts.Tractor)
            .Include(ts => ts.Driver)
            .Include(ts => ts.StartDepot)
            .Include(ts => ts.StartParking)
            .Include(ts => ts.EndDepot)
            .Include(ts => ts.EndParking)
            .Where(ts => (ts.StartParkingId == parkingId || ts.EndParkingId == parkingId) && ts.DeletedAt == null)
            .Select(ts => new TractorScheduleListItemDto
            {
                Id = ts.Id,
                TractorId = ts.TractorId,
                TractorName = ts.Tractor.TractorName,
                TractorCode = ts.Tractor.TractorCode,
                DriverId = ts.DriverId,
                DriverName = ts.Driver != null ? $"{ts.Driver.FirstName} {ts.Driver.LastName}" : null,
                DayOfWeek = ts.DayOfWeek,
                ShiftStartTime = ts.ShiftStartTime,
                ShiftEndTime = ts.ShiftEndTime,
                StartLocationName = ts.StartDepot != null ? ts.StartDepot.DepotName : ts.StartParking!.ParkingName,
                EndLocationName = ts.EndDepot != null ? ts.EndDepot.DepotName : ts.EndParking!.ParkingName,
                IsOvertime = ts.IsOvertime,
                Active = ts.Active,
                CreatedAt = ts.CreatedAt,
                UpdatedAt = ts.UpdatedAt
            })
            .OrderBy(ts => ts.DayOfWeek)
            .ThenBy(ts => ts.ShiftStartTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<TractorScheduleListItemDto>> GetSchedulesForDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        var startDayOfWeek = (int)startDate.DayOfWeek;
        var endDayOfWeek = (int)endDate.DayOfWeek;

        return await _context.TractorSchedules
            .Include(ts => ts.Tractor)
            .Include(ts => ts.Driver)
            .Include(ts => ts.StartDepot)
            .Include(ts => ts.StartParking)
            .Include(ts => ts.EndDepot)
            .Include(ts => ts.EndParking)
            .Where(ts => ts.DayOfWeek >= startDayOfWeek && ts.DayOfWeek <= endDayOfWeek && ts.DeletedAt == null)
            .Select(ts => new TractorScheduleListItemDto
            {
                Id = ts.Id,
                TractorId = ts.TractorId,
                TractorName = ts.Tractor.TractorName,
                TractorCode = ts.Tractor.TractorCode,
                DriverId = ts.DriverId,
                DriverName = ts.Driver != null ? $"{ts.Driver.FirstName} {ts.Driver.LastName}" : null,
                DayOfWeek = ts.DayOfWeek,
                ShiftStartTime = ts.ShiftStartTime,
                ShiftEndTime = ts.ShiftEndTime,
                StartLocationName = ts.StartDepot != null ? ts.StartDepot.DepotName : ts.StartParking!.ParkingName,
                EndLocationName = ts.EndDepot != null ? ts.EndDepot.DepotName : ts.EndParking!.ParkingName,
                IsOvertime = ts.IsOvertime,
                Active = ts.Active,
                CreatedAt = ts.CreatedAt,
                UpdatedAt = ts.UpdatedAt
            })
            .OrderBy(ts => ts.DayOfWeek)
            .ThenBy(ts => ts.ShiftStartTime)
            .ToListAsync();
    }

    public async Task<bool> HasScheduleConflictAsync(int tractorId, int dayOfWeek, TimeSpan startTime, TimeSpan endTime, int? excludeId = null)
    {
        var query = _context.TractorSchedules
            .Where(ts => ts.TractorId == tractorId && 
                        ts.DayOfWeek == dayOfWeek && 
                        ts.DeletedAt == null &&
                        ((startTime >= ts.ShiftStartTime && startTime < ts.ShiftEndTime) ||
                         (endTime > ts.ShiftStartTime && endTime <= ts.ShiftEndTime) ||
                         (startTime <= ts.ShiftStartTime && endTime >= ts.ShiftEndTime)));

        if (excludeId.HasValue)
            query = query.Where(ts => ts.Id != excludeId.Value);

        return await query.AnyAsync();
    }

    public async Task<bool> HasDriverConflictAsync(int driverId, int dayOfWeek, TimeSpan startTime, TimeSpan endTime, int? excludeId = null)
    {
        var query = _context.TractorSchedules
            .Where(ts => ts.DriverId == driverId && 
                        ts.DayOfWeek == dayOfWeek && 
                        ts.DeletedAt == null &&
                        ((startTime >= ts.ShiftStartTime && startTime < ts.ShiftEndTime) ||
                         (endTime > ts.ShiftStartTime && endTime <= ts.ShiftEndTime) ||
                         (startTime <= ts.ShiftStartTime && endTime >= ts.ShiftEndTime)));

        if (excludeId.HasValue)
            query = query.Where(ts => ts.Id != excludeId.Value);

        return await query.AnyAsync();
    }

    public async Task<IEnumerable<TractorScheduleListItemDto>> GetAvailableSchedulesAsync(int dayOfWeek, TimeSpan startTime, TimeSpan endTime)
    {
        return await _context.TractorSchedules
            .Include(ts => ts.Tractor)
            .Include(ts => ts.Driver)
            .Include(ts => ts.StartDepot)
            .Include(ts => ts.StartParking)
            .Include(ts => ts.EndDepot)
            .Include(ts => ts.EndParking)
            .Where(ts => ts.DayOfWeek == dayOfWeek && 
                        ts.Active && 
                        ts.DeletedAt == null &&
                        !(ts.ShiftStartTime < endTime && ts.ShiftEndTime > startTime))
            .Select(ts => new TractorScheduleListItemDto
            {
                Id = ts.Id,
                TractorId = ts.TractorId,
                TractorName = ts.Tractor.TractorName,
                TractorCode = ts.Tractor.TractorCode,
                DriverId = ts.DriverId,
                DriverName = ts.Driver != null ? $"{ts.Driver.FirstName} {ts.Driver.LastName}" : null,
                DayOfWeek = ts.DayOfWeek,
                ShiftStartTime = ts.ShiftStartTime,
                ShiftEndTime = ts.ShiftEndTime,
                StartLocationName = ts.StartDepot != null ? ts.StartDepot.DepotName : ts.StartParking!.ParkingName,
                EndLocationName = ts.EndDepot != null ? ts.EndDepot.DepotName : ts.EndParking!.ParkingName,
                IsOvertime = ts.IsOvertime,
                Active = ts.Active,
                CreatedAt = ts.CreatedAt,
                UpdatedAt = ts.UpdatedAt
            })
            .OrderBy(ts => ts.ShiftStartTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<DriverListItemDto>> GetAvailableDriversForScheduleAsync(int dayOfWeek, TimeSpan startTime, TimeSpan endTime)
    {
        var conflictingDriverIds = await _context.TractorSchedules
            .Where(ts => ts.DayOfWeek == dayOfWeek && 
                        ts.DeletedAt == null &&
                        ts.DriverId.HasValue &&
                        ((startTime >= ts.ShiftStartTime && startTime < ts.ShiftEndTime) ||
                         (endTime > ts.ShiftStartTime && endTime <= ts.ShiftEndTime) ||
                         (startTime <= ts.ShiftStartTime && endTime >= ts.ShiftEndTime)))
            .Select(ts => ts.DriverId!.Value)
            .ToListAsync();

        return await _context.Drivers
            .Include(d => d.Company)
            .Where(d => d.Active && 
                       d.DeletedAt == null &&
                       !conflictingDriverIds.Contains(d.Id))
            .Select(d => new DriverListItemDto
            {
                Id = d.Id,
                DriverCode = d.DriverCode,
                FirstName = d.FirstName,
                LastName = d.LastName,
                CompanyId = d.CompanyId,
                CompanyName = d.Company.Name,
                LicenseNumber = d.LicenseNumber,
                LicenseExpiry = d.LicenseExpiry,
                HazmatCertified = d.HazmatCertified,
                Active = d.Active,
                Status = d.Status,
                MobileNumber = d.MobileNumber,
                Email = d.Email,
                CreatedAt = d.CreatedAt,
                UpdatedAt = d.UpdatedAt
            })
            .OrderBy(d => d.FirstName)
            .ThenBy(d => d.LastName)
            .ToListAsync();
    }

    public async Task<IEnumerable<TractorListItemDto>> GetAvailableTractorsForScheduleAsync(int dayOfWeek, TimeSpan startTime, TimeSpan endTime)
    {
        var conflictingTractorIds = await _context.TractorSchedules
            .Where(ts => ts.DayOfWeek == dayOfWeek && 
                        ts.DeletedAt == null &&
                        ((startTime >= ts.ShiftStartTime && startTime < ts.ShiftEndTime) ||
                         (endTime > ts.ShiftStartTime && endTime <= ts.ShiftEndTime) ||
                         (startTime <= ts.ShiftStartTime && endTime >= ts.ShiftEndTime)))
            .Select(ts => ts.TractorId)
            .ToListAsync();

        return await _context.Tractors
            .Include(t => t.Haulier)
            .Where(t => t.Status == "Active" && 
                       t.DeletedAt == null &&
                       !conflictingTractorIds.Contains(t.Id))
            .Select(t => new TractorListItemDto
            {
                Id = t.Id,
                TractorCode = t.TractorCode,
                TractorName = t.TractorName,
                LicensePlate = t.LicensePlate,
                HaulierId = t.HaulierId,
                HaulierName = t.Haulier.HaulierName,
                Status = t.Status,
                PumpAvailable = t.PumpAvailable,
                PumpFlowRateLpm = t.PumpFlowRateLpm,
                CurbWeightKg = t.CurbWeightKg,
                NumberOfAxles = t.NumberOfAxles,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt
            })
            .OrderBy(t => t.TractorName)
            .ToListAsync();
    }

    private void ValidateLocationConstraints(int? startDepotId, int? startParkingId, int? endDepotId, int? endParkingId)
    {
        // Start location: Must have either depot OR parking, but not both
        if ((startDepotId.HasValue && startParkingId.HasValue) || (!startDepotId.HasValue && !startParkingId.HasValue))
        {
            throw new ArgumentException("Must specify either start depot or start parking, but not both");
        }

        // End location: Must have either depot OR parking, but not both
        if ((endDepotId.HasValue && endParkingId.HasValue) || (!endDepotId.HasValue && !endParkingId.HasValue))
        {
            throw new ArgumentException("Must specify either end depot or end parking, but not both");
        }
    }
}