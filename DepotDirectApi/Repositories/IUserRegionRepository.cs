using DepotDirectApi.Models.DTOs;
using DepotDirectApi.Models.Entities;

namespace DepotDirectApi.Repositories;

public interface IUserRegionRepository
{
    Task<UserRegion> AssignUserToRegionAsync(int userId, int regionId, int createdBy);
    Task<bool> RemoveUserFromRegionAsync(int userId, int regionId);
    Task<bool> IsUserAssignedToRegionAsync(int userId, int regionId);
    Task<List<RegionListItemDto>> GetUserRegionsAsync(int userId);
    Task<List<UserListItemDto>> GetRegionUsersAsync(int regionId);
    Task<UserWithRegionsDto?> GetUserWithRegionsAsync(int userId);
    Task<RegionWithUsersDto?> GetRegionWithUsersAsync(int regionId);
    Task<List<UserRegionDto>> GetAllUserRegionsAsync();
    Task<UserRegionDto?> GetUserRegionAsync(int userId, int regionId);
    Task<UserRegion?> UpdateUserRegionAsync(int userId, int regionId, UserRegionUpdateDto updateDto, int updatedBy);
    Task<bool> ValidateUserRegionAssignmentAsync(int userId, int regionId);
}