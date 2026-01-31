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

// New: IDepotRepository for Depot endpoints
public interface IDepotRepository
{
    Task<IEnumerable<DepotListItemDto>> GetAllAsync();
    Task<DepotResponseDto?> GetByIdAsync(int id);
    Task<DepotResponseDto> CreateAsync(CreateDepotDto createDepotDto, int? createdBy = null);
    Task<DepotResponseDto?> UpdateAsync(int id, UpdateDepotDto updateDepotDto, int? updatedBy = null);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<bool> ExistsByDepotCodeAndCountryAsync(string depotCode, int countryId, int? excludeId = null);

    Task<IEnumerable<DepotListItemDto>> GetByCompanyIdAsync(int companyId);
    Task<IEnumerable<DepotListItemDto>> GetByCountryIdAsync(int countryId);
    Task<IEnumerable<DepotListItemDto>> GetByRegionIdAsync(int regionId);
    Task<IEnumerable<DepotListItemDto>> SearchAsync(string searchTerm);

    // Region-Depot mapping operations
    Task<RegionDepotDto> AssignDepotToRegionAsync(int depotId, int regionId, string? depotCode = null, int? createdBy = null);
    Task<bool> RemoveDepotFromRegionAsync(int depotId, int regionId);
    Task<bool> IsDepotAssignedToRegionAsync(int depotId, int regionId);

    // Depot product operations
    Task<IEnumerable<DepotProductDto>> GetProductsByDepotIdAsync(int depotId);
    Task<DepotProductDto> CreateDepotProductAsync(int depotId, CreateDepotProductDto dto, int? createdBy = null);
    Task<DepotProductDto?> UpdateDepotProductAsync(int depotId, int id, UpdateDepotProductDto dto, int? updatedBy = null);
}
