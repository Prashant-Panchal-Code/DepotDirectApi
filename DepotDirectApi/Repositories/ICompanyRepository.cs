using DepotDirectApi.Models.DTOs;
using DepotDirectApi.Models.Entities;

namespace DepotDirectApi.Repositories;

public interface ICompanyRepository
{
    Task<IEnumerable<CompanyListItemDto>> GetAllAsync();
    Task<CompanyResponseDto?> GetByIdAsync(int id);
    Task<CompanyResponseDto> CreateAsync(CreateCompanyDto createCompanyDto, int? createdBy = null);
    Task<CompanyResponseDto?> UpdateAsync(int id, UpdateCompanyDto updateCompanyDto, int? updatedBy = null);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<bool> ExistsByCodeAndCountryAsync(string companyCode, int countryId, int? excludeId = null);
    Task<IEnumerable<CompanyListItemDto>> GetByCountryIdAsync(int countryId);
    Task<IEnumerable<CompanyListItemDto>> SearchAsync(string searchTerm);
}