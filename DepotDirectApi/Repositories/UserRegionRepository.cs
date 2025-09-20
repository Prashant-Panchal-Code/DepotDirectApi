using DepotDirectApi.Data;
using DepotDirectApi.Models.DTOs;
using DepotDirectApi.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace DepotDirectApi.Repositories;

public class UserRegionRepository : IUserRegionRepository
{
    private readonly DepotDirectDbContext _context;

    public UserRegionRepository(DepotDirectDbContext context)
    {
        _context = context;
    }

    public async Task<UserRegion> AssignUserToRegionAsync(int userId, int regionId, int createdBy)
    {
        // Validate that the assignment is allowed (user's company country matches region's country)
        if (!await ValidateUserRegionAssignmentAsync(userId, regionId))
        {
            throw new ArgumentException("User cannot be assigned to a region in a different country than their company");
        }

        // Check if assignment already exists
        if (await IsUserAssignedToRegionAsync(userId, regionId))
        {
            throw new ArgumentException("User is already assigned to this region");
        }

        var userRegion = new UserRegion
        {
            UserId = userId,
            RegionId = regionId,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.UserRegions.Add(userRegion);
        await _context.SaveChangesAsync();
        
        return userRegion;
    }

    public async Task<bool> RemoveUserFromRegionAsync(int userId, int regionId)
    {
        var userRegion = await _context.UserRegions
            .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RegionId == regionId);

        if (userRegion == null)
            return false;

        _context.UserRegions.Remove(userRegion);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> IsUserAssignedToRegionAsync(int userId, int regionId)
    {
        return await _context.UserRegions
            .AnyAsync(ur => ur.UserId == userId && ur.RegionId == regionId);
    }

    public async Task<List<RegionListItemDto>> GetUserRegionsAsync(int userId)
    {
        return await _context.UserRegions
            .Where(ur => ur.UserId == userId)
            .Include(ur => ur.Region)
            .ThenInclude(r => r.Company)
            .Select(ur => new RegionListItemDto
            {
                Id = ur.Region.Id,
                Name = ur.Region.Name,
                RegionCode = ur.Region.RegionCode,
                CompanyId = ur.Region.CompanyId,
                CompanyName = ur.Region.Company.Name,
                CreatedAt = ur.Region.CreatedAt,
                UpdatedAt = ur.Region.UpdatedAt
            })
            .ToListAsync();
    }

    public async Task<List<UserListItemDto>> GetRegionUsersAsync(int regionId)
    {
        return await _context.UserRegions
            .Where(ur => ur.RegionId == regionId)
            .Include(ur => ur.User)
            .Select(ur => new UserListItemDto
            {
                Id = ur.User.Id,
                FullName = ur.User.FullName,
                Email = ur.User.Email,
                Active = ur.User.Active
            })
            .ToListAsync();
    }

    public async Task<UserWithRegionsDto?> GetUserWithRegionsAsync(int userId)
    {
        var user = await _context.Users
            .Where(u => u.Id == userId && u.DeletedAt == null)
            .Include(u => u.Company)
            .Include(u => u.Role)
            .FirstOrDefaultAsync();

        if (user == null)
            return null;

        var regions = await GetUserRegionsAsync(userId);

        return new UserWithRegionsDto
        {
            Id = user.Id,
            CompanyId = user.CompanyId,
            CompanyName = user.Company?.Name,
            RoleId = user.RoleId,
            RoleName = user.Role.Name,
            Email = user.Email,
            FullName = user.FullName,
            Phone = user.Phone,
            Active = user.Active,
            Metadata = user.Metadata,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            CreatedBy = user.CreatedBy,
            LastUpdatedBy = user.LastUpdatedBy,
            Regions = regions
        };
    }

    public async Task<RegionWithUsersDto?> GetRegionWithUsersAsync(int regionId)
    {
        var region = await _context.Regions
            .Where(r => r.Id == regionId && r.DeletedAt == null)
            .Include(r => r.Company)
            .FirstOrDefaultAsync();

        if (region == null)
            return null;

        var users = await GetRegionUsersAsync(regionId);

        return new RegionWithUsersDto
        {
            Id = region.Id,
            Name = region.Name,
            RegionCode = region.RegionCode,
            CompanyId = region.CompanyId,
            Metadata = region.Metadata,
            CreatedAt = region.CreatedAt,
            UpdatedAt = region.UpdatedAt,
            CreatedBy = region.CreatedBy,
            LastUpdatedBy = region.LastUpdatedBy,
            Company = new CompanyDto
            {
                Id = region.Company.Id,
                Name = region.Company.Name,
                CountryId = region.Company.CountryId,
                CompanyCode = region.Company.CompanyCode,
                Description = region.Company.Description,
                CreatedAt = region.Company.CreatedAt,
                UpdatedAt = region.Company.UpdatedAt,
                CreatedBy = region.Company.CreatedBy,
                LastUpdatedBy = region.Company.LastUpdatedBy
            },
            Users = users
        };
    }

    public async Task<List<UserRegionDto>> GetAllUserRegionsAsync()
    {
        var userRegions = await _context.UserRegions
            .Include(ur => ur.User)
            .Include(ur => ur.Region)
            .ThenInclude(r => r.Company)
            .ToListAsync();

        return userRegions.Select(ur => new UserRegionDto
        {
            Id = ur.Id,
            UserId = ur.UserId,
            RegionId = ur.RegionId,
            Metadata = ur.Metadata != null ? JsonSerializer.Deserialize<object>(ur.Metadata.RootElement.GetRawText()) : null,
            CreatedBy = ur.CreatedBy,
            CreatedAt = ur.CreatedAt,
            UpdatedAt = ur.UpdatedAt,
            User = new UserDto
            {
                Id = ur.User.Id,
                FullName = ur.User.FullName,
                Email = ur.User.Email,
                Active = ur.User.Active,
                CompanyId = ur.User.CompanyId,
                RoleId = ur.User.RoleId
            },
            Region = new RegionDto
            {
                Id = ur.Region.Id,
                Name = ur.Region.Name,
                RegionCode = ur.Region.RegionCode,
                CompanyId = ur.Region.CompanyId
            }
        }).ToList();
    }

    public async Task<UserRegionDto?> GetUserRegionAsync(int userId, int regionId)
    {
        var userRegion = await _context.UserRegions
            .Where(ur => ur.UserId == userId && ur.RegionId == regionId)
            .Include(ur => ur.User)
            .Include(ur => ur.Region)
            .FirstOrDefaultAsync();

        if (userRegion == null)
            return null;

        return new UserRegionDto
        {
            Id = userRegion.Id,
            UserId = userRegion.UserId,
            RegionId = userRegion.RegionId,
            Metadata = userRegion.Metadata != null ? JsonSerializer.Deserialize<object>(userRegion.Metadata.RootElement.GetRawText()) : null,
            CreatedBy = userRegion.CreatedBy,
            CreatedAt = userRegion.CreatedAt,
            UpdatedAt = userRegion.UpdatedAt,
            User = new UserDto
            {
                Id = userRegion.User.Id,
                FullName = userRegion.User.FullName,
                Email = userRegion.User.Email,
                Active = userRegion.User.Active,
                CompanyId = userRegion.User.CompanyId,
                RoleId = userRegion.User.RoleId
            },
            Region = new RegionDto
            {
                Id = userRegion.Region.Id,
                Name = userRegion.Region.Name,
                RegionCode = userRegion.Region.RegionCode,
                CompanyId = userRegion.Region.CompanyId
            }
        };
    }

    public async Task<UserRegion?> UpdateUserRegionAsync(int userId, int regionId, UserRegionUpdateDto updateDto, int updatedBy)
    {
        var userRegion = await _context.UserRegions
            .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RegionId == regionId);

        if (userRegion == null)
            return null;

        if (updateDto.Metadata != null)
        {
            userRegion.Metadata = JsonDocument.Parse(JsonSerializer.Serialize(updateDto.Metadata));
        }

        userRegion.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return userRegion;
    }

    public async Task<bool> ValidateUserRegionAssignmentAsync(int userId, int regionId)
    {
        // Get user's company country
        var userCompanyCountry = await _context.Users
            .Where(u => u.Id == userId && u.DeletedAt == null)
            .Include(u => u.Company)
            .Select(u => u.Company != null ? u.Company.CountryId : (int?)null)
            .FirstOrDefaultAsync();

        if (userCompanyCountry == null)
            return false;

        // Get region's country through company
        var regionCountry = await _context.Regions
            .Where(r => r.Id == regionId && r.DeletedAt == null)
            .Include(r => r.Company)
            .Select(r => r.Company.CountryId)
            .FirstOrDefaultAsync();

        if (regionCountry == 0)
            return false;

        return userCompanyCountry == regionCountry;
    }
}