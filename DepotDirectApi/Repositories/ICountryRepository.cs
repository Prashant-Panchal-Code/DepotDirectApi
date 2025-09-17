using DepotDirectApi.Models.DTOs;
using DepotDirectApi.Models.Entities;

namespace DepotDirectApi.Repositories;

public interface ICountryRepository
{
    Task<PagedResult<Country>> GetAllAsync(int page = 1, int pageSize = 50, string? search = null);
    Task<Country?> GetByIdAsync(int id);
    Task<Country> CreateAsync(Country country);
    Task<Country?> UpdateAsync(int id, Country country);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<bool> ExistsByNameAsync(string name, int? excludeId = null);
    Task<bool> ExistsByIsoCodeAsync(string isoCode, int? excludeId = null);
    Task<CountryWithStatsDto?> GetWithStatsAsync(int id);
}