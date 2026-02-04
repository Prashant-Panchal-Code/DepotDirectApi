using DepotDirectApi.Data;
using DepotDirectApi.Models.DTOs;
using DepotDirectApi.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace DepotDirectApi.Repositories;

public class TrailerRepository : ITrailerRepository
{
    private readonly DepotDirectDbContext _context;
    private readonly ILogger<TrailerRepository> _logger;

    public TrailerRepository(DepotDirectDbContext context, ILogger<TrailerRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<TrailerListItemDto>> GetAllAsync()
    {
        return await _context.Trailers
            .Include(t => t.Haulier)
            .ThenInclude(h => h.Region)
            .Include(t => t.Region)
            .Where(t => t.DeletedAt == null)
            .Select(t => new TrailerListItemDto
            {
                Id = t.Id,
                TrailerCode = t.TrailerCode,
                TrailerName = t.TrailerName,
                LicensePlate = t.LicensePlate,
                HaulierId = t.HaulierId,
                HaulierName = t.Haulier.HaulierName,
                RegionId = t.RegionId,
                RegionName = t.Region != null ? t.Region.Name : null,
                UnladenWeightKg = t.UnladenWeightKg,
                MaxPayloadKg = t.MaxPayloadKg,
                MaxPayloadLiters = t.MaxPayloadLiters,
                NumberOfAxles = t.NumberOfAxles,
                Status = t.Status,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt
            })
            .OrderBy(t => t.HaulierName)
            .ThenBy(t => t.TrailerName)
            .ToListAsync();
    }

    public async Task<TrailerResponseDto?> GetByIdAsync(int id)
    {
        return await _context.Trailers
            .Include(t => t.Haulier)
            .ThenInclude(h => h.Region)
            .Include(t => t.Region)
            .Include(t => t.TrailerCompartments)
            .Where(t => t.Id == id && t.DeletedAt == null)
            .Select(t => new TrailerResponseDto
            {
                Id = t.Id,
                TrailerCode = t.TrailerCode,
                TrailerName = t.TrailerName,
                LicensePlate = t.LicensePlate,
                HaulierId = t.HaulierId,
                RegionId = t.RegionId,
                UnladenWeightKg = t.UnladenWeightKg,
                MaxPayloadKg = t.MaxPayloadKg,
                MaxPayloadLiters = t.MaxPayloadLiters,
                NumberOfAxles = t.NumberOfAxles,
                Status = t.Status,
                AxleConfiguration = t.AxleConfiguration,
                Metadata = t.Metadata,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt,
                DeletedAt = t.DeletedAt,
                Haulier = new HaulierListItemDto
                {
                    Id = t.Haulier.Id,
                    RegionId = t.Haulier.RegionId,
                    RegionName = t.Haulier.Region.Name,
                    HaulierCode = t.Haulier.HaulierCode,
                    HaulierName = t.Haulier.HaulierName,
                    TaxId = t.Haulier.TaxId,
                    ContractNumber = t.Haulier.ContractNumber,
                    ContractExpiry = t.Haulier.ContractExpiry,
                    ContactName = t.Haulier.ContactName,
                    ContactEmail = t.Haulier.ContactEmail,
                    ContactPhone = t.Haulier.ContactPhone,
                    Active = t.Haulier.Active,
                    CreatedAt = t.Haulier.CreatedAt,
                    UpdatedAt = t.Haulier.UpdatedAt
                },
                Region = t.Region != null ? new RegionDto
                {
                    Id = t.Region.Id,
                    Name = t.Region.Name,
                    CompanyId = t.Region.CompanyId,
                    CreatedAt = t.Region.CreatedAt,
                    UpdatedAt = t.Region.UpdatedAt
                } : null,
                TrailerCompartments = t.TrailerCompartments.Select(tc => new TrailerCompartmentResponseDto
                {
                    Id = tc.Id,
                    TrailerId = tc.TrailerId,
                    CompartmentNumber = tc.CompartmentNumber,
                    CapacityL = tc.CapacityL,
                    MinVolumeL = tc.MinVolumeL,
                    SafeFillL = tc.SafeFillL,
                    MustUse = tc.MustUse,
                    PartialLoadAllowed = tc.PartialLoadAllowed,
                    Metadata = tc.Metadata
                }).ToList()
            })
            .FirstOrDefaultAsync();
    }

    public async Task<TrailerResponseDto> CreateAsync(CreateTrailerDto createTrailerDto, int? createdBy = null)
    {
        // Check if trailer code already exists for this haulier
        if (await ExistsByTrailerCodeAndHaulierAsync(createTrailerDto.TrailerCode, createTrailerDto.HaulierId))
        {
            throw new ArgumentException($"Trailer with code '{createTrailerDto.TrailerCode}' already exists for this haulier");
        }

        // Check if license plate already exists
        if (await ExistsByLicensePlateAsync(createTrailerDto.LicensePlate))
        {
            throw new ArgumentException($"Trailer with license plate '{createTrailerDto.LicensePlate}' already exists");
        }

        // Verify haulier exists
        var haulierExists = await _context.Hauliers.AnyAsync(h => h.Id == createTrailerDto.HaulierId && h.DeletedAt == null);
        if (!haulierExists)
        {
            throw new ArgumentException($"Haulier with ID {createTrailerDto.HaulierId} not found");
        }

        // Verify region exists and belongs to the same region as the haulier
        if (createTrailerDto.RegionId.HasValue)
        {
            var haulier = await _context.Hauliers.FindAsync(createTrailerDto.HaulierId);
            if (haulier != null && createTrailerDto.RegionId.Value != haulier.RegionId)
            {
                throw new ArgumentException("Trailer region must match the haulier's region");
            }
        }

        var trailer = new Trailer
        {
            TrailerCode = createTrailerDto.TrailerCode,
            TrailerName = createTrailerDto.TrailerName,
            LicensePlate = createTrailerDto.LicensePlate,
            HaulierId = createTrailerDto.HaulierId,
            RegionId = createTrailerDto.RegionId,
            UnladenWeightKg = createTrailerDto.UnladenWeightKg,
            MaxPayloadKg = createTrailerDto.MaxPayloadKg,
            MaxPayloadLiters = createTrailerDto.MaxPayloadLiters,
            NumberOfAxles = createTrailerDto.NumberOfAxles,
            Status = createTrailerDto.Status ?? "Active",
            AxleConfiguration = createTrailerDto.AxleConfiguration,
            Metadata = createTrailerDto.Metadata,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Trailers.Add(trailer);
        await _context.SaveChangesAsync();

        return await GetByIdAsync(trailer.Id) ?? throw new InvalidOperationException("Failed to retrieve created trailer");
    }

    public async Task<TrailerResponseDto?> UpdateAsync(int id, UpdateTrailerDto updateTrailerDto, int? updatedBy = null)
    {
        var trailer = await _context.Trailers.Include(t => t.Haulier).FirstOrDefaultAsync(t => t.Id == id);
        if (trailer == null || trailer.DeletedAt != null)
            return null;

        // Check if trailer code already exists for this haulier (excluding current trailer)
        if (updateTrailerDto.TrailerCode != null && 
            await ExistsByTrailerCodeAndHaulierAsync(updateTrailerDto.TrailerCode, trailer.HaulierId, id))
        {
            throw new ArgumentException($"Trailer with code '{updateTrailerDto.TrailerCode}' already exists for this haulier");
        }

        // Check if license plate already exists (excluding current trailer)
        if (updateTrailerDto.LicensePlate != null && 
            await ExistsByLicensePlateAsync(updateTrailerDto.LicensePlate, id))
        {
            throw new ArgumentException($"Trailer with license plate '{updateTrailerDto.LicensePlate}' already exists");
        }

        // Verify region exists and belongs to the same region as the haulier
        if (updateTrailerDto.RegionId.HasValue)
        {
            if (updateTrailerDto.RegionId.Value != trailer.Haulier.RegionId)
            {
                throw new ArgumentException("Trailer region must match the haulier's region");
            }
        }

        // Update only provided fields
        if (updateTrailerDto.TrailerCode != null)
            trailer.TrailerCode = updateTrailerDto.TrailerCode;
        if (updateTrailerDto.TrailerName != null)
            trailer.TrailerName = updateTrailerDto.TrailerName;
        if (updateTrailerDto.LicensePlate != null)
            trailer.LicensePlate = updateTrailerDto.LicensePlate;
        if (updateTrailerDto.RegionId.HasValue)
            trailer.RegionId = updateTrailerDto.RegionId.Value;
        if (updateTrailerDto.UnladenWeightKg.HasValue)
            trailer.UnladenWeightKg = updateTrailerDto.UnladenWeightKg.Value;
        if (updateTrailerDto.MaxPayloadKg.HasValue)
            trailer.MaxPayloadKg = updateTrailerDto.MaxPayloadKg.Value;
        if (updateTrailerDto.MaxPayloadLiters.HasValue)
            trailer.MaxPayloadLiters = updateTrailerDto.MaxPayloadLiters.Value;
        if (updateTrailerDto.NumberOfAxles.HasValue)
            trailer.NumberOfAxles = updateTrailerDto.NumberOfAxles.Value;
        if (updateTrailerDto.Status != null)
            trailer.Status = updateTrailerDto.Status;
        if (updateTrailerDto.AxleConfiguration != null)
            trailer.AxleConfiguration = updateTrailerDto.AxleConfiguration;
        if (updateTrailerDto.Metadata != null)
            trailer.Metadata = updateTrailerDto.Metadata;

        trailer.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return await GetByIdAsync(id);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var trailer = await _context.Trailers.FindAsync(id);
        if (trailer == null || trailer.DeletedAt != null)
            return false;

        // Check if trailer is being used in any vehicle combinations
        var isBeingUsed = await _context.VehicleCombinationTrailers.AnyAsync(vct => vct.TrailerId == id);
        if (isBeingUsed)
        {
            throw new InvalidOperationException("Cannot delete trailer that is part of vehicle combinations");
        }

        // Soft delete
        trailer.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Trailers.AnyAsync(t => t.Id == id && t.DeletedAt == null);
    }

    public async Task<bool> ExistsByTrailerCodeAndHaulierAsync(string trailerCode, int haulierId, int? excludeId = null)
    {
        var query = _context.Trailers.Where(t => t.TrailerCode == trailerCode && t.HaulierId == haulierId && t.DeletedAt == null);
        
        if (excludeId.HasValue)
            query = query.Where(t => t.Id != excludeId.Value);

        return await query.AnyAsync();
    }

    public async Task<bool> ExistsByLicensePlateAsync(string licensePlate, int? excludeId = null)
    {
        var query = _context.Trailers.Where(t => t.LicensePlate == licensePlate && t.DeletedAt == null);
        
        if (excludeId.HasValue)
            query = query.Where(t => t.Id != excludeId.Value);

        return await query.AnyAsync();
    }

    public async Task<IEnumerable<TrailerListItemDto>> GetByHaulierIdAsync(int haulierId)
    {
        return await _context.Trailers
            .Include(t => t.Haulier)
            .ThenInclude(h => h.Region)
            .Include(t => t.Region)
            .Where(t => t.HaulierId == haulierId && t.DeletedAt == null)
            .Select(t => new TrailerListItemDto
            {
                Id = t.Id,
                TrailerCode = t.TrailerCode,
                TrailerName = t.TrailerName,
                LicensePlate = t.LicensePlate,
                HaulierId = t.HaulierId,
                HaulierName = t.Haulier.HaulierName,
                RegionId = t.RegionId,
                RegionName = t.Region != null ? t.Region.Name : null,
                UnladenWeightKg = t.UnladenWeightKg,
                MaxPayloadKg = t.MaxPayloadKg,
                MaxPayloadLiters = t.MaxPayloadLiters,
                NumberOfAxles = t.NumberOfAxles,
                Status = t.Status,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt
            })
            .OrderBy(t => t.TrailerName)
            .ToListAsync();
    }

    public async Task<IEnumerable<TrailerListItemDto>> GetByRegionIdAsync(int regionId)
    {
        return await _context.Trailers
            .Include(t => t.Haulier)
            .ThenInclude(h => h.Region)
            .Include(t => t.Region)
            .Where(t => t.RegionId == regionId && t.DeletedAt == null)
            .Select(t => new TrailerListItemDto
            {
                Id = t.Id,
                TrailerCode = t.TrailerCode,
                TrailerName = t.TrailerName,
                LicensePlate = t.LicensePlate,
                HaulierId = t.HaulierId,
                HaulierName = t.Haulier.HaulierName,
                RegionId = t.RegionId,
                RegionName = t.Region != null ? t.Region.Name : null,
                UnladenWeightKg = t.UnladenWeightKg,
                MaxPayloadKg = t.MaxPayloadKg,
                MaxPayloadLiters = t.MaxPayloadLiters,
                NumberOfAxles = t.NumberOfAxles,
                Status = t.Status,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt
            })
            .OrderBy(t => t.TrailerName)
            .ToListAsync();
    }

    public async Task<IEnumerable<TrailerListItemDto>> GetByStatusAsync(string status)
    {
        return await _context.Trailers
            .Include(t => t.Haulier)
            .ThenInclude(h => h.Region)
            .Include(t => t.Region)
            .Where(t => t.Status == status && t.DeletedAt == null)
            .Select(t => new TrailerListItemDto
            {
                Id = t.Id,
                TrailerCode = t.TrailerCode,
                TrailerName = t.TrailerName,
                LicensePlate = t.LicensePlate,
                HaulierId = t.HaulierId,
                HaulierName = t.Haulier.HaulierName,
                RegionId = t.RegionId,
                RegionName = t.Region != null ? t.Region.Name : null,
                UnladenWeightKg = t.UnladenWeightKg,
                MaxPayloadKg = t.MaxPayloadKg,
                MaxPayloadLiters = t.MaxPayloadLiters,
                NumberOfAxles = t.NumberOfAxles,
                Status = t.Status,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt
            })
            .OrderBy(t => t.HaulierName)
            .ThenBy(t => t.TrailerName)
            .ToListAsync();
    }

    public async Task<IEnumerable<TrailerListItemDto>> SearchAsync(string searchTerm)
    {
        var normalizedSearchTerm = searchTerm.ToLower();

        return await _context.Trailers
            .Include(t => t.Haulier)
            .ThenInclude(h => h.Region)
            .Include(t => t.Region)
            .Where(t => t.DeletedAt == null && 
                        (t.TrailerCode.ToLower().Contains(normalizedSearchTerm) ||
                         t.TrailerName.ToLower().Contains(normalizedSearchTerm) ||
                         t.LicensePlate.ToLower().Contains(normalizedSearchTerm) ||
                         t.Haulier.HaulierName.ToLower().Contains(normalizedSearchTerm)))
            .Select(t => new TrailerListItemDto
            {
                Id = t.Id,
                TrailerCode = t.TrailerCode,
                TrailerName = t.TrailerName,
                LicensePlate = t.LicensePlate,
                HaulierId = t.HaulierId,
                HaulierName = t.Haulier.HaulierName,
                RegionId = t.RegionId,
                RegionName = t.Region != null ? t.Region.Name : null,
                UnladenWeightKg = t.UnladenWeightKg,
                MaxPayloadKg = t.MaxPayloadKg,
                MaxPayloadLiters = t.MaxPayloadLiters,
                NumberOfAxles = t.NumberOfAxles,
                Status = t.Status,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt
            })
            .OrderBy(t => t.HaulierName)
            .ThenBy(t => t.TrailerName)
            .ToListAsync();
    }

    public async Task<IEnumerable<TrailerListItemDto>> GetAvailableTrailersAsync(DateTime startDate, DateTime endDate)
    {
        // This is a simplified implementation - in a real system you'd check for conflicts with trips, maintenance, etc.
        return await _context.Trailers
            .Include(t => t.Haulier)
            .ThenInclude(h => h.Region)
            .Include(t => t.Region)
            .Where(t => t.Status == "Active" && t.DeletedAt == null)
            .Select(t => new TrailerListItemDto
            {
                Id = t.Id,
                TrailerCode = t.TrailerCode,
                TrailerName = t.TrailerName,
                LicensePlate = t.LicensePlate,
                HaulierId = t.HaulierId,
                HaulierName = t.Haulier.HaulierName,
                RegionId = t.RegionId,
                RegionName = t.Region != null ? t.Region.Name : null,
                UnladenWeightKg = t.UnladenWeightKg,
                MaxPayloadKg = t.MaxPayloadKg,
                MaxPayloadLiters = t.MaxPayloadLiters,
                NumberOfAxles = t.NumberOfAxles,
                Status = t.Status,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt
            })
            .OrderBy(t => t.TrailerName)
            .ToListAsync();
    }

    public async Task<bool> IsTrailerAvailableAsync(int trailerId, DateTime startDate, DateTime endDate)
    {
        // This is a simplified implementation - in a real system you'd check for conflicts with trips, maintenance, etc.
        var trailer = await _context.Trailers
            .Where(t => t.Id == trailerId && t.Status == "Active" && t.DeletedAt == null)
            .FirstOrDefaultAsync();

        return trailer != null;
    }
}