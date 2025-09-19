using DepotDirectApi.Data;
using DepotDirectApi.Models.DTOs;
using DepotDirectApi.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace DepotDirectApi.Repositories;

public class RegionRepository : IRegionRepository
{
    private readonly DepotDirectDbContext _context;

    public RegionRepository(DepotDirectDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<RegionListItemDto>> GetAllAsync()
    {
        return await _context.Regions
            .Where(r => r.DeletedAt == null)
            .Include(r => r.Company)
            .Select(r => new RegionListItemDto
            {
                Id = r.Id,
                Name = r.Name,
                RegionCode = r.RegionCode,
                CompanyId = r.CompanyId,
                CompanyName = r.Company.Name,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt
            })
            .OrderBy(r => r.Name)
            .ToListAsync();
    }

    public async Task<RegionResponseDto?> GetByIdAsync(int id)
    {
        var region = await _context.Regions
            .Where(r => r.Id == id && r.DeletedAt == null)
            .Include(r => r.Company)
            .ThenInclude(c => c.Country)
            .FirstOrDefaultAsync();

        if (region == null)
            return null;

        return new RegionResponseDto
        {
            Id = region.Id,
            Name = region.Name,
            RegionCode = region.RegionCode,
            CompanyId = region.CompanyId,
            Metadata = region.Metadata,
            CreatedBy = region.CreatedBy,
            LastUpdatedBy = region.LastUpdatedBy,
            CreatedAt = region.CreatedAt,
            UpdatedAt = region.UpdatedAt,
            DeletedAt = region.DeletedAt,
            Company = region.Company != null ? new CompanyDto
            {
                Id = region.Company.Id,
                Name = region.Company.Name,
                CompanyCode = region.Company.CompanyCode,
                CountryId = region.Company.CountryId,
                Description = region.Company.Description,
                Metadata = region.Company.Metadata,
                CreatedBy = region.Company.CreatedBy,
                LastUpdatedBy = region.Company.LastUpdatedBy,
                CreatedAt = region.Company.CreatedAt,
                UpdatedAt = region.Company.UpdatedAt
            } : null
        };
    }

    public async Task<RegionResponseDto> CreateAsync(CreateRegionDto createRegionDto, int? createdBy = null)
    {
        // Validate company exists
        var companyExists = await _context.Companies
            .AnyAsync(c => c.Id == createRegionDto.CompanyId && c.DeletedAt == null);
        
        if (!companyExists)
            throw new ArgumentException($"Company with ID {createRegionDto.CompanyId} does not exist.");

        // Check if region code is unique within the company (if provided)
        if (!string.IsNullOrEmpty(createRegionDto.RegionCode))
        {
            var codeExists = await ExistsByCodeAndCompanyAsync(createRegionDto.RegionCode, createRegionDto.CompanyId);
            if (codeExists)
                throw new ArgumentException($"Region code '{createRegionDto.RegionCode}' already exists in this company.");
        }

        var region = new Region
        {
            Name = createRegionDto.Name,
            RegionCode = createRegionDto.RegionCode,
            CompanyId = createRegionDto.CompanyId,
            Metadata = createRegionDto.Metadata,
            CreatedBy = createdBy,
            LastUpdatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Regions.Add(region);
        await _context.SaveChangesAsync();

        // Return the created region with company information
        return await GetByIdAsync(region.Id) ?? throw new InvalidOperationException("Failed to retrieve created region.");
    }

    public async Task<RegionResponseDto?> UpdateAsync(int id, UpdateRegionDto updateRegionDto, int? updatedBy = null)
    {
        var region = await _context.Regions
            .Where(r => r.Id == id && r.DeletedAt == null)
            .FirstOrDefaultAsync();

        if (region == null)
            return null;

        // Validate company exists if companyId is being updated
        if (updateRegionDto.CompanyId.HasValue)
        {
            var companyExists = await _context.Companies
                .AnyAsync(c => c.Id == updateRegionDto.CompanyId.Value && c.DeletedAt == null);
            
            if (!companyExists)
                throw new ArgumentException($"Company with ID {updateRegionDto.CompanyId.Value} does not exist.");
        }

        // Check if region code is unique within the company (if provided and changed)
        if (!string.IsNullOrEmpty(updateRegionDto.RegionCode))
        {
            var companyId = updateRegionDto.CompanyId ?? region.CompanyId;
            var codeExists = await ExistsByCodeAndCompanyAsync(updateRegionDto.RegionCode, companyId, id);
            if (codeExists)
                throw new ArgumentException($"Region code '{updateRegionDto.RegionCode}' already exists in this company.");
        }

        // Update fields
        if (!string.IsNullOrEmpty(updateRegionDto.Name))
            region.Name = updateRegionDto.Name;
            
        if (updateRegionDto.RegionCode != null)
            region.RegionCode = updateRegionDto.RegionCode;
            
        if (updateRegionDto.CompanyId.HasValue)
            region.CompanyId = updateRegionDto.CompanyId.Value;
            
        if (updateRegionDto.Metadata != null)
            region.Metadata = updateRegionDto.Metadata;

        region.LastUpdatedBy = updatedBy;
        region.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return await GetByIdAsync(id);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var region = await _context.Regions
            .Where(r => r.Id == id && r.DeletedAt == null)
            .FirstOrDefaultAsync();

        if (region == null)
            return false;

        // Soft delete
        region.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Regions
            .AnyAsync(r => r.Id == id && r.DeletedAt == null);
    }

    public async Task<bool> ExistsByCodeAndCompanyAsync(string regionCode, int companyId, int? excludeId = null)
    {
        var query = _context.Regions
            .Where(r => r.RegionCode == regionCode && r.CompanyId == companyId && r.DeletedAt == null);

        if (excludeId.HasValue)
            query = query.Where(r => r.Id != excludeId.Value);

        return await query.AnyAsync();
    }

    public async Task<IEnumerable<RegionListItemDto>> GetByCompanyIdAsync(int companyId)
    {
        return await _context.Regions
            .Where(r => r.CompanyId == companyId && r.DeletedAt == null)
            .Include(r => r.Company)
            .Select(r => new RegionListItemDto
            {
                Id = r.Id,
                Name = r.Name,
                RegionCode = r.RegionCode,
                CompanyId = r.CompanyId,
                CompanyName = r.Company.Name,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt
            })
            .OrderBy(r => r.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<RegionListItemDto>> SearchAsync(string searchTerm)
    {
        var normalizedSearchTerm = searchTerm.ToLower().Trim();
        
        return await _context.Regions
            .Where(r => r.DeletedAt == null && 
                       (r.Name.ToLower().Contains(normalizedSearchTerm) ||
                        (r.RegionCode != null && r.RegionCode.ToLower().Contains(normalizedSearchTerm))))
            .Include(r => r.Company)
            .Select(r => new RegionListItemDto
            {
                Id = r.Id,
                Name = r.Name,
                RegionCode = r.RegionCode,
                CompanyId = r.CompanyId,
                CompanyName = r.Company.Name,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt
            })
            .OrderBy(r => r.Name)
            .ToListAsync();
    }
}