using DepotDirectApi.Models.DTOs;

namespace DepotDirectApi.Repositories;

public interface IProductRepository
{
    Task<IEnumerable<ProductListItemDto>> GetByRegionIdAsync(int regionId);
}
