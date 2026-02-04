using DepotDirectApi.Models.DTOs;

namespace DepotDirectApi.Repositories;

public interface IDriverRepository
{
    Task<IEnumerable<DriverListItemDto>> GetAllAsync();
    Task<DriverResponseDto?> GetByIdAsync(int id);
    Task<DriverResponseDto> CreateAsync(CreateDriverDto createDriverDto, int? createdBy = null);
    Task<DriverResponseDto?> UpdateAsync(int id, UpdateDriverDto updateDriverDto, int? updatedBy = null);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<bool> ExistsByDriverCodeAndCompanyAsync(string driverCode, int companyId, int? excludeId = null);
    Task<bool> ExistsByLicenseNumberAndCompanyAsync(string licenseNumber, int companyId, int? excludeId = null);
    Task<IEnumerable<DriverListItemDto>> GetByCompanyIdAsync(int companyId);
    Task<IEnumerable<DriverListItemDto>> GetByRegionIdAsync(int regionId);
    Task<IEnumerable<DriverListItemDto>> GetByHomeDepotIdAsync(int homeDepotId);
    Task<IEnumerable<DriverListItemDto>> GetByBreakRuleIdAsync(int breakRuleId);
    Task<IEnumerable<DriverListItemDto>> GetByStatusAsync(string status);
    Task<IEnumerable<DriverListItemDto>> SearchAsync(string searchTerm);
    
    // Driver availability
    Task<IEnumerable<DriverListItemDto>> GetAvailableDriversAsync(DateTime startDate, DateTime endDate);
    Task<bool> IsDriverAvailableAsync(int driverId, DateTime startDate, DateTime endDate);
}

public interface IDriverShiftRepository
{
    Task<IEnumerable<DriverShiftResponseDto>> GetAllAsync();
    Task<DriverShiftResponseDto?> GetByIdAsync(int id);
    Task<DriverShiftResponseDto> CreateAsync(CreateDriverShiftDto createDriverShiftDto, int? createdBy = null);
    Task<DriverShiftResponseDto?> UpdateAsync(int id, UpdateDriverShiftDto updateDriverShiftDto, int? updatedBy = null);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<IEnumerable<DriverShiftResponseDto>> GetByDriverIdAsync(int driverId);
    Task<IEnumerable<DriverShiftResponseDto>> GetByDepotIdAsync(int depotId);
    Task<IEnumerable<DriverShiftResponseDto>> GetByDayOfWeekAsync(int dayOfWeek);
}

public interface IDriverTimeOffRepository
{
    Task<IEnumerable<DriverTimeOffResponseDto>> GetAllAsync();
    Task<DriverTimeOffResponseDto?> GetByIdAsync(int id);
    Task<DriverTimeOffResponseDto> CreateAsync(CreateDriverTimeOffDto createDriverTimeOffDto, int? createdBy = null);
    Task<DriverTimeOffResponseDto?> UpdateAsync(int id, UpdateDriverTimeOffDto updateDriverTimeOffDto, int? updatedBy = null);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<IEnumerable<DriverTimeOffResponseDto>> GetByDriverIdAsync(int driverId);
    Task<IEnumerable<DriverTimeOffResponseDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<bool> HasConflictingTimeOffAsync(int driverId, DateTime startDate, DateTime endDate, int? excludeId = null);
}