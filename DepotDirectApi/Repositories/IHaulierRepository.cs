using DepotDirectApi.Models.DTOs;

namespace DepotDirectApi.Repositories;

public interface IHaulierRepository
{
    Task<IEnumerable<HaulierListItemDto>> GetAllAsync();
    Task<HaulierResponseDto?> GetByIdAsync(int id);
    Task<HaulierResponseDto> CreateAsync(CreateHaulierDto createHaulierDto, int? createdBy = null);
    Task<HaulierResponseDto?> UpdateAsync(int id, UpdateHaulierDto updateHaulierDto, int? updatedBy = null);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<bool> ExistsByHaulierCodeAndRegionAsync(string haulierCode, int regionId, int? excludeId = null);
    Task<IEnumerable<HaulierListItemDto>> GetByRegionIdAsync(int regionId);
    Task<IEnumerable<HaulierListItemDto>> SearchAsync(string searchTerm);
    Task<IEnumerable<HaulierListItemDto>> GetActiveHauliersAsync();
    Task<IEnumerable<HaulierListItemDto>> GetByContractExpiryDateAsync(DateTime fromDate, DateTime toDate);
}