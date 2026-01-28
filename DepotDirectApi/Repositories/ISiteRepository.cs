using DepotDirectApi.Models.DTOs;

namespace DepotDirectApi.Repositories;

public interface ISiteRepository
{
    Task<IEnumerable<SiteListItemDto>> GetAllAsync();
    Task<SiteResponseDto?> GetByIdAsync(int id);
    Task<SiteResponseDto> CreateAsync(CreateSiteDto createSiteDto, int? createdBy = null);
    Task<SiteResponseDto?> UpdateAsync(int id, UpdateSiteDto updateSiteDto, int? updatedBy = null);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<bool> ExistsBySiteCodeAndCountryAsync(string siteCode, int countryId, int? excludeId = null);
    Task<IEnumerable<SiteListItemDto>> GetByCompanyIdAsync(int companyId);
    Task<IEnumerable<SiteListItemDto>> GetByCountryIdAsync(int countryId);
    Task<IEnumerable<SiteListItemDto>> GetByRegionIdAsync(int regionId);
    Task<IEnumerable<SiteListItemDto>> SearchAsync(string searchTerm);
    
    // Region-Site mapping operations
    Task<RegionSiteDto> AssignSiteToRegionAsync(int siteId, int regionId, string? siteCode = null, int? createdBy = null);
    Task<bool> RemoveSiteFromRegionAsync(int siteId, int regionId);
    Task<bool> IsSiteAssignedToRegionAsync(int siteId, int regionId);
}
