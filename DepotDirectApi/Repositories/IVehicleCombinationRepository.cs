using DepotDirectApi.Models.DTOs;

namespace DepotDirectApi.Repositories;

public interface IVehicleCombinationRepository
{
    Task<IEnumerable<VehicleCombinationListItemDto>> GetAllAsync();
    Task<VehicleCombinationResponseDto?> GetByIdAsync(int id);
    Task<VehicleCombinationResponseDto> CreateAsync(CreateVehicleCombinationDto createVehicleCombinationDto, int? createdBy = null);
    Task<VehicleCombinationResponseDto?> UpdateAsync(int id, UpdateVehicleCombinationDto updateVehicleCombinationDto, int? updatedBy = null);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<bool> ExistsByCombinationCodeAndTractorAsync(string combinationCode, int tractorId, int? excludeId = null);
    Task<IEnumerable<VehicleCombinationListItemDto>> GetByTractorIdAsync(int tractorId);
    Task<IEnumerable<VehicleCombinationListItemDto>> SearchAsync(string searchTerm);
    
    // Trailer management
    Task<VehicleCombinationTrailerResponseDto> AddTrailerToCombinationAsync(int combinationId, AddTrailerToCombinationDto addTrailerDto, int? createdBy = null);
    Task<bool> RemoveTrailerFromCombinationAsync(int combinationId, int trailerId);
    Task<bool> IsTrailerInCombinationAsync(int combinationId, int trailerId);
    Task<IEnumerable<TrailerListItemDto>> GetTrailersInCombinationAsync(int combinationId);
    Task<IEnumerable<VehicleCombinationListItemDto>> GetCombinationsWithTrailerAsync(int trailerId);
    
    // Default combination management
    Task<VehicleCombinationResponseDto?> GetDefaultCombinationForTractorAsync(int tractorId);
    Task<bool> SetDefaultCombinationAsync(int combinationId, int? updatedBy = null);
    Task<bool> RemoveDefaultCombinationAsync(int tractorId, int? updatedBy = null);
}

public interface ITractorScheduleRepository
{
    Task<IEnumerable<TractorScheduleListItemDto>> GetAllAsync();
    Task<TractorScheduleResponseDto?> GetByIdAsync(int id);
    Task<TractorScheduleResponseDto> CreateAsync(CreateTractorScheduleDto createTractorScheduleDto, int? createdBy = null);
    Task<TractorScheduleResponseDto?> UpdateAsync(int id, UpdateTractorScheduleDto updateTractorScheduleDto, int? updatedBy = null);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<IEnumerable<TractorScheduleListItemDto>> GetByTractorIdAsync(int tractorId);
    Task<IEnumerable<TractorScheduleListItemDto>> GetByDriverIdAsync(int driverId);
    Task<IEnumerable<TractorScheduleListItemDto>> GetByDayOfWeekAsync(int dayOfWeek);
    Task<IEnumerable<TractorScheduleListItemDto>> GetByDepotIdAsync(int depotId);
    Task<IEnumerable<TractorScheduleListItemDto>> GetByParkingIdAsync(int parkingId);
    Task<IEnumerable<TractorScheduleListItemDto>> GetSchedulesForDateRangeAsync(DateTime startDate, DateTime endDate);
    
    // Conflict checking
    Task<bool> HasScheduleConflictAsync(int tractorId, int dayOfWeek, TimeSpan startTime, TimeSpan endTime, int? excludeId = null);
    Task<bool> HasDriverConflictAsync(int driverId, int dayOfWeek, TimeSpan startTime, TimeSpan endTime, int? excludeId = null);
    
    // Availability
    Task<IEnumerable<TractorScheduleListItemDto>> GetAvailableSchedulesAsync(int dayOfWeek, TimeSpan startTime, TimeSpan endTime);
    Task<IEnumerable<DriverListItemDto>> GetAvailableDriversForScheduleAsync(int dayOfWeek, TimeSpan startTime, TimeSpan endTime);
    Task<IEnumerable<TractorListItemDto>> GetAvailableTractorsForScheduleAsync(int dayOfWeek, TimeSpan startTime, TimeSpan endTime);
}