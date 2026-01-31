using DepotDirectApi.Models.DTOs;

namespace DepotDirectApi.Repositories;

public interface ITankRepository
{
    Task<SiteTankDto> CreateTankAsync(CreateTankDto dto, int? createdBy = null);
    Task<SiteTankDto?> UpdateTankAsync(int tankId, UpdateTankDto dto, int? updatedBy = null);
    Task<bool> DeleteTankAsync(int tankId);
    Task<IEnumerable<SiteTankDto>> GetTanksBySiteAsync(int siteId);
    Task<SiteTankWithInventoryDto?> GetTankWithInventoryAsync(int tankId);
    Task<SiteTankFullDto?> GetTankFullDetailsAsync(int tankId);
    Task<TankReadingDto> CreateTankReadingAsync(int tankId, CreateTankReadingDto dto, int? createdBy = null);
    Task<IEnumerable<SiteTankFullDto>> GetTanksFullBySiteAsync(int siteId);
}
