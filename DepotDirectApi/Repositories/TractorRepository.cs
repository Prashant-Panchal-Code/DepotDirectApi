using DepotDirectApi.Data;
using DepotDirectApi.Models.DTOs;
using DepotDirectApi.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace DepotDirectApi.Repositories;

public class TractorRepository : ITractorRepository
{
    private readonly DepotDirectDbContext _context;
    private readonly ILogger<TractorRepository> _logger;

    public TractorRepository(DepotDirectDbContext context, ILogger<TractorRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<TractorListItemDto>> GetAllAsync()
    {
        return await _context.Tractors
            .Include(t => t.Haulier)
            .ThenInclude(h => h.Region)
            .Include(t => t.Region)
            .Where(t => t.DeletedAt == null)
            .Select(t => new TractorListItemDto
            {
                Id = t.Id,
                TractorCode = t.TractorCode,
                TractorName = t.TractorName,
                LicensePlate = t.LicensePlate,
                HaulierId = t.HaulierId,
                HaulierName = t.Haulier.HaulierName,
                RegionId = t.RegionId,
                RegionName = t.Region != null ? t.Region.Name : null,
                Status = t.Status,
                PumpAvailable = t.PumpAvailable,
                PumpFlowRateLpm = t.PumpFlowRateLpm,
                CurbWeightKg = t.CurbWeightKg,
                NumberOfAxles = t.NumberOfAxles,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt
            })
            .OrderBy(t => t.HaulierName)
            .ThenBy(t => t.TractorName)
            .ToListAsync();
    }

    public async Task<TractorResponseDto?> GetByIdAsync(int id)
    {
        return await _context.Tractors
            .Include(t => t.Haulier)
            .ThenInclude(h => h.Region)
            .Include(t => t.Region)
            .Where(t => t.Id == id && t.DeletedAt == null)
            .Select(t => new TractorResponseDto
            {
                Id = t.Id,
                TractorCode = t.TractorCode,
                TractorName = t.TractorName,
                LicensePlate = t.LicensePlate,
                HaulierId = t.HaulierId,
                RegionId = t.RegionId,
                Status = t.Status,
                PumpAvailable = t.PumpAvailable,
                PumpFlowRateLpm = t.PumpFlowRateLpm,
                CurbWeightKg = t.CurbWeightKg,
                NumberOfAxles = t.NumberOfAxles,
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
                } : null
            })
            .FirstOrDefaultAsync();
    }

