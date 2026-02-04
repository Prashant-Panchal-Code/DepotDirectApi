using DepotDirectApi.Models.DTOs;

namespace DepotDirectApi.Repositories;

public interface ITractorRepository
{
    Task<IEnumerable<TractorListItemDto>> GetAllAsync();
    Task<TractorResponseDto?> GetByIdAsync(int id);
    Task<TractorResponseDto> CreateAsync(CreateTractorDto createTractorDto, int? createdBy = null);
    Task<TractorResponseDto?> UpdateAsync(int id, UpdateTractorDto updateTractorDto, int? updatedBy = null);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<bool> ExistsByTractorCodeAndHaulierAsync(string tractorCode, int haulierId, int? excludeId = null);
    Task<bool> ExistsByLicensePlateAsync(string licensePlate, int? excludeId = null);
    Task<IEnumerable<TractorListItemDto>> GetByHaulierIdAsync(int haulierId);
    Task<IEnumerable<TractorListItemDto>> GetByRegionIdAsync(int regionId);
    Task<IEnumerable<TractorListItemDto>> GetByStatusAsync(string status);
    Task<IEnumerable<TractorListItemDto>> GetWithPumpAsync();
    Task<IEnumerable<TractorListItemDto>> SearchAsync(string searchTerm);
    
    // Availability
    Task<IEnumerable<TractorListItemDto>> GetAvailableTractorsAsync(DateTime startDate, DateTime endDate);
    Task<bool> IsTractorAvailableAsync(int tractorId, DateTime startDate, DateTime endDate);
}

public interface ITrailerRepository
{
    Task<IEnumerable<TrailerListItemDto>> GetAllAsync();
    Task<TrailerResponseDto?> GetByIdAsync(int id);
    Task<TrailerResponseDto> CreateAsync(CreateTrailerDto createTrailerDto, int? createdBy = null);
    Task<TrailerResponseDto?> UpdateAsync(int id, UpdateTrailerDto updateTrailerDto, int? updatedBy = null);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<bool> ExistsByTrailerCodeAndHaulierAsync(string trailerCode, int haulierId, int? excludeId = null);
    Task<bool> ExistsByLicensePlateAsync(string licensePlate, int? excludeId = null);
    Task<IEnumerable<TrailerListItemDto>> GetByHaulierIdAsync(int haulierId);
    Task<IEnumerable<TrailerListItemDto>> GetByRegionIdAsync(int regionId);
    Task<IEnumerable<TrailerListItemDto>> GetByStatusAsync(string status);
    Task<IEnumerable<TrailerListItemDto>> SearchAsync(string searchTerm);
    
    // Availability
    Task<IEnumerable<TrailerListItemDto>> GetAvailableTrailersAsync(DateTime startDate, DateTime endDate);
    Task<bool> IsTrailerAvailableAsync(int trailerId, DateTime startDate, DateTime endDate);
}

public interface ITrailerCompartmentRepository
{
    Task<IEnumerable<TrailerCompartmentResponseDto>> GetAllAsync();
    Task<TrailerCompartmentResponseDto?> GetByIdAsync(int id);
    Task<TrailerCompartmentResponseDto> CreateAsync(CreateTrailerCompartmentDto createTrailerCompartmentDto, int? createdBy = null);
    Task<TrailerCompartmentResponseDto?> UpdateAsync(int id, UpdateTrailerCompartmentDto updateTrailerCompartmentDto, int? updatedBy = null);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<IEnumerable<TrailerCompartmentResponseDto>> GetByTrailerIdAsync(int trailerId);
    Task<bool> ExistsByTrailerAndCompartmentNumberAsync(int trailerId, int compartmentNumber, int? excludeId = null);
    
    // Product assignments
    Task<CompartmentAllowedProductResponseDto> AssignProductToCompartmentAsync(int compartmentId, int productId, int? createdBy = null);
    Task<bool> RemoveProductFromCompartmentAsync(int compartmentId, int productId);
    Task<bool> IsProductAllowedInCompartmentAsync(int compartmentId, int productId);
    Task<IEnumerable<ProductListItemDto>> GetAllowedProductsAsync(int compartmentId);
    Task<IEnumerable<TrailerCompartmentResponseDto>> GetCompartmentsForProductAsync(int productId);
}