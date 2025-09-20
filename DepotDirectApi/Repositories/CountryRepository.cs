using DepotDirectApi.Data;
using DepotDirectApi.Models.DTOs;
using DepotDirectApi.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace DepotDirectApi.Repositories;

public class CountryRepository : ICountryRepository
{
    private readonly DepotDirectDbContext _context;

    public CountryRepository(DepotDirectDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<Country>> GetAllAsync(int page = 1, int pageSize = 50, string? search = null)
    {
        var query = _context.Countries.Where(c => c.DeletedAt == null);

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(c => c.Name.ToLower().Contains(search.ToLower()) ||
                                   (c.IsoCode != null && c.IsoCode.ToLower().Contains(search.ToLower())));
        }

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        var countries = await query
            .OrderBy(c => c.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<Country>
        {
            Data = countries,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = totalPages,
            HasNextPage = page < totalPages,
            HasPreviousPage = page > 1
        };
    }

    public async Task<Country?> GetByIdAsync(int id)
    {
        return await _context.Countries
            .Where(c => c.Id == id && c.DeletedAt == null)
            .FirstOrDefaultAsync();
    }

    public async Task<Country> CreateAsync(Country country)
    {
        country.CreatedAt = DateTime.UtcNow;
        country.UpdatedAt = DateTime.UtcNow;
        
        _context.Countries.Add(country);
        await _context.SaveChangesAsync();
        return country;
    }

    public async Task<Country?> UpdateAsync(int id, Country country)
    {
        var existingCountry = await GetByIdAsync(id);
        if (existingCountry == null)
            return null;

        existingCountry.Name = country.Name;
        existingCountry.IsoCode = country.IsoCode;
        existingCountry.Metadata = country.Metadata;
        existingCountry.LastUpdatedBy = country.LastUpdatedBy;
        existingCountry.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return existingCountry;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var country = await GetByIdAsync(id);
        if (country == null)
            return false;

        // Soft delete
        country.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Countries
            .AnyAsync(c => c.Id == id && c.DeletedAt == null);
    }

    public async Task<bool> ExistsByNameAsync(string name, int? excludeId = null)
    {
        var query = _context.Countries.Where(c => c.Name.ToLower() == name.ToLower() && c.DeletedAt == null);
        
        if (excludeId.HasValue)
            query = query.Where(c => c.Id != excludeId.Value);

        return await query.AnyAsync();
    }

    public async Task<bool> ExistsByIsoCodeAsync(string isoCode, int? excludeId = null)
    {
        if (string.IsNullOrWhiteSpace(isoCode))
            return false;

        var query = _context.Countries.Where(c => c.IsoCode != null && 
                                                c.IsoCode.ToLower() == isoCode.ToLower() && 
                                                c.DeletedAt == null);
        
        if (excludeId.HasValue)
            query = query.Where(c => c.Id != excludeId.Value);

        return await query.AnyAsync();
    }

    public async Task<CountryWithStatsDto?> GetWithStatsAsync(int id)
    {
        var country = await _context.Countries
            .Where(c => c.Id == id && c.DeletedAt == null)
            .Select(c => new CountryWithStatsDto
            {
                Id = c.Id,
                Name = c.Name,
                IsoCode = c.IsoCode,
                Metadata = c.Metadata,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt,
                CreatedBy = c.CreatedBy,
                LastUpdatedBy = c.LastUpdatedBy,
                CompaniesCount = c.Companies.Count(comp => comp.DeletedAt == null),
                RegionsCount = c.Companies
                    .Where(comp => comp.DeletedAt == null)
                    .SelectMany(comp => comp.Regions)
                    .Count(region => region.DeletedAt == null)
            })
            .FirstOrDefaultAsync();

        return country;
    }

    public async Task<List<CountryWithStatsDto>> GetAllWithStatsAsync()
    {
        var countries = await _context.Countries
            .Where(c => c.DeletedAt == null)
            .Include(c => c.Companies.Where(co => co.DeletedAt == null))
                .ThenInclude(co => co.Regions.Where(r => r.DeletedAt == null))
            .OrderBy(c => c.Name)
            .ToListAsync();

        var result = countries.Select(c => new CountryWithStatsDto
        {
            Id = c.Id,
            Name = c.Name,
            IsoCode = c.IsoCode,
            Metadata = c.Metadata != null ? JsonSerializer.Deserialize<object>(c.Metadata.RootElement.GetRawText()) : null,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt,
            CreatedBy = c.CreatedBy,
            LastUpdatedBy = c.LastUpdatedBy,
            CompaniesCount = c.Companies.Count,
            RegionsCount = c.Companies.SelectMany(comp => comp.Regions).Count()
        }).ToList();

        return result;
    }
}