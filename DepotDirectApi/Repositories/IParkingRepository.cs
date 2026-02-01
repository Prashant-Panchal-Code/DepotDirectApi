using DepotDirectApi.Models.DTOs;

namespace DepotDirectApi.Repositories;

public interface IParkingRepository
{
    Task<IEnumerable<ParkingListItemDto>> GetAllAsync();
    Task<ParkingResponseDto?> GetByIdAsync(int id);
    Task<ParkingResponseDto> CreateAsync(CreateParkingDto createParkingDto, int? createdBy = null);
    Task<ParkingResponseDto?> UpdateAsync(int id, UpdateParkingDto updateParkingDto, int? updatedBy = null);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<bool> ExistsByParkingCodeAndCountryAsync(string parkingCode, int countryId, int? excludeId = null);
    Task<IEnumerable<ParkingListItemDto>> GetByCompanyIdAsync(int companyId);
    Task<IEnumerable<ParkingListItemDto>> GetByCountryIdAsync(int countryId);
    Task<IEnumerable<ParkingListItemDto>> GetByRegionIdAsync(int regionId);
    Task<IEnumerable<ParkingListItemDto>> SearchAsync(string searchTerm);
    
    // Region-Parking mapping operations
    Task<RegionParkingDto> AssignParkingToRegionAsync(int parkingId, int regionId, string? parkingCode = null, int? createdBy = null);
    Task<bool> RemoveParkingFromRegionAsync(int parkingId, int regionId);
    Task<bool> IsParkingAssignedToRegionAsync(int parkingId, int regionId);
}