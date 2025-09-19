using DepotDirectApi.Models.DTOs;
using DepotDirectApi.Models.Entities;

namespace DepotDirectApi.Repositories;

public interface IUserRepository
{
    Task<IEnumerable<UserDto>> GetAllAsync();
    Task<UserDto?> GetByIdAsync(int id);
    Task<UserDto?> GetByEmailAsync(string email);
    Task<IEnumerable<UserDto>> GetByCompanyIdAsync(int companyId);
    Task<IEnumerable<UserDto>> GetByRoleIdAsync(int roleId);
    Task<User> CreateAsync(CreateUserDto createUserDto);
    Task<User?> UpdateAsync(int id, UpdateUserDto updateUserDto);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<bool> ExistsByEmailAsync(string email);
}