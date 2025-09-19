using DepotDirectApi.Models.DTOs;

namespace DepotDirectApi.Repositories;

public interface IRegionRepository
{
    Task<IEnumerable<RegionListItemDto>> GetAllAsync();
    Task<RegionResponseDto?> GetByIdAsync(int id);
    Task<RegionResponseDto> CreateAsync(CreateRegionDto createRegionDto, int? createdBy = null);
    Task<RegionResponseDto?> UpdateAsync(int id, UpdateRegionDto updateRegionDto, int? updatedBy = null);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<bool> ExistsByCodeAndCompanyAsync(string regionCode, int companyId, int? excludeId = null);
    Task<IEnumerable<RegionListItemDto>> GetByCompanyIdAsync(int companyId);
    Task<IEnumerable<RegionListItemDto>> SearchAsync(string searchTerm);
}