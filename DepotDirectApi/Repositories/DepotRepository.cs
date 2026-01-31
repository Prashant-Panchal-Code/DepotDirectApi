using DepotDirectApi.Data;
using DepotDirectApi.Models.DTOs;
using DepotDirectApi.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace DepotDirectApi.Repositories;

public class DepotRepository : IDepotRepository
{
    private readonly DepotDirectDbContext _context;

    public DepotRepository(DepotDirectDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<DepotListItemDto>> GetAllAsync()
    {
        return await _context.Depots
            .Where(d => d.DeletedAt == null)
            .Include(d => d.Company)
            .Include(d => d.Country)
            .Select(d => new DepotListItemDto
            {
                Id = d.Id,
                DepotCode = d.DepotCode,
                DepotName = d.DepotName,
                Town = d.Town,
                Active = d.Active,
                Priority = d.Priority,
                CompanyId = d.CompanyId,
                CompanyName = d.Company.Name,
                CountryId = d.CountryId,
                CountryName = d.Country.Name,
                Latitude = d.Latitude,
                Longitude = d.Longitude,
                LatLong = d.LatLong,
                Street = d.Street,
                PostalCode = d.PostalCode,
                CreatedAt = d.CreatedAt,
                UpdatedAt = d.UpdatedAt
            })
            .OrderBy(d => d.DepotName)
            .ToListAsync();
    }

    public async Task<DepotResponseDto?> GetByIdAsync(int id)
    {
        var depot = await _context.Depots
            .Where(d => d.Id == id && d.DeletedAt == null)
            .Include(d => d.Company)
            .Include(d => d.Country)
            .Include(d => d.RegionDepots)
            .ThenInclude(rd => rd.Region)
            .FirstOrDefaultAsync();

        if (depot == null)
            return null;

        return new DepotResponseDto
        {
            Id = depot.Id,
            DepotCode = depot.DepotCode,
            DepotName = depot.DepotName,
            Shortcode = depot.Shortcode,
            Latitude = depot.Latitude,
            Longitude = depot.Longitude,
            LatLong = depot.LatLong,
            Street = depot.Street,
            PostalCode = depot.PostalCode,
            Town = depot.Town,
            Active = depot.Active,
            Priority = depot.Priority,
            LoadingBays = depot.LoadingBays,
            OperatingHours = depot.OperatingHours,
            ManagerName = depot.ManagerName,
            ManagerPhone = depot.ManagerPhone,
            ManagerEmail = depot.ManagerEmail,
            EmergencyContact = depot.EmergencyContact,
            AverageLoadingTime = depot.AverageLoadingTime,
            MaxTruckSize = depot.MaxTruckSize,
            Certifications = depot.Certifications,
            CountryId = depot.CountryId,
            CompanyId = depot.CompanyId,
            Metadata = depot.Metadata,
            CreatedBy = depot.CreatedBy,
            LastUpdatedBy = depot.LastUpdatedBy,
            CreatedAt = depot.CreatedAt,
            UpdatedAt = depot.UpdatedAt,
            DeletedAt = depot.DeletedAt,
            Country = depot.Country != null ? new CountryDto
            {
                Id = depot.Country.Id,
                Name = depot.Country.Name,
                IsoCode = depot.Country.IsoCode,
                Metadata = depot.Country.Metadata,
                CreatedBy = depot.Country.CreatedBy,
                LastUpdatedBy = depot.Country.LastUpdatedBy,
                CreatedAt = depot.Country.CreatedAt,
                UpdatedAt = depot.Country.UpdatedAt
            } : null,
            Company = depot.Company != null ? new CompanyDto
            {
                Id = depot.Company.Id,
                Name = depot.Company.Name,
                CompanyCode = depot.Company.CompanyCode,
                CountryId = depot.Company.CountryId,
                Description = depot.Company.Description,
                CreatedAt = depot.Company.CreatedAt,
                UpdatedAt = depot.Company.UpdatedAt,
                CreatedBy = depot.Company.CreatedBy,
                LastUpdatedBy = depot.Company.LastUpdatedBy
            } : null,
            Regions = depot.RegionDepots
                .Where(rd => rd.DeletedAt == null)
                .Select(rd => new RegionDto
                {
                    Id = rd.Region.Id,
                    Name = rd.Region.Name,
                    RegionCode = rd.Region.RegionCode,
                    CompanyId = rd.Region.CompanyId,
                    Metadata = rd.Region.Metadata,
                    CreatedBy = rd.Region.CreatedBy,
                    LastUpdatedBy = rd.Region.LastUpdatedBy,
                    CreatedAt = rd.Region.CreatedAt,
                    UpdatedAt = rd.Region.UpdatedAt
                })
                .ToList()
        };
    }

    public async Task<DepotResponseDto> CreateAsync(CreateDepotDto createDepotDto, int? createdBy = null)
    {
        // Validate region exists and get company_id and country_id from region
        var region = await _context.Regions
            .Where(r => r.Id == createDepotDto.RegionId && r.DeletedAt == null)
            .Include(r => r.Company)
            .FirstOrDefaultAsync();

        if (region == null)
            throw new ArgumentException($"Region with ID {createDepotDto.RegionId} does not exist.");

        var companyId = region.CompanyId;
        var countryId = region.Company.CountryId;

        // Check if depot code is unique within the country
        var codeExists = await ExistsByDepotCodeAndCountryAsync(createDepotDto.DepotCode, countryId);
        if (codeExists)
            throw new ArgumentException($"Depot code '{createDepotDto.DepotCode}' already exists in this country.");

        if (region.Company == null)
            throw new InvalidOperationException($"Region {createDepotDto.RegionId} has no associated company.");

        var depot = new Depot
        {
            DepotCode = createDepotDto.DepotCode,
            DepotName = createDepotDto.DepotName,
            CountryId = countryId,
            CompanyId = companyId,
            CreatedBy = createdBy,
            LastUpdatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Depots.Add(depot);
        await _context.SaveChangesAsync();

        // Create the region-depot mapping
        var regionDepot = new RegionDepot
        {
            DepotId = depot.Id,
            RegionId = createDepotDto.RegionId,
            DepotCode = createDepotDto.DepotCode,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.RegionDepots.Add(regionDepot);
        await _context.SaveChangesAsync();

        return await GetByIdAsync(depot.Id) ?? throw new InvalidOperationException("Failed to retrieve created depot.");
    }

    public async Task<DepotResponseDto?> UpdateAsync(int id, UpdateDepotDto updateDepotDto, int? updatedBy = null)
    {
        var depot = await _context.Depots
            .Where(d => d.Id == id && d.DeletedAt == null)
            .FirstOrDefaultAsync();

        if (depot == null)
            return null;

        // Check unique code
        if (!string.IsNullOrEmpty(updateDepotDto.DepotCode) && updateDepotDto.DepotCode != depot.DepotCode)
        {
            var codeExists = await ExistsByDepotCodeAndCountryAsync(updateDepotDto.DepotCode, depot.CountryId, id);
            if (codeExists)
                throw new ArgumentException($"Depot code '{updateDepotDto.DepotCode}' already exists in this country.");
        }

        if (!string.IsNullOrEmpty(updateDepotDto.DepotCode)) depot.DepotCode = updateDepotDto.DepotCode;
        if (!string.IsNullOrEmpty(updateDepotDto.DepotName)) depot.DepotName = updateDepotDto.DepotName;
        if (updateDepotDto.Shortcode != null) depot.Shortcode = updateDepotDto.Shortcode;
        if (updateDepotDto.Latitude.HasValue) depot.Latitude = updateDepotDto.Latitude;
        if (updateDepotDto.Longitude.HasValue) depot.Longitude = updateDepotDto.Longitude;
        if (updateDepotDto.Street != null) depot.Street = updateDepotDto.Street;
        if (updateDepotDto.PostalCode != null) depot.PostalCode = updateDepotDto.PostalCode;
        if (updateDepotDto.Town != null) depot.Town = updateDepotDto.Town;
        if (updateDepotDto.Active.HasValue) depot.Active = updateDepotDto.Active.Value;
        if (!string.IsNullOrEmpty(updateDepotDto.Priority)) depot.Priority = updateDepotDto.Priority;
        if (updateDepotDto.LoadingBays.HasValue) depot.LoadingBays = updateDepotDto.LoadingBays;
        if (updateDepotDto.OperatingHours != null) depot.OperatingHours = updateDepotDto.OperatingHours;
        if (updateDepotDto.ManagerName != null) depot.ManagerName = updateDepotDto.ManagerName;
        if (updateDepotDto.ManagerPhone != null) depot.ManagerPhone = updateDepotDto.ManagerPhone;
        if (updateDepotDto.ManagerEmail != null) depot.ManagerEmail = updateDepotDto.ManagerEmail;
        if (updateDepotDto.EmergencyContact != null) depot.EmergencyContact = updateDepotDto.EmergencyContact;
        if (updateDepotDto.AverageLoadingTime.HasValue) depot.AverageLoadingTime = updateDepotDto.AverageLoadingTime;
        if (updateDepotDto.MaxTruckSize != null) depot.MaxTruckSize = updateDepotDto.MaxTruckSize;
        if (updateDepotDto.Certifications != null) depot.Certifications = updateDepotDto.Certifications;
        if (updateDepotDto.Metadata != null) depot.Metadata = updateDepotDto.Metadata;

        depot.LastUpdatedBy = updatedBy;
        depot.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return await GetByIdAsync(id);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var depot = await _context.Depots
            .Where(d => d.Id == id && d.DeletedAt == null)
            .FirstOrDefaultAsync();

        if (depot == null)
            return false;

        depot.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Depots.AnyAsync(d => d.Id == id && d.DeletedAt == null);
    }

    public async Task<bool> ExistsByDepotCodeAndCountryAsync(string depotCode, int countryId, int? excludeId = null)
    {
        var query = _context.Depots
            .Where(d => d.DepotCode == depotCode && d.CountryId == countryId && d.DeletedAt == null);

        if (excludeId.HasValue)
            query = query.Where(d => d.Id != excludeId.Value);

        return await query.AnyAsync();
    }

    public async Task<IEnumerable<DepotListItemDto>> GetByCompanyIdAsync(int companyId)
    {
        return await _context.Depots
            .Where(d => d.CompanyId == companyId && d.DeletedAt == null)
            .Include(d => d.Company)
            .Include(d => d.Country)
            .Select(d => new DepotListItemDto
            {
                Id = d.Id,
                DepotCode = d.DepotCode,
                DepotName = d.DepotName,
                Town = d.Town,
                Active = d.Active,
                Priority = d.Priority,
                CompanyId = d.CompanyId,
                CompanyName = d.Company.Name,
                CountryId = d.CountryId,
                CountryName = d.Country.Name,
                Latitude = d.Latitude,
                Longitude = d.Longitude,
                LatLong = d.LatLong,
                Street = d.Street,
                PostalCode = d.PostalCode,
                CreatedAt = d.CreatedAt,
                UpdatedAt = d.UpdatedAt
            })
            .OrderBy(d => d.DepotName)
            .ToListAsync();
    }

    public async Task<IEnumerable<DepotListItemDto>> GetByCountryIdAsync(int countryId)
    {
        return await _context.Depots
            .Where(d => d.CountryId == countryId && d.DeletedAt == null)
            .Include(d => d.Company)
            .Include(d => d.Country)
            .Select(d => new DepotListItemDto
            {
                Id = d.Id,
                DepotCode = d.DepotCode,
                DepotName = d.DepotName,
                Town = d.Town,
                Active = d.Active,
                Priority = d.Priority,
                CompanyId = d.CompanyId,
                CompanyName = d.Company.Name,
                CountryId = d.CountryId,
                CountryName = d.Country.Name,
                Latitude = d.Latitude,
                Longitude = d.Longitude,
                LatLong = d.LatLong,
                Street = d.Street,
                PostalCode = d.PostalCode,
                CreatedAt = d.CreatedAt,
                UpdatedAt = d.UpdatedAt
            })
            .OrderBy(d => d.DepotName)
            .ToListAsync();
    }

    public async Task<IEnumerable<DepotListItemDto>> GetByRegionIdAsync(int regionId)
    {
        return await _context.RegionDepots
            .Where(rd => rd.RegionId == regionId && rd.DeletedAt == null)
            .Include(rd => rd.Depot)
            .ThenInclude(d => d.Company)
            .Include(rd => rd.Depot)
            .ThenInclude(d => d.Country)
            .Where(rd => rd.Depot.DeletedAt == null)
            .Select(rd => new DepotListItemDto
            {
                Id = rd.Depot.Id,
                DepotCode = rd.Depot.DepotCode,
                DepotName = rd.Depot.DepotName,
                Town = rd.Depot.Town,
                Active = rd.Depot.Active,
                Priority = rd.Depot.Priority,
                CompanyId = rd.Depot.CompanyId,
                CompanyName = rd.Depot.Company.Name,
                CountryId = rd.Depot.CountryId,
                CountryName = rd.Depot.Country.Name,
                Latitude = rd.Depot.Latitude,
                Longitude = rd.Depot.Longitude,
                LatLong = rd.Depot.LatLong,
                Street = rd.Depot.Street,
                PostalCode = rd.Depot.PostalCode,
                CreatedAt = rd.Depot.CreatedAt,
                UpdatedAt = rd.Depot.UpdatedAt
            })
            .OrderBy(d => d.DepotName)
            .ToListAsync();
    }

    public async Task<IEnumerable<DepotListItemDto>> SearchAsync(string searchTerm)
    {
        var normalized = searchTerm.ToLower().Trim();

        return await _context.Depots
            .Where(d => d.DeletedAt == null &&
                        (d.DepotCode.ToLower().Contains(normalized) ||
                         d.DepotName.ToLower().Contains(normalized) ||
                         (d.Town != null && d.Town.ToLower().Contains(normalized))))
            .Include(d => d.Company)
            .Include(d => d.Country)
            .Select(d => new DepotListItemDto
            {
                Id = d.Id,
                DepotCode = d.DepotCode,
                DepotName = d.DepotName,
                Town = d.Town,
                Active = d.Active,
                Priority = d.Priority,
                CompanyId = d.CompanyId,
                CompanyName = d.Company.Name,
                CountryId = d.CountryId,
                CountryName = d.Country.Name,
                Latitude = d.Latitude,
                Longitude = d.Longitude,
                LatLong = d.LatLong,
                Street = d.Street,
                PostalCode = d.PostalCode,
                CreatedAt = d.CreatedAt,
                UpdatedAt = d.UpdatedAt
            })
            .OrderBy(d => d.DepotName)
            .ToListAsync();
    }

    public async Task<RegionDepotDto> AssignDepotToRegionAsync(int depotId, int regionId, string? depotCode = null, int? createdBy = null)
    {
        var depotExists = await ExistsAsync(depotId);
        if (!depotExists)
            throw new ArgumentException($"Depot with ID {depotId} does not exist.");

        var region = await _context.Regions
            .Where(r => r.Id == regionId && r.DeletedAt == null)
            .FirstOrDefaultAsync();

        if (region == null)
            throw new ArgumentException($"Region with ID {regionId} does not exist.");

        var already = await IsDepotAssignedToRegionAsync(depotId, regionId);
        if (already)
            throw new ArgumentException($"Depot {depotId} is already assigned to Region {regionId}.");

        var depot = await _context.Depots.FindAsync(depotId);
        if (depot!.CompanyId != region.CompanyId)
            throw new ArgumentException("Depot and Region must belong to the same company.");

        var regionDepot = new RegionDepot
        {
            DepotId = depotId,
            RegionId = regionId,
            DepotCode = depotCode,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.RegionDepots.Add(regionDepot);
        await _context.SaveChangesAsync();

        var result = await _context.RegionDepots
            .Where(rd => rd.Id == regionDepot.Id)
            .Include(rd => rd.Depot)
            .Include(rd => rd.Region)
            .Select(rd => new RegionDepotDto
            {
                Id = rd.Id,
                DepotId = rd.DepotId,
                DepotName = rd.Depot.DepotName,
                DepotCode = rd.Depot.DepotCode,
                RegionId = rd.RegionId,
                RegionName = rd.Region.Name,
                RegionDepotCode = rd.DepotCode,
                Metadata = rd.Metadata,
                CreatedBy = rd.CreatedBy,
                CreatedAt = rd.CreatedAt,
                UpdatedAt = rd.UpdatedAt
            })
            .FirstOrDefaultAsync();

        return result ?? throw new InvalidOperationException("Failed to retrieve created region-depot mapping.");
    }

    public async Task<bool> RemoveDepotFromRegionAsync(int depotId, int regionId)
    {
        var rd = await _context.RegionDepots
            .Where(r => r.DepotId == depotId && r.RegionId == regionId && r.DeletedAt == null)
            .FirstOrDefaultAsync();

        if (rd == null)
            return false;

        rd.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> IsDepotAssignedToRegionAsync(int depotId, int regionId)
    {
        return await _context.RegionDepots
            .AnyAsync(r => r.DepotId == depotId && r.RegionId == regionId && r.DeletedAt == null);
    }
}
