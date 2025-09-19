using DepotDirectApi.Data;
using DepotDirectApi.Models.DTOs;
using DepotDirectApi.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace DepotDirectApi.Repositories;

public class CompanyRepository : ICompanyRepository
{
    private readonly DepotDirectDbContext _context;

    public CompanyRepository(DepotDirectDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CompanyListItemDto>> GetAllAsync()
    {
        return await _context.Companies
            .Where(c => c.DeletedAt == null)
            .Include(c => c.Country)
            .Select(c => new CompanyListItemDto
            {
                Id = c.Id,
                Name = c.Name,
                CompanyCode = c.CompanyCode,
                CountryId = c.CountryId,
                CountryName = c.Country.Name,
                Description = c.Description,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            })
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<CompanyResponseDto?> GetByIdAsync(int id)
    {
        var company = await _context.Companies
            .Where(c => c.Id == id && c.DeletedAt == null)
            .Include(c => c.Country)
            .FirstOrDefaultAsync();

        if (company == null)
            return null;

        return new CompanyResponseDto
        {
            Id = company.Id,
            Name = company.Name,
            CompanyCode = company.CompanyCode,
            CountryId = company.CountryId,
            Description = company.Description,
            Metadata = company.Metadata,
            CreatedBy = company.CreatedBy,
            LastUpdatedBy = company.LastUpdatedBy,
            CreatedAt = company.CreatedAt,
            UpdatedAt = company.UpdatedAt,
            DeletedAt = company.DeletedAt,
            Country = company.Country != null ? new CountryDto
            {
                Id = company.Country.Id,
                Name = company.Country.Name,
                IsoCode = company.Country.IsoCode,
                Metadata = company.Country.Metadata,
                CreatedBy = company.Country.CreatedBy,
                LastUpdatedBy = company.Country.LastUpdatedBy,
                CreatedAt = company.Country.CreatedAt,
                UpdatedAt = company.Country.UpdatedAt
            } : null
        };
    }

    public async Task<CompanyResponseDto> CreateAsync(CreateCompanyDto createCompanyDto, int? createdBy = null)
    {
        // Validate country exists
        var countryExists = await _context.Countries
            .AnyAsync(c => c.Id == createCompanyDto.CountryId && c.DeletedAt == null);
        
        if (!countryExists)
            throw new ArgumentException($"Country with ID {createCompanyDto.CountryId} does not exist.");

        // Check if company code is unique within the country (if provided)
        if (!string.IsNullOrEmpty(createCompanyDto.CompanyCode))
        {
            var codeExists = await ExistsByCodeAndCountryAsync(createCompanyDto.CompanyCode, createCompanyDto.CountryId);
            if (codeExists)
                throw new ArgumentException($"Company code '{createCompanyDto.CompanyCode}' already exists in this country.");
        }

        var company = new Company
        {
            Name = createCompanyDto.Name,
            CompanyCode = createCompanyDto.CompanyCode,
            CountryId = createCompanyDto.CountryId,
            Description = createCompanyDto.Description,
            Metadata = createCompanyDto.Metadata,
            CreatedBy = createdBy,
            LastUpdatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Companies.Add(company);
        await _context.SaveChangesAsync();

        // Return the created company with country information
        return await GetByIdAsync(company.Id) ?? throw new InvalidOperationException("Failed to retrieve created company.");
    }

    public async Task<CompanyResponseDto?> UpdateAsync(int id, UpdateCompanyDto updateCompanyDto, int? updatedBy = null)
    {
        var company = await _context.Companies
            .Where(c => c.Id == id && c.DeletedAt == null)
            .FirstOrDefaultAsync();

        if (company == null)
            return null;

        // Validate country exists if countryId is being updated
        if (updateCompanyDto.CountryId.HasValue)
        {
            var countryExists = await _context.Countries
                .AnyAsync(c => c.Id == updateCompanyDto.CountryId.Value && c.DeletedAt == null);
            
            if (!countryExists)
                throw new ArgumentException($"Country with ID {updateCompanyDto.CountryId.Value} does not exist.");
        }

        // Check if company code is unique within the country (if provided and changed)
        if (!string.IsNullOrEmpty(updateCompanyDto.CompanyCode))
        {
            var countryId = updateCompanyDto.CountryId ?? company.CountryId;
            var codeExists = await ExistsByCodeAndCountryAsync(updateCompanyDto.CompanyCode, countryId, id);
            if (codeExists)
                throw new ArgumentException($"Company code '{updateCompanyDto.CompanyCode}' already exists in this country.");
        }

        // Update fields
        if (!string.IsNullOrEmpty(updateCompanyDto.Name))
            company.Name = updateCompanyDto.Name;
            
        if (updateCompanyDto.CompanyCode != null)
            company.CompanyCode = updateCompanyDto.CompanyCode;
            
        if (updateCompanyDto.CountryId.HasValue)
            company.CountryId = updateCompanyDto.CountryId.Value;
            
        if (updateCompanyDto.Description != null)
            company.Description = updateCompanyDto.Description;
            
        if (updateCompanyDto.Metadata != null)
            company.Metadata = updateCompanyDto.Metadata;

        company.LastUpdatedBy = updatedBy;
        company.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return await GetByIdAsync(id);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var company = await _context.Companies
            .Where(c => c.Id == id && c.DeletedAt == null)
            .FirstOrDefaultAsync();

        if (company == null)
            return false;

        // Soft delete
        company.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Companies
            .AnyAsync(c => c.Id == id && c.DeletedAt == null);
    }

    public async Task<bool> ExistsByCodeAndCountryAsync(string companyCode, int countryId, int? excludeId = null)
    {
        var query = _context.Companies
            .Where(c => c.CompanyCode == companyCode && c.CountryId == countryId && c.DeletedAt == null);

        if (excludeId.HasValue)
            query = query.Where(c => c.Id != excludeId.Value);

        return await query.AnyAsync();
    }

    public async Task<IEnumerable<CompanyListItemDto>> GetByCountryIdAsync(int countryId)
    {
        return await _context.Companies
            .Where(c => c.CountryId == countryId && c.DeletedAt == null)
            .Include(c => c.Country)
            .Select(c => new CompanyListItemDto
            {
                Id = c.Id,
                Name = c.Name,
                CompanyCode = c.CompanyCode,
                CountryId = c.CountryId,
                CountryName = c.Country.Name,
                Description = c.Description,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            })
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<CompanyListItemDto>> SearchAsync(string searchTerm)
    {
        var normalizedSearchTerm = searchTerm.ToLower().Trim();
        
        return await _context.Companies
            .Where(c => c.DeletedAt == null && 
                       (c.Name.ToLower().Contains(normalizedSearchTerm) ||
                        (c.CompanyCode != null && c.CompanyCode.ToLower().Contains(normalizedSearchTerm)) ||
                        (c.Description != null && c.Description.ToLower().Contains(normalizedSearchTerm))))
            .Include(c => c.Country)
            .Select(c => new CompanyListItemDto
            {
                Id = c.Id,
                Name = c.Name,
                CompanyCode = c.CompanyCode,
                CountryId = c.CountryId,
                CountryName = c.Country.Name,
                Description = c.Description,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            })
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    // Region relationship methods - regions now belong directly to companies
    public async Task<IEnumerable<RegionListItemDto>> GetRegionsByCompanyIdAsync(int companyId)
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
}