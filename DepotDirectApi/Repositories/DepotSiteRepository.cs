using DepotDirectApi.Data;
using DepotDirectApi.Models.DTOs;
using DepotDirectApi.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace DepotDirectApi.Repositories;

public class DepotSiteRepository : IDepotSiteRepository
{
    private readonly DepotDirectDbContext _context;

    public DepotSiteRepository(DepotDirectDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<DepotSiteListItemDto>> GetAllAsync()
    {
        return await _context.DepotSites
            .Where(ds => ds.DeletedAt == null)
            .Include(ds => ds.Depot)
                .ThenInclude(d => d.Company)
            .Include(ds => ds.Site)
                .ThenInclude(s => s.Company)
            .Select(ds => new DepotSiteListItemDto
            {
                Id = ds.Id,
                DepotId = ds.DepotId,
                DepotCode = ds.Depot.DepotCode,
                DepotName = ds.Depot.DepotName,
                SiteId = ds.SiteId,
                SiteCode = ds.Site.SiteCode,
                SiteName = ds.Site.SiteName,
                DistanceKm = ds.DistanceKm,
                TravelTimeMins = ds.TravelTimeMins,
                ReturnTimeMins = ds.ReturnTimeMins,
                Active = ds.Active,
                IsPrimary = ds.IsPrimary,
                TransportRate = ds.TransportRate,
                CreatedAt = ds.CreatedAt,
                UpdatedAt = ds.UpdatedAt
            })
            .OrderBy(ds => ds.DepotName)
            .ThenBy(ds => ds.SiteName)
            .ToListAsync();
    }

    public async Task<DepotSiteResponseDto?> GetByIdAsync(int id)
    {
        var depotSite = await _context.DepotSites
            .Where(ds => ds.Id == id && ds.DeletedAt == null)
            .Include(ds => ds.Depot)
                .ThenInclude(d => d.Company)
            .Include(ds => ds.Site)
                .ThenInclude(s => s.Company)
            .FirstOrDefaultAsync();

        if (depotSite == null)
            return null;

        return new DepotSiteResponseDto
        {
            Id = depotSite.Id,
            DepotId = depotSite.DepotId,
            SiteId = depotSite.SiteId,
            DistanceKm = depotSite.DistanceKm,
            TravelTimeMins = depotSite.TravelTimeMins,
            ReturnTimeMins = depotSite.ReturnTimeMins,
            Active = depotSite.Active,
            IsPrimary = depotSite.IsPrimary,
            TransportRate = depotSite.TransportRate,
            Metadata = depotSite.Metadata,
            CreatedBy = depotSite.CreatedBy,
            CreatedAt = depotSite.CreatedAt,
            UpdatedAt = depotSite.UpdatedAt,
            DeletedAt = depotSite.DeletedAt,
            Depot = new DepotSiteDepotDto
            {
                Id = depotSite.Depot.Id,
                DepotCode = depotSite.Depot.DepotCode,
                DepotName = depotSite.Depot.DepotName,
                Town = depotSite.Depot.Town,
                Active = depotSite.Depot.Active,
                Priority = depotSite.Depot.Priority,
                CompanyId = depotSite.Depot.CompanyId,
                CompanyName = depotSite.Depot.Company.Name
            },
            Site = new DepotSiteSiteDto
            {
                Id = depotSite.Site.Id,
                SiteCode = depotSite.Site.SiteCode,
                SiteName = depotSite.Site.SiteName,
                Town = depotSite.Site.Town,
                Active = depotSite.Site.Active,
                Priority = depotSite.Site.Priority,
                CompanyId = depotSite.Site.CompanyId,
                CompanyName = depotSite.Site.Company.Name
            }
        };
    }

    public async Task<DepotSiteResponseDto> CreateAsync(CreateDepotSiteDto createDepotSiteDto, int? createdBy = null)
    {
        // Validate depot exists and is active
        var depot = await _context.Depots
            .Where(d => d.Id == createDepotSiteDto.DepotId && d.DeletedAt == null)
            .Include(d => d.Company)
            .FirstOrDefaultAsync();

        if (depot == null)
            throw new ArgumentException($"Depot with ID {createDepotSiteDto.DepotId} does not exist.");

        // Validate site exists and is active
        var site = await _context.Sites
            .Where(s => s.Id == createDepotSiteDto.SiteId && s.DeletedAt == null)
            .Include(s => s.Company)
            .FirstOrDefaultAsync();

        if (site == null)
            throw new ArgumentException($"Site with ID {createDepotSiteDto.SiteId} does not exist.");

        // Validate depot and site belong to same company
        if (depot.CompanyId != site.CompanyId)
            throw new ArgumentException("Depot and Site must belong to the same company.");

        // Check if mapping already exists (including soft-deleted ones)
        var existingRoute = await _context.DepotSites
            .Where(ds => ds.DepotId == createDepotSiteDto.DepotId && ds.SiteId == createDepotSiteDto.SiteId)
            .FirstOrDefaultAsync();

        if (existingRoute != null)
        {
            if (existingRoute.DeletedAt == null)
            {
                // Route exists and is active
                throw new ArgumentException($"Route from Depot {createDepotSiteDto.DepotId} to Site {createDepotSiteDto.SiteId} already exists.");
            }
            else
            {
                // Route exists but is soft-deleted, reactivate it with new data
                existingRoute.DistanceKm = createDepotSiteDto.DistanceKm;
                existingRoute.TravelTimeMins = createDepotSiteDto.TravelTimeMins;
                existingRoute.ReturnTimeMins = createDepotSiteDto.ReturnTimeMins;
                existingRoute.Active = createDepotSiteDto.Active;
                existingRoute.IsPrimary = createDepotSiteDto.IsPrimary;
                existingRoute.TransportRate = createDepotSiteDto.TransportRate;
                existingRoute.Metadata = createDepotSiteDto.Metadata;
                existingRoute.CreatedBy = createdBy;
                existingRoute.DeletedAt = null; // Remove soft delete
                existingRoute.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return await GetByIdAsync(existingRoute.Id) ?? throw new InvalidOperationException("Failed to retrieve reactivated depot-site route.");
            }
        }

        // Create new route if none exists
        var depotSite = new DepotSite
        {
            DepotId = createDepotSiteDto.DepotId,
            SiteId = createDepotSiteDto.SiteId,
            DistanceKm = createDepotSiteDto.DistanceKm,
            TravelTimeMins = createDepotSiteDto.TravelTimeMins,
            ReturnTimeMins = createDepotSiteDto.ReturnTimeMins,
            Active = createDepotSiteDto.Active,
            IsPrimary = createDepotSiteDto.IsPrimary,
            TransportRate = createDepotSiteDto.TransportRate,
            Metadata = createDepotSiteDto.Metadata,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.DepotSites.Add(depotSite);
        await _context.SaveChangesAsync();

        return await GetByIdAsync(depotSite.Id) ?? throw new InvalidOperationException("Failed to retrieve created depot-site route.");
    }

    public async Task<DepotSiteResponseDto?> UpdateAsync(int id, UpdateDepotSiteDto updateDepotSiteDto, int? updatedBy = null)
    {
        var depotSite = await _context.DepotSites
            .Where(ds => ds.Id == id && ds.DeletedAt == null)
            .FirstOrDefaultAsync();

        if (depotSite == null)
            return null;

        // Update fields if provided
        if (updateDepotSiteDto.DistanceKm.HasValue)
            depotSite.DistanceKm = updateDepotSiteDto.DistanceKm.Value;
        
        if (updateDepotSiteDto.TravelTimeMins.HasValue)
            depotSite.TravelTimeMins = updateDepotSiteDto.TravelTimeMins.Value;
        
        if (updateDepotSiteDto.ReturnTimeMins.HasValue)
            depotSite.ReturnTimeMins = updateDepotSiteDto.ReturnTimeMins.Value;
        
        if (updateDepotSiteDto.Active.HasValue)
            depotSite.Active = updateDepotSiteDto.Active.Value;
        
        if (updateDepotSiteDto.IsPrimary.HasValue)
            depotSite.IsPrimary = updateDepotSiteDto.IsPrimary.Value;
        
        if (updateDepotSiteDto.TransportRate.HasValue)
            depotSite.TransportRate = updateDepotSiteDto.TransportRate.Value;
        
        if (updateDepotSiteDto.Metadata != null)
            depotSite.Metadata = updateDepotSiteDto.Metadata;

        depotSite.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return await GetByIdAsync(id);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var depotSite = await _context.DepotSites
            .Where(ds => ds.Id == id && ds.DeletedAt == null)
            .FirstOrDefaultAsync();

        if (depotSite == null)
            return false;

        depotSite.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.DepotSites.AnyAsync(ds => ds.Id == id && ds.DeletedAt == null);
    }

    public async Task<bool> ExistsByDepotAndSiteAsync(int depotId, int siteId, int? excludeId = null)
    {
        var query = _context.DepotSites
            .Where(ds => ds.DepotId == depotId && ds.SiteId == siteId && ds.DeletedAt == null);

        if (excludeId.HasValue)
            query = query.Where(ds => ds.Id != excludeId.Value);

        return await query.AnyAsync();
    }

    /// <summary>
    /// Check if depot-site route exists
    /// </summary>
    /// <param name="depotId">Depot ID</param>
    /// <param name="siteId">Site ID</param>
    /// <param name="excludeId">Optional ID to exclude from check</param>
    /// <param name="includeDeleted">Whether to include soft-deleted records</param>
    /// <returns>True if route exists</returns>
    public async Task<bool> ExistsByDepotAndSiteAsync(int depotId, int siteId, int? excludeId = null, bool includeDeleted = false)
    {
        var query = _context.DepotSites
            .Where(ds => ds.DepotId == depotId && ds.SiteId == siteId);

        if (!includeDeleted)
            query = query.Where(ds => ds.DeletedAt == null);

        if (excludeId.HasValue)
            query = query.Where(ds => ds.Id != excludeId.Value);

        return await query.AnyAsync();
    }

    public async Task<IEnumerable<DepotSiteListItemDto>> GetByDepotIdAsync(int depotId)
    {
        return await _context.DepotSites
            .Where(ds => ds.DepotId == depotId && ds.DeletedAt == null)
            .Include(ds => ds.Depot)
                .ThenInclude(d => d.Company)
            .Include(ds => ds.Site)
                .ThenInclude(s => s.Company)
            .Select(ds => new DepotSiteListItemDto
            {
                Id = ds.Id,
                DepotId = ds.DepotId,
                DepotCode = ds.Depot.DepotCode,
                DepotName = ds.Depot.DepotName,
                SiteId = ds.SiteId,
                SiteCode = ds.Site.SiteCode,
                SiteName = ds.Site.SiteName,
                DistanceKm = ds.DistanceKm,
                TravelTimeMins = ds.TravelTimeMins,
                ReturnTimeMins = ds.ReturnTimeMins,
                Active = ds.Active,
                IsPrimary = ds.IsPrimary,
                TransportRate = ds.TransportRate,
                CreatedAt = ds.CreatedAt,
                UpdatedAt = ds.UpdatedAt
            })
            .OrderBy(ds => ds.SiteName)
            .ToListAsync();
    }

    public async Task<IEnumerable<DepotSiteListItemDto>> GetBySiteIdAsync(int siteId)
    {
        return await _context.DepotSites
            .Where(ds => ds.SiteId == siteId && ds.DeletedAt == null)
            .Include(ds => ds.Depot)
                .ThenInclude(d => d.Company)
            .Include(ds => ds.Site)
                .ThenInclude(s => s.Company)
            .Select(ds => new DepotSiteListItemDto
            {
                Id = ds.Id,
                DepotId = ds.DepotId,
                DepotCode = ds.Depot.DepotCode,
                DepotName = ds.Depot.DepotName,
                SiteId = ds.SiteId,
                SiteCode = ds.Site.SiteCode,
                SiteName = ds.Site.SiteName,
                DistanceKm = ds.DistanceKm,
                TravelTimeMins = ds.TravelTimeMins,
                ReturnTimeMins = ds.ReturnTimeMins,
                Active = ds.Active,
                IsPrimary = ds.IsPrimary,
                TransportRate = ds.TransportRate,
                CreatedAt = ds.CreatedAt,
                UpdatedAt = ds.UpdatedAt
            })
            .OrderBy(ds => ds.DepotName)
            .ToListAsync();
    }

    public async Task<IEnumerable<DepotSiteListItemDto>> GetActiveRoutesAsync()
    {
        return await _context.DepotSites
            .Where(ds => ds.DeletedAt == null && ds.Active)
            .Include(ds => ds.Depot)
                .ThenInclude(d => d.Company)
            .Include(ds => ds.Site)
                .ThenInclude(s => s.Company)
            .Select(ds => new DepotSiteListItemDto
            {
                Id = ds.Id,
                DepotId = ds.DepotId,
                DepotCode = ds.Depot.DepotCode,
                DepotName = ds.Depot.DepotName,
                SiteId = ds.SiteId,
                SiteCode = ds.Site.SiteCode,
                SiteName = ds.Site.SiteName,
                DistanceKm = ds.DistanceKm,
                TravelTimeMins = ds.TravelTimeMins,
                ReturnTimeMins = ds.ReturnTimeMins,
                Active = ds.Active,
                IsPrimary = ds.IsPrimary,
                TransportRate = ds.TransportRate,
                CreatedAt = ds.CreatedAt,
                UpdatedAt = ds.UpdatedAt
            })
            .OrderBy(ds => ds.DepotName)
            .ThenBy(ds => ds.SiteName)
            .ToListAsync();
    }

    public async Task<IEnumerable<DepotSiteListItemDto>> GetPrimaryRoutesAsync()
    {
        return await _context.DepotSites
            .Where(ds => ds.DeletedAt == null && ds.IsPrimary)
            .Include(ds => ds.Depot)
                .ThenInclude(d => d.Company)
            .Include(ds => ds.Site)
                .ThenInclude(s => s.Company)
            .Select(ds => new DepotSiteListItemDto
            {
                Id = ds.Id,
                DepotId = ds.DepotId,
                DepotCode = ds.Depot.DepotCode,
                DepotName = ds.Depot.DepotName,
                SiteId = ds.SiteId,
                SiteCode = ds.Site.SiteCode,
                SiteName = ds.Site.SiteName,
                DistanceKm = ds.DistanceKm,
                TravelTimeMins = ds.TravelTimeMins,
                ReturnTimeMins = ds.ReturnTimeMins,
                Active = ds.Active,
                IsPrimary = ds.IsPrimary,
                TransportRate = ds.TransportRate,
                CreatedAt = ds.CreatedAt,
                UpdatedAt = ds.UpdatedAt
            })
            .OrderBy(ds => ds.SiteName)
            .ToListAsync();
    }

    public async Task<IEnumerable<DepotSiteListItemDto>> GetByCompanyIdAsync(int companyId)
    {
        return await _context.DepotSites
            .Where(ds => ds.DeletedAt == null && ds.Depot.CompanyId == companyId)
            .Include(ds => ds.Depot)
                .ThenInclude(d => d.Company)
            .Include(ds => ds.Site)
                .ThenInclude(s => s.Company)
            .Select(ds => new DepotSiteListItemDto
            {
                Id = ds.Id,
                DepotId = ds.DepotId,
                DepotCode = ds.Depot.DepotCode,
                DepotName = ds.Depot.DepotName,
                SiteId = ds.SiteId,
                SiteCode = ds.Site.SiteCode,
                SiteName = ds.Site.SiteName,
                DistanceKm = ds.DistanceKm,
                TravelTimeMins = ds.TravelTimeMins,
                ReturnTimeMins = ds.ReturnTimeMins,
                Active = ds.Active,
                IsPrimary = ds.IsPrimary,
                TransportRate = ds.TransportRate,
                CreatedAt = ds.CreatedAt,
                UpdatedAt = ds.UpdatedAt
            })
            .OrderBy(ds => ds.DepotName)
            .ThenBy(ds => ds.SiteName)
            .ToListAsync();
    }

    public async Task<IEnumerable<DepotSiteListItemDto>> SearchAsync(string searchTerm)
    {
        var normalized = searchTerm.ToLower().Trim();

        return await _context.DepotSites
            .Where(ds => ds.DeletedAt == null &&
                        (ds.Depot.DepotCode.ToLower().Contains(normalized) ||
                         ds.Depot.DepotName.ToLower().Contains(normalized) ||
                         ds.Site.SiteCode.ToLower().Contains(normalized) ||
                         ds.Site.SiteName.ToLower().Contains(normalized)))
            .Include(ds => ds.Depot)
                .ThenInclude(d => d.Company)
            .Include(ds => ds.Site)
                .ThenInclude(s => s.Company)
            .Select(ds => new DepotSiteListItemDto
            {
                Id = ds.Id,
                DepotId = ds.DepotId,
                DepotCode = ds.Depot.DepotCode,
                DepotName = ds.Depot.DepotName,
                SiteId = ds.SiteId,
                SiteCode = ds.Site.SiteCode,
                SiteName = ds.Site.SiteName,
                DistanceKm = ds.DistanceKm,
                TravelTimeMins = ds.TravelTimeMins,
                ReturnTimeMins = ds.ReturnTimeMins,
                Active = ds.Active,
                IsPrimary = ds.IsPrimary,
                TransportRate = ds.TransportRate,
                CreatedAt = ds.CreatedAt,
                UpdatedAt = ds.UpdatedAt
            })
            .OrderBy(ds => ds.DepotName)
            .ThenBy(ds => ds.SiteName)
            .ToListAsync();
    }

    public async Task<DepotSiteResponseDto?> SetPrimaryDepotForSiteAsync(int siteId, int depotId, int? updatedBy = null)
    {
        // First, find the depot-site route
        var targetRoute = await _context.DepotSites
            .Where(ds => ds.SiteId == siteId && ds.DepotId == depotId && ds.DeletedAt == null)
            .FirstOrDefaultAsync();

        if (targetRoute == null)
            throw new ArgumentException($"Route from Depot {depotId} to Site {siteId} does not exist.");

        // Clear any existing primary flags for this site
        var existingPrimaries = await _context.DepotSites
            .Where(ds => ds.SiteId == siteId && ds.IsPrimary && ds.DeletedAt == null)
            .ToListAsync();

        foreach (var route in existingPrimaries)
        {
            route.IsPrimary = false;
            route.UpdatedAt = DateTime.UtcNow;
        }

        // Set the target route as primary
        targetRoute.IsPrimary = true;
        targetRoute.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return await GetByIdAsync(targetRoute.Id);
    }
}