    public async Task<TractorResponseDto> CreateAsync(CreateTractorDto createTractorDto, int? createdBy = null)
    {
        // Check if tractor code already exists for this haulier
        if (await ExistsByTractorCodeAndHaulierAsync(createTractorDto.TractorCode, createTractorDto.HaulierId))
        {
            throw new ArgumentException($"Tractor with code '{createTractorDto.TractorCode}' already exists for this haulier");
        }

        // Check if license plate already exists
        if (await ExistsByLicensePlateAsync(createTractorDto.LicensePlate))
        {
            throw new ArgumentException($"Tractor with license plate '{createTractorDto.LicensePlate}' already exists");
        }

        // Verify haulier exists
        var haulierExists = await _context.Hauliers.AnyAsync(h => h.Id == createTractorDto.HaulierId && h.DeletedAt == null);
        if (!haulierExists)
        {
            throw new ArgumentException($"Haulier with ID {createTractorDto.HaulierId} not found");
        }

        // Verify region exists and belongs to the same region as the haulier
        if (createTractorDto.RegionId.HasValue)
        {
            var haulier = await _context.Hauliers.FindAsync(createTractorDto.HaulierId);
            if (haulier != null && createTractorDto.RegionId.Value != haulier.RegionId)
            {
                throw new ArgumentException("Tractor region must match the haulier's region");
            }
        }

        var tractor = new Tractor
        {
            TractorCode = createTractorDto.TractorCode,
            TractorName = createTractorDto.TractorName,
            LicensePlate = createTractorDto.LicensePlate,
            HaulierId = createTractorDto.HaulierId,
            RegionId = createTractorDto.RegionId,
            Status = createTractorDto.Status ?? "Active",
            PumpAvailable = createTractorDto.PumpAvailable ?? false,
            PumpFlowRateLpm = createTractorDto.PumpFlowRateLpm,
            CurbWeightKg = createTractorDto.CurbWeightKg,
            NumberOfAxles = createTractorDto.NumberOfAxles,
            AxleConfiguration = createTractorDto.AxleConfiguration,
            Metadata = createTractorDto.Metadata,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Tractors.Add(tractor);
        await _context.SaveChangesAsync();

        return await GetByIdAsync(tractor.Id) ?? throw new InvalidOperationException("Failed to retrieve created tractor");
    }

    public async Task<TractorResponseDto?> UpdateAsync(int id, UpdateTractorDto updateTractorDto, int? updatedBy = null)
    {
        var tractor = await _context.Tractors.Include(t => t.Haulier).FirstOrDefaultAsync(t => t.Id == id);
        if (tractor == null || tractor.DeletedAt != null)
            return null;

        // Check if tractor code already exists for this haulier (excluding current tractor)
        if (updateTractorDto.TractorCode != null && 
            await ExistsByTractorCodeAndHaulierAsync(updateTractorDto.TractorCode, tractor.HaulierId, id))
        {
            throw new ArgumentException($"Tractor with code '{updateTractorDto.TractorCode}' already exists for this haulier");
        }

        // Check if license plate already exists (excluding current tractor)
        if (updateTractorDto.LicensePlate != null && 
            await ExistsByLicensePlateAsync(updateTractorDto.LicensePlate, id))
        {
            throw new ArgumentException($"Tractor with license plate '{updateTractorDto.LicensePlate}' already exists");
        }

        // Verify region exists and belongs to the same region as the haulier
        if (updateTractorDto.RegionId.HasValue)
        {
            if (updateTractorDto.RegionId.Value != tractor.Haulier.RegionId)
            {
                throw new ArgumentException("Tractor region must match the haulier's region");
            }
        }

        // Update only provided fields
        if (updateTractorDto.TractorCode != null)
            tractor.TractorCode = updateTractorDto.TractorCode;
        if (updateTractorDto.TractorName != null)
            tractor.TractorName = updateTractorDto.TractorName;
        if (updateTractorDto.LicensePlate != null)
            tractor.LicensePlate = updateTractorDto.LicensePlate;
        if (updateTractorDto.RegionId.HasValue)
            tractor.RegionId = updateTractorDto.RegionId.Value;
        if (updateTractorDto.Status != null)
            tractor.Status = updateTractorDto.Status;
        if (updateTractorDto.PumpAvailable.HasValue)
            tractor.PumpAvailable = updateTractorDto.PumpAvailable.Value;
        if (updateTractorDto.PumpFlowRateLpm.HasValue)
            tractor.PumpFlowRateLpm = updateTractorDto.PumpFlowRateLpm.Value;
        if (updateTractorDto.CurbWeightKg.HasValue)
            tractor.CurbWeightKg = updateTractorDto.CurbWeightKg.Value;
        if (updateTractorDto.NumberOfAxles.HasValue)
            tractor.NumberOfAxles = updateTractorDto.NumberOfAxles.Value;
        if (updateTractorDto.AxleConfiguration != null)
            tractor.AxleConfiguration = updateTractorDto.AxleConfiguration;
        if (updateTractorDto.Metadata != null)
            tractor.Metadata = updateTractorDto.Metadata;

        tractor.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return await GetByIdAsync(id);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var tractor = await _context.Tractors.FindAsync(id);
        if (tractor == null || tractor.DeletedAt != null)
            return false;

        // Check if tractor is being used in any vehicle combinations
        var isBeingUsed = await _context.VehicleCombinations.AnyAsync(vc => vc.TractorId == id && vc.DeletedAt == null);
        if (isBeingUsed)
        {
            throw new InvalidOperationException("Cannot delete tractor that is part of vehicle combinations");
        }

        // Check if tractor has any active schedules
        var hasActiveSchedules = await _context.TractorSchedules.AnyAsync(ts => ts.TractorId == id && ts.Active && ts.DeletedAt == null);
        if (hasActiveSchedules)
        {
            throw new InvalidOperationException("Cannot delete tractor that has active schedules");
        }

        // Soft delete
        tractor.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Tractors.AnyAsync(t => t.Id == id && t.DeletedAt == null);
    }

    public async Task<bool> ExistsByTractorCodeAndHaulierAsync(string tractorCode, int haulierId, int? excludeId = null)
    {
        var query = _context.Tractors.Where(t => t.TractorCode == tractorCode && t.HaulierId == haulierId && t.DeletedAt == null);
        
        if (excludeId.HasValue)
            query = query.Where(t => t.Id != excludeId.Value);

        return await query.AnyAsync();
    }

    public async Task<bool> ExistsByLicensePlateAsync(string licensePlate, int? excludeId = null)
    {
        var query = _context.Tractors.Where(t => t.LicensePlate == licensePlate && t.DeletedAt == null);
        
        if (excludeId.HasValue)
            query = query.Where(t => t.Id != excludeId.Value);

        return await query.AnyAsync();
    }

    public async Task<IEnumerable<TractorListItemDto>> GetByHaulierIdAsync(int haulierId)
    {
        return await _context.Tractors
            .Include(t => t.Haulier)
            .ThenInclude(h => h.Region)
            .Include(t => t.Region)
            .Where(t => t.HaulierId == haulierId && t.DeletedAt == null)
            .Select(t => new TractorListItemDto
            {
                Id = t.Id,
                TractorCode = t.TractorCode,
                TractorName = t.TractorName,
                LicensePlate = t.LicensePlate,
                HaulierId = t.HaulierId,
                HaulierName = t.Haulier.HaulierName,
                RegionId = t.RegionId,
                RegionName = t.Region != null ? t.Region.Name : null,
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

    public async Task<IEnumerable<TractorListItemDto>> GetByRegionIdAsync(int regionId)
    {
        return await _context.Tractors
            .Include(t => t.Haulier)
            .ThenInclude(h => h.Region)
            .Include(t => t.Region)
            .Where(t => t.RegionId == regionId && t.DeletedAt == null)
            .Select(t => new TractorListItemDto
            {
                Id = t.Id,
                TractorCode = t.TractorCode,
                TractorName = t.TractorName,
                LicensePlate = t.LicensePlate,
                HaulierId = t.HaulierId,
                HaulierName = t.Haulier.HaulierName,
                RegionId = t.RegionId,
                RegionName = t.Region != null ? t.Region.Name : null,
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

    public async Task<IEnumerable<TractorListItemDto>> GetByStatusAsync(string status)
    {
        return await _context.Tractors
            .Include(t => t.Haulier)
            .ThenInclude(h => h.Region)
            .Include(t => t.Region)
            .Where(t => t.Status == status && t.DeletedAt == null)
            .Select(t => new TractorListItemDto
            {
                Id = t.Id,
                TractorCode = t.TractorCode,
                TractorName = t.TractorName,
                LicensePlate = t.LicensePlate,
                HaulierId = t.HaulierId,
                HaulierName = t.Haulier.HaulierName,
                RegionId = t.RegionId,
                RegionName = t.Region != null ? t.Region.Name : null,
                Status = t.Status,
                PumpAvailable = t.PumpAvailable,
                PumpFlowRateLpm = t.PumpFlowRateLpm,
                CurbWeightKg = t.CurbWeightKg,
                NumberOfAxles = t.NumberOfAxles,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt
            })
            .OrderBy(t => t.HaulierName)
            .ThenBy(t => t.TractorName)
            .ToListAsync();
    }

    public async Task<IEnumerable<TractorListItemDto>> GetWithPumpAsync()
    {
        return await _context.Tractors
            .Include(t => t.Haulier)
            .ThenInclude(h => h.Region)
            .Include(t => t.Region)
            .Where(t => t.PumpAvailable && t.DeletedAt == null)
            .Select(t => new TractorListItemDto
            {
                Id = t.Id,
                TractorCode = t.TractorCode,
                TractorName = t.TractorName,
                LicensePlate = t.LicensePlate,
                HaulierId = t.HaulierId,
                HaulierName = t.Haulier.HaulierName,
                RegionId = t.RegionId,
                RegionName = t.Region != null ? t.Region.Name : null,
                Status = t.Status,
                PumpAvailable = t.PumpAvailable,
                PumpFlowRateLpm = t.PumpFlowRateLpm,
                CurbWeightKg = t.CurbWeightKg,
                NumberOfAxles = t.NumberOfAxles,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt
            })
            .OrderBy(t => t.HaulierName)
            .ThenBy(t => t.TractorName)
            .ToListAsync();
    }

    public async Task<IEnumerable<TractorListItemDto>> SearchAsync(string searchTerm)
    {
        var normalizedSearchTerm = searchTerm.ToLower();

        return await _context.Tractors
            .Include(t => t.Haulier)
            .ThenInclude(h => h.Region)
            .Include(t => t.Region)
            .Where(t => t.DeletedAt == null && 
                        (t.TractorCode.ToLower().Contains(normalizedSearchTerm) ||
                         t.TractorName.ToLower().Contains(normalizedSearchTerm) ||
                         t.LicensePlate.ToLower().Contains(normalizedSearchTerm) ||
                         t.Haulier.HaulierName.ToLower().Contains(normalizedSearchTerm)))
            .Select(t => new TractorListItemDto
            {
                Id = t.Id,
                TractorCode = t.TractorCode,
                TractorName = t.TractorName,
                LicensePlate = t.LicensePlate,
                HaulierId = t.HaulierId,
                HaulierName = t.Haulier.HaulierName,
                RegionId = t.RegionId,
                RegionName = t.Region != null ? t.Region.Name : null,
                Status = t.Status,
                PumpAvailable = t.PumpAvailable,
                PumpFlowRateLpm = t.PumpFlowRateLpm,
                CurbWeightKg = t.CurbWeightKg,
                NumberOfAxles = t.NumberOfAxles,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt
            })
            .OrderBy(t => t.HaulierName)
            .ThenBy(t => t.TractorName)
            .ToListAsync();
    }

    public async Task<IEnumerable<TractorListItemDto>> GetAvailableTractorsAsync(DateTime startDate, DateTime endDate)
    {
        // This is a simplified implementation - in a real system you'd check for conflicts with trips, maintenance, etc.
        return await _context.Tractors
            .Include(t => t.Haulier)
            .ThenInclude(h => h.Region)
            .Include(t => t.Region)
            .Where(t => t.Status == "Active" && t.DeletedAt == null)
            .Select(t => new TractorListItemDto
            {
                Id = t.Id,
                TractorCode = t.TractorCode,
                TractorName = t.TractorName,
                LicensePlate = t.LicensePlate,
                HaulierId = t.HaulierId,
                HaulierName = t.Haulier.HaulierName,
                RegionId = t.RegionId,
                RegionName = t.Region != null ? t.Region.Name : null,
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

    public async Task<bool> IsTractorAvailableAsync(int tractorId, DateTime startDate, DateTime endDate)
    {
        // This is a simplified implementation - in a real system you'd check for conflicts with trips, maintenance, etc.
        var tractor = await _context.Tractors
            .Where(t => t.Id == tractorId && t.Status == "Active" && t.DeletedAt == null)
            .FirstOrDefaultAsync();

        return tractor != null;
    }
}