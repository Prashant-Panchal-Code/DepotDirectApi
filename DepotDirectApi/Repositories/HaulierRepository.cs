using DepotDirectApi.Data;
using DepotDirectApi.Models.DTOs;
using DepotDirectApi.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace DepotDirectApi.Repositories;

public class HaulierRepository : IHaulierRepository
{
    private readonly DepotDirectDbContext _context;

    public HaulierRepository(DepotDirectDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<HaulierListItemDto>> GetAllAsync()
    {
        return await _context.Hauliers
            .Include(h => h.Region)
            .Where(h => h.DeletedAt == null)
            .Select(h => new HaulierListItemDto
            {
                Id = h.Id,
                RegionId = h.RegionId,
                RegionName = h.Region.Name,
                HaulierCode = h.HaulierCode,
                HaulierName = h.HaulierName,
                TaxId = h.TaxId,
                ContractNumber = h.ContractNumber,
                ContractExpiry = h.ContractExpiry,
                ContactName = h.ContactName,
                ContactEmail = h.ContactEmail,
                ContactPhone = h.ContactPhone,
                Active = h.Active,
                CreatedAt = h.CreatedAt,
                UpdatedAt = h.UpdatedAt
            })
            .OrderBy(h => h.RegionName)
            .ThenBy(h => h.HaulierName)
            .ToListAsync();
    }

    public async Task<HaulierResponseDto?> GetByIdAsync(int id)
    {
        return await _context.Hauliers
            .Include(h => h.Region)
            .ThenInclude(r => r.Company)
            .Where(h => h.Id == id && h.DeletedAt == null)
            .Select(h => new HaulierResponseDto
            {
                Id = h.Id,
                RegionId = h.RegionId,
                HaulierCode = h.HaulierCode,
                HaulierName = h.HaulierName,
                TaxId = h.TaxId,
                ContractNumber = h.ContractNumber,
                ContractExpiry = h.ContractExpiry,
                ContactName = h.ContactName,
                ContactEmail = h.ContactEmail,
                ContactPhone = h.ContactPhone,
                Active = h.Active,
                Metadata = h.Metadata,
                CreatedAt = h.CreatedAt,
                UpdatedAt = h.UpdatedAt,
                DeletedAt = h.DeletedAt,
                Region = new RegionDto
                {
                    Id = h.Region.Id,
                    Name = h.Region.Name,
                    RegionCode = h.Region.RegionCode,
                    CompanyId = h.Region.CompanyId,
                    Metadata = h.Region.Metadata,
                    CreatedBy = h.Region.CreatedBy,
                    LastUpdatedBy = h.Region.LastUpdatedBy,
                    CreatedAt = h.Region.CreatedAt,
                    UpdatedAt = h.Region.UpdatedAt
                }
            })
            .FirstOrDefaultAsync();
    }

    public async Task<HaulierResponseDto> CreateAsync(CreateHaulierDto createHaulierDto, int? createdBy = null)
    {
        // Validate region exists
        var regionExists = await _context.Regions.AnyAsync(r => r.Id == createHaulierDto.RegionId && r.DeletedAt == null);
        if (!regionExists)
        {
            throw new ArgumentException($"Region with ID {createHaulierDto.RegionId} does not exist.");
        }

        // Check if haulier code already exists for this region
        if (await ExistsByHaulierCodeAndRegionAsync(createHaulierDto.HaulierCode, createHaulierDto.RegionId))
        {
            throw new ArgumentException($"Haulier with code '{createHaulierDto.HaulierCode}' already exists in this region.");
        }

        var haulier = new Haulier
        {
            RegionId = createHaulierDto.RegionId,
            HaulierCode = createHaulierDto.HaulierCode,
            HaulierName = createHaulierDto.HaulierName,
            TaxId = createHaulierDto.TaxId,
            ContractNumber = createHaulierDto.ContractNumber,
            ContractExpiry = createHaulierDto.ContractExpiry,
            ContactName = createHaulierDto.ContactName,
            ContactEmail = createHaulierDto.ContactEmail,
            ContactPhone = createHaulierDto.ContactPhone,
            Active = createHaulierDto.Active ?? true,
            Metadata = createHaulierDto.Metadata,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Hauliers.Add(haulier);
        await _context.SaveChangesAsync();

        return await GetByIdAsync(haulier.Id) ?? throw new InvalidOperationException("Failed to retrieve created haulier.");
    }

    public async Task<HaulierResponseDto?> UpdateAsync(int id, UpdateHaulierDto updateHaulierDto, int? updatedBy = null)
    {
        var haulier = await _context.Hauliers
            .Where(h => h.Id == id && h.DeletedAt == null)
            .FirstOrDefaultAsync();

        if (haulier == null)
            return null;

        // Check if haulier code already exists for this region (excluding current haulier)
        if (!string.IsNullOrEmpty(updateHaulierDto.HaulierCode) && updateHaulierDto.HaulierCode != haulier.HaulierCode)
        {
            if (await ExistsByHaulierCodeAndRegionAsync(updateHaulierDto.HaulierCode, haulier.RegionId, id))
            {
                throw new ArgumentException($"Haulier with code '{updateHaulierDto.HaulierCode}' already exists in this region.");
            }
        }

        // Update fields
        if (!string.IsNullOrEmpty(updateHaulierDto.HaulierCode))
            haulier.HaulierCode = updateHaulierDto.HaulierCode;
        if (!string.IsNullOrEmpty(updateHaulierDto.HaulierName))
            haulier.HaulierName = updateHaulierDto.HaulierName;
        if (updateHaulierDto.TaxId != null)
            haulier.TaxId = updateHaulierDto.TaxId;
        if (updateHaulierDto.ContractNumber != null)
            haulier.ContractNumber = updateHaulierDto.ContractNumber;
        if (updateHaulierDto.ContractExpiry.HasValue)
            haulier.ContractExpiry = updateHaulierDto.ContractExpiry;
        if (updateHaulierDto.ContactName != null)
            haulier.ContactName = updateHaulierDto.ContactName;
        if (updateHaulierDto.ContactEmail != null)
            haulier.ContactEmail = updateHaulierDto.ContactEmail;
        if (updateHaulierDto.ContactPhone != null)
            haulier.ContactPhone = updateHaulierDto.ContactPhone;
        if (updateHaulierDto.Active.HasValue)
            haulier.Active = updateHaulierDto.Active.Value;
        if (updateHaulierDto.Metadata != null)
            haulier.Metadata = updateHaulierDto.Metadata;

        haulier.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return await GetByIdAsync(id);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var haulier = await _context.Hauliers
            .Where(h => h.Id == id && h.DeletedAt == null)
            .FirstOrDefaultAsync();

        if (haulier == null)
            return false;

        // Check if haulier has any active vehicles
        var hasActiveVehicles = await _context.Tractors.AnyAsync(t => t.HaulierId == id && t.DeletedAt == null) ||
                              await _context.Trailers.AnyAsync(t => t.HaulierId == id && t.DeletedAt == null);

        if (hasActiveVehicles)
        {
            throw new InvalidOperationException("Cannot delete haulier that has active vehicles assigned.");
        }

        // Soft delete
        haulier.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Hauliers.AnyAsync(h => h.Id == id && h.DeletedAt == null);
    }

    public async Task<bool> ExistsByHaulierCodeAndRegionAsync(string haulierCode, int regionId, int? excludeId = null)
    {
        var query = _context.Hauliers
            .Where(h => h.HaulierCode == haulierCode && h.RegionId == regionId && h.DeletedAt == null);

        if (excludeId.HasValue)
            query = query.Where(h => h.Id != excludeId.Value);

        return await query.AnyAsync();
    }

    public async Task<IEnumerable<HaulierListItemDto>> GetByRegionIdAsync(int regionId)
    {
        return await _context.Hauliers
            .Include(h => h.Region)
            .Where(h => h.RegionId == regionId && h.DeletedAt == null)
            .Select(h => new HaulierListItemDto
            {
                Id = h.Id,
                RegionId = h.RegionId,
                RegionName = h.Region.Name,
                HaulierCode = h.HaulierCode,
                HaulierName = h.HaulierName,
                TaxId = h.TaxId,
                ContractNumber = h.ContractNumber,
                ContractExpiry = h.ContractExpiry,
                ContactName = h.ContactName,
                ContactEmail = h.ContactEmail,
                ContactPhone = h.ContactPhone,
                Active = h.Active,
                CreatedAt = h.CreatedAt,
                UpdatedAt = h.UpdatedAt
            })
            .OrderBy(h => h.HaulierName)
            .ToListAsync();
    }

    public async Task<IEnumerable<HaulierListItemDto>> SearchAsync(string searchTerm)
    {
        var normalizedSearchTerm = searchTerm.ToLower().Trim();

        return await _context.Hauliers
            .Include(h => h.Region)
            .Where(h => h.DeletedAt == null &&
                       (h.HaulierCode.ToLower().Contains(normalizedSearchTerm) ||
                        h.HaulierName.ToLower().Contains(normalizedSearchTerm) ||
                        (h.ContactName != null && h.ContactName.ToLower().Contains(normalizedSearchTerm)) ||
                        (h.TaxId != null && h.TaxId.ToLower().Contains(normalizedSearchTerm))))
            .Select(h => new HaulierListItemDto
            {
                Id = h.Id,
                RegionId = h.RegionId,
                RegionName = h.Region.Name,
                HaulierCode = h.HaulierCode,
                HaulierName = h.HaulierName,
                TaxId = h.TaxId,
                ContractNumber = h.ContractNumber,
                ContractExpiry = h.ContractExpiry,
                ContactName = h.ContactName,
                ContactEmail = h.ContactEmail,
                ContactPhone = h.ContactPhone,
                Active = h.Active,
                CreatedAt = h.CreatedAt,
                UpdatedAt = h.UpdatedAt
            })
            .OrderBy(h => h.RegionName)
            .ThenBy(h => h.HaulierName)
            .ToListAsync();
    }

    public async Task<IEnumerable<HaulierListItemDto>> GetActiveHauliersAsync()
    {
        return await _context.Hauliers
            .Include(h => h.Region)
            .Where(h => h.Active && h.DeletedAt == null)
            .Select(h => new HaulierListItemDto
            {
                Id = h.Id,
                RegionId = h.RegionId,
                RegionName = h.Region.Name,
                HaulierCode = h.HaulierCode,
                HaulierName = h.HaulierName,
                TaxId = h.TaxId,
                ContractNumber = h.ContractNumber,
                ContractExpiry = h.ContractExpiry,
                ContactName = h.ContactName,
                ContactEmail = h.ContactEmail,
                ContactPhone = h.ContactPhone,
                Active = h.Active,
                CreatedAt = h.CreatedAt,
                UpdatedAt = h.UpdatedAt
            })
            .OrderBy(h => h.RegionName)
            .ThenBy(h => h.HaulierName)
            .ToListAsync();
    }

    public async Task<IEnumerable<HaulierListItemDto>> GetByContractExpiryDateAsync(DateTime fromDate, DateTime toDate)
    {
        return await _context.Hauliers
            .Include(h => h.Region)
            .Where(h => h.DeletedAt == null && h.ContractExpiry.HasValue && 
                       h.ContractExpiry.Value >= fromDate && h.ContractExpiry.Value <= toDate)
            .Select(h => new HaulierListItemDto
            {
                Id = h.Id,
                RegionId = h.RegionId,
                RegionName = h.Region.Name,
                HaulierCode = h.HaulierCode,
                HaulierName = h.HaulierName,
                TaxId = h.TaxId,
                ContractNumber = h.ContractNumber,
                ContractExpiry = h.ContractExpiry,
                ContactName = h.ContactName,
                ContactEmail = h.ContactEmail,
                ContactPhone = h.ContactPhone,
                Active = h.Active,
                CreatedAt = h.CreatedAt,
                UpdatedAt = h.UpdatedAt
            })
            .OrderBy(h => h.ContractExpiry)
            .ThenBy(h => h.HaulierName)
            .ToListAsync();
    }
}