using DepotDirectApi.Data;
using DepotDirectApi.Models.DTOs;
using DepotDirectApi.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace DepotDirectApi.Repositories;

public class VehicleCombinationRepository : IVehicleCombinationRepository
{
    private readonly DepotDirectDbContext _context;
    private readonly ILogger<VehicleCombinationRepository> _logger;

    public VehicleCombinationRepository(DepotDirectDbContext context, ILogger<VehicleCombinationRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<VehicleCombinationListItemDto>> GetAllAsync()
    {
        return await _context.VehicleCombinations
            .Include(vc => vc.Tractor)
            .Include(vc => vc.VehicleCombinationTrailers)
            .Where(vc => vc.DeletedAt == null)
            .Select(vc => new VehicleCombinationListItemDto
            {
                Id = vc.Id,
                CombinationCode = vc.CombinationCode,
                TractorId = vc.TractorId,
                TractorName = vc.Tractor.TractorName,
                TractorCode = vc.Tractor.TractorCode,
                GrossWeightLimitKg = vc.GrossWeightLimitKg,
                TotalCapacityL = vc.TotalCapacityL,
                Active = vc.Active,
                IsDefault = vc.IsDefault,
                TrailerCount = vc.VehicleCombinationTrailers.Count(),
                CreatedAt = vc.CreatedAt,
                UpdatedAt = vc.UpdatedAt
            })
            .OrderBy(vc => vc.TractorName)
            .ThenBy(vc => vc.CombinationCode)
            .ToListAsync();
    }

    public async Task<VehicleCombinationResponseDto?> GetByIdAsync(int id)
    {
        return await _context.VehicleCombinations
            .Include(vc => vc.Tractor)
                .ThenInclude(t => t.Haulier)
            .Include(vc => vc.VehicleCombinationTrailers)
                .ThenInclude(vct => vct.Trailer)
                    .ThenInclude(t => t.Haulier)
            .Where(vc => vc.Id == id && vc.DeletedAt == null)
            .Select(vc => new VehicleCombinationResponseDto
            {
                Id = vc.Id,
                CombinationCode = vc.CombinationCode,
                TractorId = vc.TractorId,
                GrossWeightLimitKg = vc.GrossWeightLimitKg,
                TotalCapacityL = vc.TotalCapacityL,
                Active = vc.Active,
                IsDefault = vc.IsDefault,
                CreatedAt = vc.CreatedAt,
                UpdatedAt = vc.UpdatedAt,
                DeletedAt = vc.DeletedAt,
                Tractor = new TractorListItemDto
                {
                    Id = vc.Tractor.Id,
                    TractorCode = vc.Tractor.TractorCode,
                    TractorName = vc.Tractor.TractorName,
                    LicensePlate = vc.Tractor.LicensePlate,
                    HaulierId = vc.Tractor.HaulierId,
                    HaulierName = vc.Tractor.Haulier.HaulierName,
                    Status = vc.Tractor.Status,
                    PumpAvailable = vc.Tractor.PumpAvailable,
                    PumpFlowRateLpm = vc.Tractor.PumpFlowRateLpm,
                    CurbWeightKg = vc.Tractor.CurbWeightKg,
                    NumberOfAxles = vc.Tractor.NumberOfAxles,
                    CreatedAt = vc.Tractor.CreatedAt,
                    UpdatedAt = vc.Tractor.UpdatedAt
                },
                Trailers = vc.VehicleCombinationTrailers.OrderBy(vct => vct.SequenceNumber).Select(vct => new TrailerListItemDto
                {
                    Id = vct.Trailer.Id,
                    TrailerCode = vct.Trailer.TrailerCode,
                    TrailerName = vct.Trailer.TrailerName,
                    LicensePlate = vct.Trailer.LicensePlate,
                    HaulierId = vct.Trailer.HaulierId,
                    HaulierName = vct.Trailer.Haulier.HaulierName,
                    UnladenWeightKg = vct.Trailer.UnladenWeightKg,
                    MaxPayloadKg = vct.Trailer.MaxPayloadKg,
                    MaxPayloadLiters = vct.Trailer.MaxPayloadLiters,
                    NumberOfAxles = vct.Trailer.NumberOfAxles,
                    Status = vct.Trailer.Status,
                    CreatedAt = vct.Trailer.CreatedAt,
                    UpdatedAt = vct.Trailer.UpdatedAt
                }).ToList()
            })
            .FirstOrDefaultAsync();
    }

    public async Task<VehicleCombinationResponseDto> CreateAsync(CreateVehicleCombinationDto createVehicleCombinationDto, int? createdBy = null)
    {
        // Check if combination code already exists for this tractor
        if (await ExistsByCombinationCodeAndTractorAsync(createVehicleCombinationDto.CombinationCode, createVehicleCombinationDto.TractorId))
        {
            throw new ArgumentException($"Vehicle combination with code '{createVehicleCombinationDto.CombinationCode}' already exists for this tractor");
        }

        // Verify tractor exists
        var tractorExists = await _context.Tractors.AnyAsync(t => t.Id == createVehicleCombinationDto.TractorId && t.DeletedAt == null);
        if (!tractorExists)
        {
            throw new ArgumentException($"Tractor with ID {createVehicleCombinationDto.TractorId} not found");
        }

        var vehicleCombination = new VehicleCombination
        {
            CombinationCode = createVehicleCombinationDto.CombinationCode,
            TractorId = createVehicleCombinationDto.TractorId,
            GrossWeightLimitKg = createVehicleCombinationDto.GrossWeightLimitKg,
            TotalCapacityL = createVehicleCombinationDto.TotalCapacityL,
            Active = createVehicleCombinationDto.Active ?? true,
            IsDefault = createVehicleCombinationDto.IsDefault ?? false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.VehicleCombinations.Add(vehicleCombination);
        await _context.SaveChangesAsync();

        return await GetByIdAsync(vehicleCombination.Id) ?? throw new InvalidOperationException("Failed to retrieve created vehicle combination");
    }

    public async Task<VehicleCombinationResponseDto?> UpdateAsync(int id, UpdateVehicleCombinationDto updateVehicleCombinationDto, int? updatedBy = null)
    {
        var vehicleCombination = await _context.VehicleCombinations.FindAsync(id);
        if (vehicleCombination == null || vehicleCombination.DeletedAt != null)
            return null;

        // Check if combination code already exists for this tractor (excluding current combination)
        if (updateVehicleCombinationDto.CombinationCode != null && 
            await ExistsByCombinationCodeAndTractorAsync(updateVehicleCombinationDto.CombinationCode, vehicleCombination.TractorId, id))
        {
            throw new ArgumentException($"Vehicle combination with code '{updateVehicleCombinationDto.CombinationCode}' already exists for this tractor");
        }

        // Update only provided fields
        if (updateVehicleCombinationDto.CombinationCode != null)
            vehicleCombination.CombinationCode = updateVehicleCombinationDto.CombinationCode;
        if (updateVehicleCombinationDto.GrossWeightLimitKg.HasValue)
            vehicleCombination.GrossWeightLimitKg = updateVehicleCombinationDto.GrossWeightLimitKg.Value;
        if (updateVehicleCombinationDto.TotalCapacityL.HasValue)
            vehicleCombination.TotalCapacityL = updateVehicleCombinationDto.TotalCapacityL.Value;
        if (updateVehicleCombinationDto.Active.HasValue)
            vehicleCombination.Active = updateVehicleCombinationDto.Active.Value;
        if (updateVehicleCombinationDto.IsDefault.HasValue)
            vehicleCombination.IsDefault = updateVehicleCombinationDto.IsDefault.Value;

        vehicleCombination.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return await GetByIdAsync(id);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var vehicleCombination = await _context.VehicleCombinations.FindAsync(id);
        if (vehicleCombination == null || vehicleCombination.DeletedAt != null)
            return false;

        // Remove all trailer associations first
        var trailerAssociations = await _context.VehicleCombinationTrailers
            .Where(vct => vct.CombinationId == id)
            .ToListAsync();
        
        _context.VehicleCombinationTrailers.RemoveRange(trailerAssociations);

        // Soft delete the combination
        vehicleCombination.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.VehicleCombinations.AnyAsync(vc => vc.Id == id && vc.DeletedAt == null);
    }

    public async Task<bool> ExistsByCombinationCodeAndTractorAsync(string combinationCode, int tractorId, int? excludeId = null)
    {
        var query = _context.VehicleCombinations.Where(vc => vc.CombinationCode == combinationCode && vc.TractorId == tractorId && vc.DeletedAt == null);
        
        if (excludeId.HasValue)
            query = query.Where(vc => vc.Id != excludeId.Value);

        return await query.AnyAsync();
    }

    public async Task<IEnumerable<VehicleCombinationListItemDto>> GetByTractorIdAsync(int tractorId)
    {
        return await _context.VehicleCombinations
            .Include(vc => vc.Tractor)
            .Include(vc => vc.VehicleCombinationTrailers)
            .Where(vc => vc.TractorId == tractorId && vc.DeletedAt == null)
            .Select(vc => new VehicleCombinationListItemDto
            {
                Id = vc.Id,
                CombinationCode = vc.CombinationCode,
                TractorId = vc.TractorId,
                TractorName = vc.Tractor.TractorName,
                TractorCode = vc.Tractor.TractorCode,
                GrossWeightLimitKg = vc.GrossWeightLimitKg,
                TotalCapacityL = vc.TotalCapacityL,
                Active = vc.Active,
                IsDefault = vc.IsDefault,
                TrailerCount = vc.VehicleCombinationTrailers.Count(),
                CreatedAt = vc.CreatedAt,
                UpdatedAt = vc.UpdatedAt
            })
            .OrderBy(vc => vc.CombinationCode)
            .ToListAsync();
    }

    public async Task<IEnumerable<VehicleCombinationListItemDto>> SearchAsync(string searchTerm)
    {
        var normalizedSearchTerm = searchTerm.ToLower();

        return await _context.VehicleCombinations
            .Include(vc => vc.Tractor)
            .Include(vc => vc.VehicleCombinationTrailers)
            .Where(vc => vc.DeletedAt == null && 
                        (vc.CombinationCode.ToLower().Contains(normalizedSearchTerm) ||
                         vc.Tractor.TractorName.ToLower().Contains(normalizedSearchTerm) ||
                         vc.Tractor.TractorCode.ToLower().Contains(normalizedSearchTerm)))
            .Select(vc => new VehicleCombinationListItemDto
            {
                Id = vc.Id,
                CombinationCode = vc.CombinationCode,
                TractorId = vc.TractorId,
                TractorName = vc.Tractor.TractorName,
                TractorCode = vc.Tractor.TractorCode,
                GrossWeightLimitKg = vc.GrossWeightLimitKg,
                TotalCapacityL = vc.TotalCapacityL,
                Active = vc.Active,
                IsDefault = vc.IsDefault,
                TrailerCount = vc.VehicleCombinationTrailers.Count(),
                CreatedAt = vc.CreatedAt,
                UpdatedAt = vc.UpdatedAt
            })
            .OrderBy(vc => vc.TractorName)
            .ThenBy(vc => vc.CombinationCode)
            .ToListAsync();
    }

    public async Task<VehicleCombinationTrailerResponseDto> AddTrailerToCombinationAsync(int combinationId, AddTrailerToCombinationDto addTrailerDto, int? createdBy = null)
    {
        // Verify combination exists
        var combinationExists = await _context.VehicleCombinations.AnyAsync(vc => vc.Id == combinationId && vc.DeletedAt == null);
        if (!combinationExists)
        {
            throw new ArgumentException($"Vehicle combination with ID {combinationId} not found");
        }

        // Verify trailer exists
        var trailerExists = await _context.Trailers.AnyAsync(t => t.Id == addTrailerDto.TrailerId && t.DeletedAt == null);
        if (!trailerExists)
        {
            throw new ArgumentException($"Trailer with ID {addTrailerDto.TrailerId} not found");
        }

        // Check if trailer is already in this combination
        if (await IsTrailerInCombinationAsync(combinationId, addTrailerDto.TrailerId))
        {
            throw new ArgumentException("Trailer is already part of this vehicle combination");
        }

        var combinationTrailer = new VehicleCombinationTrailer
        {
            CombinationId = combinationId,
            TrailerId = addTrailerDto.TrailerId,
            SequenceNumber = addTrailerDto.SequenceNumber
        };

        _context.VehicleCombinationTrailers.Add(combinationTrailer);
        await _context.SaveChangesAsync();

        return await _context.VehicleCombinationTrailers
            .Include(vct => vct.VehicleCombination)
                .ThenInclude(vc => vc.Tractor)
            .Include(vct => vct.Trailer)
                .ThenInclude(t => t.Haulier)
            .Where(vct => vct.CombinationId == combinationId && vct.TrailerId == addTrailerDto.TrailerId)
            .Select(vct => new VehicleCombinationTrailerResponseDto
            {
                CombinationId = vct.CombinationId,
                TrailerId = vct.TrailerId,
                SequenceNumber = vct.SequenceNumber,
                VehicleCombination = new VehicleCombinationListItemDto
                {
                    Id = vct.VehicleCombination.Id,
                    CombinationCode = vct.VehicleCombination.CombinationCode,
                    TractorId = vct.VehicleCombination.TractorId,
                    TractorName = vct.VehicleCombination.Tractor.TractorName,
                    TractorCode = vct.VehicleCombination.Tractor.TractorCode,
                    GrossWeightLimitKg = vct.VehicleCombination.GrossWeightLimitKg,
                    TotalCapacityL = vct.VehicleCombination.TotalCapacityL,
                    Active = vct.VehicleCombination.Active,
                    IsDefault = vct.VehicleCombination.IsDefault,
                    TrailerCount = 0, // This would need a separate query to calculate
                    CreatedAt = vct.VehicleCombination.CreatedAt,
                    UpdatedAt = vct.VehicleCombination.UpdatedAt
                },
                Trailer = new TrailerListItemDto
                {
                    Id = vct.Trailer.Id,
                    TrailerCode = vct.Trailer.TrailerCode,
                    TrailerName = vct.Trailer.TrailerName,
                    LicensePlate = vct.Trailer.LicensePlate,
                    HaulierId = vct.Trailer.HaulierId,
                    HaulierName = vct.Trailer.Haulier.HaulierName,
                    UnladenWeightKg = vct.Trailer.UnladenWeightKg,
                    MaxPayloadKg = vct.Trailer.MaxPayloadKg,
                    MaxPayloadLiters = vct.Trailer.MaxPayloadLiters,
                    NumberOfAxles = vct.Trailer.NumberOfAxles,
                    Status = vct.Trailer.Status,
                    CreatedAt = vct.Trailer.CreatedAt,
                    UpdatedAt = vct.Trailer.UpdatedAt
                }
            })
            .FirstAsync();
    }

    public async Task<bool> RemoveTrailerFromCombinationAsync(int combinationId, int trailerId)
    {
        var combinationTrailer = await _context.VehicleCombinationTrailers
            .FirstOrDefaultAsync(vct => vct.CombinationId == combinationId && vct.TrailerId == trailerId);

        if (combinationTrailer == null)
            return false;

        _context.VehicleCombinationTrailers.Remove(combinationTrailer);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> IsTrailerInCombinationAsync(int combinationId, int trailerId)
    {
        return await _context.VehicleCombinationTrailers
            .AnyAsync(vct => vct.CombinationId == combinationId && vct.TrailerId == trailerId);
    }

    public async Task<IEnumerable<TrailerListItemDto>> GetTrailersInCombinationAsync(int combinationId)
    {
        return await _context.VehicleCombinationTrailers
            .Include(vct => vct.Trailer)
                .ThenInclude(t => t.Haulier)
            .Where(vct => vct.CombinationId == combinationId)
            .OrderBy(vct => vct.SequenceNumber)
            .Select(vct => new TrailerListItemDto
            {
                Id = vct.Trailer.Id,
                TrailerCode = vct.Trailer.TrailerCode,
                TrailerName = vct.Trailer.TrailerName,
                LicensePlate = vct.Trailer.LicensePlate,
                HaulierId = vct.Trailer.HaulierId,
                HaulierName = vct.Trailer.Haulier.HaulierName,
                UnladenWeightKg = vct.Trailer.UnladenWeightKg,
                MaxPayloadKg = vct.Trailer.MaxPayloadKg,
                MaxPayloadLiters = vct.Trailer.MaxPayloadLiters,
                NumberOfAxles = vct.Trailer.NumberOfAxles,
                Status = vct.Trailer.Status,
                CreatedAt = vct.Trailer.CreatedAt,
                UpdatedAt = vct.Trailer.UpdatedAt
            })
            .ToListAsync();
    }

    public async Task<IEnumerable<VehicleCombinationListItemDto>> GetCombinationsWithTrailerAsync(int trailerId)
    {
        return await _context.VehicleCombinationTrailers
            .Include(vct => vct.VehicleCombination)
                .ThenInclude(vc => vc.Tractor)
            .Where(vct => vct.TrailerId == trailerId)
            .Select(vct => new VehicleCombinationListItemDto
            {
                Id = vct.VehicleCombination.Id,
                CombinationCode = vct.VehicleCombination.CombinationCode,
                TractorId = vct.VehicleCombination.TractorId,
                TractorName = vct.VehicleCombination.Tractor.TractorName,
                TractorCode = vct.VehicleCombination.Tractor.TractorCode,
                GrossWeightLimitKg = vct.VehicleCombination.GrossWeightLimitKg,
                TotalCapacityL = vct.VehicleCombination.TotalCapacityL,
                Active = vct.VehicleCombination.Active,
                IsDefault = vct.VehicleCombination.IsDefault,
                TrailerCount = 0, // This would need a separate query to calculate
                CreatedAt = vct.VehicleCombination.CreatedAt,
                UpdatedAt = vct.VehicleCombination.UpdatedAt
            })
            .Distinct()
            .ToListAsync();
    }

    public async Task<VehicleCombinationResponseDto?> GetDefaultCombinationForTractorAsync(int tractorId)
    {
        var defaultCombination = await _context.VehicleCombinations
            .Where(vc => vc.TractorId == tractorId && vc.IsDefault && vc.DeletedAt == null)
            .FirstOrDefaultAsync();

        return defaultCombination != null ? await GetByIdAsync(defaultCombination.Id) : null;
    }

    public async Task<bool> SetDefaultCombinationAsync(int combinationId, int? updatedBy = null)
    {
        var combination = await _context.VehicleCombinations.FindAsync(combinationId);
        if (combination == null || combination.DeletedAt != null)
            return false;

        // Remove default flag from all other combinations for this tractor
        var otherCombinations = await _context.VehicleCombinations
            .Where(vc => vc.TractorId == combination.TractorId && vc.Id != combinationId && vc.DeletedAt == null)
            .ToListAsync();

        foreach (var otherCombination in otherCombinations)
        {
            otherCombination.IsDefault = false;
            otherCombination.UpdatedAt = DateTime.UtcNow;
        }

        // Set this combination as default
        combination.IsDefault = true;
        combination.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveDefaultCombinationAsync(int tractorId, int? updatedBy = null)
    {
        var defaultCombination = await _context.VehicleCombinations
            .Where(vc => vc.TractorId == tractorId && vc.IsDefault && vc.DeletedAt == null)
            .FirstOrDefaultAsync();

        if (defaultCombination == null)
            return false;

        defaultCombination.IsDefault = false;
        defaultCombination.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }
}