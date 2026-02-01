using DepotDirectApi.Models.DTOs;

namespace DepotDirectApi.Repositories;

public interface IDepotSiteRepository
{
    Task<IEnumerable<DepotSiteListItemDto>> GetAllAsync();
    Task<DepotSiteResponseDto?> GetByIdAsync(int id);
    Task<DepotSiteResponseDto> CreateAsync(CreateDepotSiteDto createDepotSiteDto, int? createdBy = null);
    Task<DepotSiteResponseDto?> UpdateAsync(int id, UpdateDepotSiteDto updateDepotSiteDto, int? updatedBy = null);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<bool> ExistsByDepotAndSiteAsync(int depotId, int siteId, int? excludeId = null, bool includeDeleted = false);
    
    // Get routes by depot
    Task<IEnumerable<DepotSiteListItemDto>> GetByDepotIdAsync(int depotId);
    
    // Get routes by site
    Task<IEnumerable<DepotSiteListItemDto>> GetBySiteIdAsync(int siteId);
    
    // Get active routes only
    Task<IEnumerable<DepotSiteListItemDto>> GetActiveRoutesAsync();
    
    // Get primary routes for sites
    Task<IEnumerable<DepotSiteListItemDto>> GetPrimaryRoutesAsync();
    
    // Get routes by company
    Task<IEnumerable<DepotSiteListItemDto>> GetByCompanyIdAsync(int companyId);
    
    // Search routes
    Task<IEnumerable<DepotSiteListItemDto>> SearchAsync(string searchTerm);
    
    // Set primary depot for a site (ensures only one depot is primary per site)
    Task<DepotSiteResponseDto?> SetPrimaryDepotForSiteAsync(int siteId, int depotId, int? updatedBy = null);
}