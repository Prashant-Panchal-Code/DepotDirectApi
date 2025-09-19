using DepotDirectApi.Models.DTOs;
using DepotDirectApi.Models.Entities;

namespace DepotDirectApi.Repositories;

public interface IRoleRepository
{
    Task<IEnumerable<RoleDto>> GetAllAsync();
    Task<RoleDto?> GetByIdAsync(int id);
    Task<RoleDto?> GetByNameAsync(string name);
    Task<Role> CreateAsync(CreateRoleDto createRoleDto);
    Task<Role?> UpdateAsync(int id, UpdateRoleDto updateRoleDto);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<bool> ExistsByNameAsync(string name);
}