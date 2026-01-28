using DepotDirectApi.Data;
using DepotDirectApi.Models.DTOs;
using DepotDirectApi.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace DepotDirectApi.Repositories;

public class SiteRepository : ISiteRepository
{
    private readonly DepotDirectDbContext _context;

    public SiteRepository(DepotDirectDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<SiteListItemDto>> GetAllAsync()
    {
        return await _context.Sites
            .Where(s => s.DeletedAt == null)
            .Include(s => s.Company)
            .Include(s => s.Country)
            .Select(s => new SiteListItemDto
            {
                Id = s.Id,
                SiteCode = s.SiteCode,
                SiteName = s.SiteName,
                Town = s.Town,
                Active = s.Active,
                Priority = s.Priority,
                CompanyId = s.CompanyId,
                CompanyName = s.Company.Name,
                CountryId = s.CountryId,
                CountryName = s.Country.Name,
                CreatedAt = s.CreatedAt,
                UpdatedAt = s.UpdatedAt
            })
            .OrderBy(s => s.SiteName)
            .ToListAsync();
    }

    public async Task<SiteResponseDto?> GetByIdAsync(int id)
    {
        var site = await _context.Sites
            .Where(s => s.Id == id && s.DeletedAt == null)
            .Include(s => s.Company)
            .Include(s => s.Country)
            .Include(s => s.RegionSites)
            .ThenInclude(rs => rs.Region)
            .FirstOrDefaultAsync();

        if (site == null)
            return null;

        return new SiteResponseDto
        {
            Id = site.Id,
            SiteCode = site.SiteCode,
            SiteName = site.SiteName,
            Shortcode = site.Shortcode,
            Latitude = site.Latitude,
            Longitude = site.Longitude,
            LatLong = site.LatLong,
            Street = site.Street,
            PostalCode = site.PostalCode,
            Town = site.Town,
            Active = site.Active,
            Priority = site.Priority,
            ContactPerson = site.ContactPerson,
            Phone = site.Phone,
            Email = site.Email,
            OperatingHours = site.OperatingHours,
            DepotId = site.DepotId,
            DeliveryStopped = site.DeliveryStopped,
            PumpedRequired = site.PumpedRequired,
            CountryId = site.CountryId,
            CompanyId = site.CompanyId,
            Metadata = site.Metadata,
            CreatedBy = site.CreatedBy,
            LastUpdatedBy = site.LastUpdatedBy,
            CreatedAt = site.CreatedAt,
            UpdatedAt = site.UpdatedAt,
            DeletedAt = site.DeletedAt,
            Country = site.Country != null ? new CountryDto
            {
                Id = site.Country.Id,
                Name = site.Country.Name,
                IsoCode = site.Country.IsoCode,
                Metadata = site.Country.Metadata,
                CreatedBy = site.Country.CreatedBy,
                LastUpdatedBy = site.Country.LastUpdatedBy,
                CreatedAt = site.Country.CreatedAt,
                UpdatedAt = site.Country.UpdatedAt
            } : null,
            Company = site.Company != null ? new CompanyDto
            {
                Id = site.Company.Id,
                Name = site.Company.Name,
                CompanyCode = site.Company.CompanyCode,
                CountryId = site.Company.CountryId,
                Description = site.Company.Description,
                Metadata = site.Company.Metadata,
                CreatedBy = site.Company.CreatedBy,
                LastUpdatedBy = site.Company.LastUpdatedBy,
                CreatedAt = site.Company.CreatedAt,
                UpdatedAt = site.Company.UpdatedAt
            } : null,
            Regions = site.RegionSites
                .Where(rs => rs.DeletedAt == null)
                .Select(rs => new RegionDto
                {
                    Id = rs.Region.Id,
                    Name = rs.Region.Name,
                    RegionCode = rs.Region.RegionCode,
                    CompanyId = rs.Region.CompanyId,
                    Metadata = rs.Region.Metadata,
                    CreatedBy = rs.Region.CreatedBy,
                    LastUpdatedBy = rs.Region.LastUpdatedBy,
                    CreatedAt = rs.Region.CreatedAt,
                    UpdatedAt = rs.Region.UpdatedAt
                })
                .ToList()
        };
    }

    public async Task<SiteResponseDto> CreateAsync(CreateSiteDto createSiteDto, int? createdBy = null)
    {
        // Validate region exists and get company_id and country_id from region
        var region = await _context.Regions
            .Where(r => r.Id == createSiteDto.RegionId && r.DeletedAt == null)
            .Include(r => r.Company)
            .FirstOrDefaultAsync();

        if (region == null)
            throw new ArgumentException($"Region with ID {createSiteDto.RegionId} does not exist.");

        var companyId = region.CompanyId;
        var countryId = region.Company.CountryId;

        // Check if site code is unique within the country
        var codeExists = await ExistsBySiteCodeAndCountryAsync(createSiteDto.SiteCode, countryId);
        if (codeExists)
            throw new ArgumentException($"Site code '{createSiteDto.SiteCode}' already exists in this country.");

        // Validate that region's company matches the company we're about to use
        if (region.Company == null)
            throw new InvalidOperationException($"Region {createSiteDto.RegionId} has no associated company.");

        var site = new Site
        {
            SiteCode = createSiteDto.SiteCode,
            SiteName = createSiteDto.SiteName,
            CountryId = countryId,
            CompanyId = companyId,
            CreatedBy = createdBy,
            LastUpdatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Sites.Add(site);
        await _context.SaveChangesAsync();

        // Create the region-site mapping
        var regionSite = new RegionSite
        {
            SiteId = site.Id,
            RegionId = createSiteDto.RegionId,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.RegionSites.Add(regionSite);
        await _context.SaveChangesAsync();

        // Return the created site with full details
        return await GetByIdAsync(site.Id) ?? throw new InvalidOperationException("Failed to retrieve created site.");
    }

    public async Task<SiteResponseDto?> UpdateAsync(int id, UpdateSiteDto updateSiteDto, int? updatedBy = null)
    {
        var site = await _context.Sites
            .Where(s => s.Id == id && s.DeletedAt == null)
            .FirstOrDefaultAsync();

        if (site == null)
            return null;

        // Check if site code is unique within the country (if provided and changed)
        if (!string.IsNullOrEmpty(updateSiteDto.SiteCode) && updateSiteDto.SiteCode != site.SiteCode)
        {
            var codeExists = await ExistsBySiteCodeAndCountryAsync(updateSiteDto.SiteCode, site.CountryId, id);
            if (codeExists)
                throw new ArgumentException($"Site code '{updateSiteDto.SiteCode}' already exists in this country.");
        }

        // Update fields
        if (!string.IsNullOrEmpty(updateSiteDto.SiteCode))
            site.SiteCode = updateSiteDto.SiteCode;

        if (!string.IsNullOrEmpty(updateSiteDto.SiteName))
            site.SiteName = updateSiteDto.SiteName;

        if (updateSiteDto.Shortcode != null)
            site.Shortcode = updateSiteDto.Shortcode;

        if (updateSiteDto.Latitude.HasValue)
            site.Latitude = updateSiteDto.Latitude;

        if (updateSiteDto.Longitude.HasValue)
            site.Longitude = updateSiteDto.Longitude;

        if (updateSiteDto.Street != null)
            site.Street = updateSiteDto.Street;

        if (updateSiteDto.PostalCode != null)
            site.PostalCode = updateSiteDto.PostalCode;

        if (updateSiteDto.Town != null)
            site.Town = updateSiteDto.Town;

        if (updateSiteDto.Active.HasValue)
            site.Active = updateSiteDto.Active.Value;

        if (!string.IsNullOrEmpty(updateSiteDto.Priority))
            site.Priority = updateSiteDto.Priority;

        if (updateSiteDto.ContactPerson != null)
            site.ContactPerson = updateSiteDto.ContactPerson;

        if (updateSiteDto.Phone != null)
            site.Phone = updateSiteDto.Phone;

        if (updateSiteDto.Email != null)
            site.Email = updateSiteDto.Email;

        if (updateSiteDto.OperatingHours != null)
            site.OperatingHours = updateSiteDto.OperatingHours;

        if (updateSiteDto.DepotId.HasValue)
            site.DepotId = updateSiteDto.DepotId;

        if (updateSiteDto.DeliveryStopped.HasValue)
            site.DeliveryStopped = updateSiteDto.DeliveryStopped.Value;

        if (updateSiteDto.PumpedRequired.HasValue)
            site.PumpedRequired = updateSiteDto.PumpedRequired.Value;

        if (updateSiteDto.Metadata != null)
            site.Metadata = updateSiteDto.Metadata;

        site.LastUpdatedBy = updatedBy;
        site.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return await GetByIdAsync(id);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var site = await _context.Sites
            .Where(s => s.Id == id && s.DeletedAt == null)
            .FirstOrDefaultAsync();

        if (site == null)
            return false;

        // Soft delete
        site.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Sites
            .AnyAsync(s => s.Id == id && s.DeletedAt == null);
    }

    public async Task<bool> ExistsBySiteCodeAndCountryAsync(string siteCode, int countryId, int? excludeId = null)
    {
        var query = _context.Sites
            .Where(s => s.SiteCode == siteCode && s.CountryId == countryId && s.DeletedAt == null);

        if (excludeId.HasValue)
            query = query.Where(s => s.Id != excludeId.Value);

        return await query.AnyAsync();
    }

    public async Task<IEnumerable<SiteListItemDto>> GetByCompanyIdAsync(int companyId)
    {
        return await _context.Sites
            .Where(s => s.CompanyId == companyId && s.DeletedAt == null)
            .Include(s => s.Company)
            .Include(s => s.Country)
            .Select(s => new SiteListItemDto
            {
                Id = s.Id,
                SiteCode = s.SiteCode,
                SiteName = s.SiteName,
                Town = s.Town,
                Active = s.Active,
                Priority = s.Priority,
                CompanyId = s.CompanyId,
                CompanyName = s.Company.Name,
                CountryId = s.CountryId,
                CountryName = s.Country.Name,
                CreatedAt = s.CreatedAt,
                UpdatedAt = s.UpdatedAt
            })
            .OrderBy(s => s.SiteName)
            .ToListAsync();
    }

    public async Task<IEnumerable<SiteListItemDto>> GetByCountryIdAsync(int countryId)
    {
        return await _context.Sites
            .Where(s => s.CountryId == countryId && s.DeletedAt == null)
            .Include(s => s.Company)
            .Include(s => s.Country)
            .Select(s => new SiteListItemDto
            {
                Id = s.Id,
                SiteCode = s.SiteCode,
                SiteName = s.SiteName,
                Town = s.Town,
                Active = s.Active,
                Priority = s.Priority,
                CompanyId = s.CompanyId,
                CompanyName = s.Company.Name,
                CountryId = s.CountryId,
                CountryName = s.Country.Name,
                CreatedAt = s.CreatedAt,
                UpdatedAt = s.UpdatedAt
            })
            .OrderBy(s => s.SiteName)
            .ToListAsync();
    }

    public async Task<IEnumerable<SiteListItemDto>> GetByRegionIdAsync(int regionId)
    {
        return await _context.RegionSites
            .Where(rs => rs.RegionId == regionId && rs.DeletedAt == null)
            .Include(rs => rs.Site)
            .ThenInclude(s => s.Company)
            .Include(rs => rs.Site)
            .ThenInclude(s => s.Country)
            .Where(rs => rs.Site.DeletedAt == null)
            .Select(rs => new SiteListItemDto
            {
                Id = rs.Site.Id,
                SiteCode = rs.Site.SiteCode,
                SiteName = rs.Site.SiteName,
                Town = rs.Site.Town,
                Active = rs.Site.Active,
                Priority = rs.Site.Priority,
                CompanyId = rs.Site.CompanyId,
                CompanyName = rs.Site.Company.Name,
                CountryId = rs.Site.CountryId,
                CountryName = rs.Site.Country.Name,
                CreatedAt = rs.Site.CreatedAt,
                UpdatedAt = rs.Site.UpdatedAt
            })
            .OrderBy(s => s.SiteName)
            .ToListAsync();
    }

    public async Task<IEnumerable<SiteListItemDto>> SearchAsync(string searchTerm)
    {
        var normalizedSearchTerm = searchTerm.ToLower().Trim();

        return await _context.Sites
            .Where(s => s.DeletedAt == null &&
                       (s.SiteCode.ToLower().Contains(normalizedSearchTerm) ||
                        s.SiteName.ToLower().Contains(normalizedSearchTerm) ||
                        (s.Town != null && s.Town.ToLower().Contains(normalizedSearchTerm))))
            .Include(s => s.Company)
            .Include(s => s.Country)
            .Select(s => new SiteListItemDto
            {
                Id = s.Id,
                SiteCode = s.SiteCode,
                SiteName = s.SiteName,
                Town = s.Town,
                Active = s.Active,
                Priority = s.Priority,
                CompanyId = s.CompanyId,
                CompanyName = s.Company.Name,
                CountryId = s.CountryId,
                CountryName = s.Country.Name,
                CreatedAt = s.CreatedAt,
                UpdatedAt = s.UpdatedAt
            })
            .OrderBy(s => s.SiteName)
            .ToListAsync();
    }

    public async Task<RegionSiteDto> AssignSiteToRegionAsync(int siteId, int regionId, string? siteCode = null, int? createdBy = null)
    {
        // Validate site exists
        var siteExists = await ExistsAsync(siteId);
        if (!siteExists)
            throw new ArgumentException($"Site with ID {siteId} does not exist.");

        // Validate region exists
        var region = await _context.Regions
            .Where(r => r.Id == regionId && r.DeletedAt == null)
            .FirstOrDefaultAsync();

        if (region == null)
            throw new ArgumentException($"Region with ID {regionId} does not exist.");

        // Check if already assigned
        var alreadyAssigned = await IsSiteAssignedToRegionAsync(siteId, regionId);
        if (alreadyAssigned)
            throw new ArgumentException($"Site {siteId} is already assigned to Region {regionId}.");

        // Validate that site and region belong to the same company
        var site = await _context.Sites.FindAsync(siteId);
        if (site!.CompanyId != region.CompanyId)
            throw new ArgumentException($"Site and Region must belong to the same company.");

        var regionSite = new RegionSite
        {
            SiteId = siteId,
            RegionId = regionId,
            SiteCode = siteCode,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.RegionSites.Add(regionSite);
        await _context.SaveChangesAsync();

        // Return the created mapping
        var result = await _context.RegionSites
            .Where(rs => rs.Id == regionSite.Id)
            .Include(rs => rs.Site)
            .Include(rs => rs.Region)
            .Select(rs => new RegionSiteDto
            {
                Id = rs.Id,
                SiteId = rs.SiteId,
                SiteName = rs.Site.SiteName,
                SiteCode = rs.Site.SiteCode,
                RegionId = rs.RegionId,
                RegionName = rs.Region.Name,
                RegionSiteCode = rs.SiteCode,
                Metadata = rs.Metadata,
                CreatedBy = rs.CreatedBy,
                CreatedAt = rs.CreatedAt,
                UpdatedAt = rs.UpdatedAt
            })
            .FirstOrDefaultAsync();

        return result ?? throw new InvalidOperationException("Failed to retrieve created region-site mapping.");
    }

    public async Task<bool> RemoveSiteFromRegionAsync(int siteId, int regionId)
    {
        var regionSite = await _context.RegionSites
            .Where(rs => rs.SiteId == siteId && rs.RegionId == regionId && rs.DeletedAt == null)
            .FirstOrDefaultAsync();

        if (regionSite == null)
            return false;

        // Soft delete
        regionSite.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> IsSiteAssignedToRegionAsync(int siteId, int regionId)
    {
        return await _context.RegionSites
            .AnyAsync(rs => rs.SiteId == siteId && rs.RegionId == regionId && rs.DeletedAt == null);
    }
}
