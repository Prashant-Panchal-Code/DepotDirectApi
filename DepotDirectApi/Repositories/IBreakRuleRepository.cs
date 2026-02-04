using DepotDirectApi.Models.DTOs;

namespace DepotDirectApi.Repositories;

public interface IBreakRuleRepository
{
    Task<IEnumerable<BreakRuleListItemDto>> GetAllAsync();
    Task<BreakRuleResponseDto?> GetByIdAsync(int id);
    Task<BreakRuleResponseDto> CreateAsync(CreateBreakRuleDto createBreakRuleDto, int? createdBy = null);
    Task<BreakRuleResponseDto?> UpdateAsync(int id, UpdateBreakRuleDto updateBreakRuleDto, int? updatedBy = null);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<bool> ExistsByRuleNameAndCompanyAsync(string ruleName, int companyId, int? excludeId = null);
    Task<IEnumerable<BreakRuleListItemDto>> GetByCompanyIdAsync(int companyId);
    Task<IEnumerable<BreakRuleListItemDto>> SearchAsync(string searchTerm);
}