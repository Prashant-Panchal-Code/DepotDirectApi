using DepotDirectApi.Data;
using DepotDirectApi.Models.DTOs;
using DepotDirectApi.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace DepotDirectApi.Repositories;

public class NoteRepository : INoteRepository
{
    private readonly DepotDirectDbContext _context;

    public NoteRepository(DepotDirectDbContext context)
    {
        _context = context;
    }

    public async Task<NoteDto> CreateAsync(CreateNoteDto createNoteDto, int? createdBy = null)
    {
        // Basic validation: ensure exactly one target is set
        var targets = new[] { createNoteDto.SiteId.HasValue, createNoteDto.DepotId.HasValue, createNoteDto.ParkingId.HasValue, createNoteDto.VehicleId.HasValue };
        if (targets.Count(t => t) != 1)
            throw new ArgumentException("Exactly one target must be set: siteId, depotId, parkingId, or vehicleId.");

        var note = new Note
        {
            Category = createNoteDto.Category,
            Priority = createNoteDto.Priority,
            Comment = createNoteDto.Comment,
            Status = "Open",
            SiteId = createNoteDto.SiteId,
            DepotId = createNoteDto.DepotId,
            ParkingId = createNoteDto.ParkingId,
            VehicleId = createNoteDto.VehicleId,
            CompanyId = createNoteDto.CompanyId,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Notes.Add(note);
        await _context.SaveChangesAsync();

        // Resolve createdBy name (closedBy is null on creation)
        string? createdByName = null;
        if (note.CreatedBy.HasValue)
        {
            var user = await _context.Users
                .Where(u => u.Id == note.CreatedBy.Value)
                .Select(u => new { u.FullName, u.Email })
                .FirstOrDefaultAsync();

            if (user != null)
                createdByName = !string.IsNullOrWhiteSpace(user.FullName) ? user.FullName : user.Email;
        }

        return new NoteDto
        {
            Id = note.Id,
            Category = note.Category,
            Priority = note.Priority,
            Comment = note.Comment,
            Status = note.Status,
            ClosingComment = note.ClosingComment,
            ClosedAt = note.ClosedAt,
            ClosedBy = note.ClosedBy,
            SiteId = note.SiteId,
            DepotId = note.DepotId,
            ParkingId = note.ParkingId,
            VehicleId = note.VehicleId,
            CompanyId = note.CompanyId,
            CreatedBy = note.CreatedBy,
            CreatedByName = createdByName,
            ClosedByName = null,
            CreatedAt = note.CreatedAt,
            UpdatedAt = note.UpdatedAt,
            DeletedAt = note.DeletedAt
        };
    }

    public async Task<NoteDto?> GetByIdAsync(int id)
    {
        var n = await _context.Notes.Where(x => x.Id == id && x.DeletedAt == null).FirstOrDefaultAsync();
        if (n == null)
            return null;

        string? createdByName = null;
        string? closedByName = null;

        if (n.CreatedBy.HasValue)
        {
            var u = await _context.Users.Where(u => u.Id == n.CreatedBy.Value).FirstOrDefaultAsync();
            createdByName = u?.FullName ?? u?.Email;
        }

        if (n.ClosedBy.HasValue)
        {
            var u2 = await _context.Users.Where(u => u.Id == n.ClosedBy.Value).FirstOrDefaultAsync();
            closedByName = u2?.FullName ?? u2?.Email;
        }

        return new NoteDto
        {
            Id = n.Id,
            Category = n.Category,
            Priority = n.Priority,
            Comment = n.Comment,
            Status = n.Status,
            ClosingComment = n.ClosingComment,
            ClosedAt = n.ClosedAt,
            ClosedBy = n.ClosedBy,
            SiteId = n.SiteId,
            DepotId = n.DepotId,
            ParkingId = n.ParkingId,
            VehicleId = n.VehicleId,
            CompanyId = n.CompanyId,
            CreatedBy = n.CreatedBy,
            CreatedByName = createdByName,
            ClosedByName = closedByName,
            CreatedAt = n.CreatedAt,
            UpdatedAt = n.UpdatedAt,
            DeletedAt = n.DeletedAt
        };
    }

    public async Task<IEnumerable<NoteDto>> GetByTargetAsync(int? siteId, int? depotId, int? parkingId, int? vehicleId)
    {
        // Use HasValue checks only (do not treat zero as special)
        var actualSite = siteId.HasValue ? siteId.Value : (int?)null;
        var actualDepot = depotId.HasValue ? depotId.Value : (int?)null;
        var actualParking = parkingId.HasValue ? parkingId.Value : (int?)null;
        var actualVehicle = vehicleId.HasValue ? vehicleId.Value : (int?)null;

        var query = _context.Notes.Where(n => n.DeletedAt == null);

        if (actualSite.HasValue)
            query = query.Where(n => n.SiteId == actualSite.Value);
        else if (actualDepot.HasValue)
            query = query.Where(n => n.DepotId == actualDepot.Value);
        else if (actualParking.HasValue)
            query = query.Where(n => n.ParkingId == actualParking.Value);
        else if (actualVehicle.HasValue)
            query = query.Where(n => n.VehicleId == actualVehicle.Value);
        else
            return Enumerable.Empty<NoteDto>();

        var list = await query.OrderByDescending(n => n.CreatedAt).ToListAsync();

        var userIds = list.SelectMany(n => new[] { n.CreatedBy, n.ClosedBy }).Where(id => id.HasValue).Select(id => id!.Value).Distinct().ToList();
        var users = await _context.Users.Where(u => userIds.Contains(u.Id)).ToDictionaryAsync(u => u.Id, u => u);

        return list.Select(n => new NoteDto
        {
            Id = n.Id,
            Category = n.Category,
            Priority = n.Priority,
            Comment = n.Comment,
            Status = n.Status,
            ClosingComment = n.ClosingComment,
            ClosedAt = n.ClosedAt,
            ClosedBy = n.ClosedBy,
            SiteId = n.SiteId,
            DepotId = n.DepotId,
            ParkingId = n.ParkingId,
            VehicleId = n.VehicleId,
            CompanyId = n.CompanyId,
            CreatedBy = n.CreatedBy,
            CreatedByName = n.CreatedBy.HasValue && users.ContainsKey(n.CreatedBy.Value) ? (users[n.CreatedBy.Value].FullName ?? users[n.CreatedBy.Value].Email) : null,
            ClosedByName = n.ClosedBy.HasValue && users.ContainsKey(n.ClosedBy.Value) ? (users[n.ClosedBy.Value].FullName ?? users[n.ClosedBy.Value].Email) : null,
            CreatedAt = n.CreatedAt,
            UpdatedAt = n.UpdatedAt,
            DeletedAt = n.DeletedAt
        }).ToList();
    }

    public async Task<IEnumerable<NoteDto>> GetByCompanyAsync(int companyId)
    {
        var list = await _context.Notes.Where(n => n.CompanyId == companyId && n.DeletedAt == null).OrderByDescending(n => n.CreatedAt).ToListAsync();

        var userIds = list.SelectMany(n => new[] { n.CreatedBy, n.ClosedBy }).Where(id => id.HasValue).Select(id => id!.Value).Distinct().ToList();
        var users = await _context.Users.Where(u => userIds.Contains(u.Id)).ToDictionaryAsync(u => u.Id, u => u);

        return list.Select(n => new NoteDto
        {
            Id = n.Id,
            Category = n.Category,
            Priority = n.Priority,
            Comment = n.Comment,
            Status = n.Status,
            ClosingComment = n.ClosingComment,
            ClosedAt = n.ClosedAt,
            ClosedBy = n.ClosedBy,
            SiteId = n.SiteId,
            DepotId = n.DepotId,
            ParkingId = n.ParkingId,
            VehicleId = n.VehicleId,
            CompanyId = n.CompanyId,
            CreatedBy = n.CreatedBy,
            CreatedByName = n.CreatedBy.HasValue && users.ContainsKey(n.CreatedBy.Value) ? (users[n.CreatedBy.Value].FullName ?? users[n.CreatedBy.Value].Email) : null,
            ClosedByName = n.ClosedBy.HasValue && users.ContainsKey(n.ClosedBy.Value) ? (users[n.ClosedBy.Value].FullName ?? users[n.ClosedBy.Value].Email) : null,
            CreatedAt = n.CreatedAt,
            UpdatedAt = n.UpdatedAt,
            DeletedAt = n.DeletedAt
        }).ToList();
    }

    public async Task<NoteDto?> UpdateStatusAsync(int id, UpdateNoteStatusDto updateDto, int? updatedBy = null)
    {
        var n = await _context.Notes.Where(x => x.Id == id && x.DeletedAt == null).FirstOrDefaultAsync();
        if (n == null)
            return null;

        n.Status = updateDto.Status;
        n.UpdatedAt = DateTime.UtcNow;
        n.ClosedBy = null;
        n.ClosedAt = null;
        n.ClosingComment = null;

        if (updateDto.Status == "Closed")
        {
            n.ClosedAt = DateTime.UtcNow;
            n.ClosedBy = updatedBy;
            n.ClosingComment = updateDto.ClosingComment;
        }

        await _context.SaveChangesAsync();

        return await GetByIdAsync(id);
    }
}
