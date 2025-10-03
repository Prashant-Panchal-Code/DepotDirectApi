using DepotDirectApi.Data;
using DepotDirectApi.Models.DTOs;
using DepotDirectApi.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace DepotDirectApi.Repositories;

public class UserRepository : IUserRepository
{
    private readonly DepotDirectDbContext _context;

    public UserRepository(DepotDirectDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<UserDto>> GetAllAsync()
    {
        return await _context.Users
            .Include(u => u.Company)
            .Include(u => u.Role)
            .Where(u => u.DeletedAt == null)
            .Select(u => new UserDto
            {
                Id = u.Id,
                CompanyId = u.CompanyId,
                CompanyName = u.Company != null ? u.Company.Name : null,
                RoleId = u.RoleId,
                RoleName = u.Role.Name,
                Email = u.Email,
                FullName = u.FullName,
                Phone = u.Phone,
                Active = u.Active,
                Metadata = u.Metadata,
                CreatedBy = u.CreatedBy,
                LastUpdatedBy = u.LastUpdatedBy,
                CreatedAt = u.CreatedAt,
                UpdatedAt = u.UpdatedAt,
                DeletedAt = u.DeletedAt
            })
            .ToListAsync();
    }

    public async Task<UserDto?> GetByIdAsync(int id)
    {
        return await _context.Users
            .Include(u => u.Company)
            .Include(u => u.Role)
            .Where(u => u.Id == id && u.DeletedAt == null)
            .Select(u => new UserDto
            {
                Id = u.Id,
                CompanyId = u.CompanyId,
                CompanyName = u.Company != null ? u.Company.Name : null,
                RoleId = u.RoleId,
                RoleName = u.Role.Name,
                Email = u.Email,
                FullName = u.FullName,
                Phone = u.Phone,
                Active = u.Active,
                Metadata = u.Metadata,
                CreatedBy = u.CreatedBy,
                LastUpdatedBy = u.LastUpdatedBy,
                CreatedAt = u.CreatedAt,
                UpdatedAt = u.UpdatedAt,
                DeletedAt = u.DeletedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<UserDto?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .Include(u => u.Company)
            .Include(u => u.Role)
            .Where(u => u.Email == email && u.DeletedAt == null)
            .Select(u => new UserDto
            {
                Id = u.Id,
                CompanyId = u.CompanyId,
                CompanyName = u.Company != null ? u.Company.Name : null,
                RoleId = u.RoleId,
                RoleName = u.Role.Name,
                Email = u.Email,
                FullName = u.FullName,
                Phone = u.Phone,
                Active = u.Active,
                Metadata = u.Metadata,
                CreatedBy = u.CreatedBy,
                LastUpdatedBy = u.LastUpdatedBy,
                CreatedAt = u.CreatedAt,
                UpdatedAt = u.UpdatedAt,
                DeletedAt = u.DeletedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<UserDto>> GetByCompanyIdAsync(int companyId)
    {
        return await _context.Users
            .Include(u => u.Company)
            .Include(u => u.Role)
            .Where(u => u.CompanyId == companyId && u.DeletedAt == null)
            .Select(u => new UserDto
            {
                Id = u.Id,
                CompanyId = u.CompanyId,
                CompanyName = u.Company != null ? u.Company.Name : null,
                RoleId = u.RoleId,
                RoleName = u.Role.Name,
                Email = u.Email,
                FullName = u.FullName,
                Phone = u.Phone,
                Active = u.Active,
                Metadata = u.Metadata,
                CreatedBy = u.CreatedBy,
                LastUpdatedBy = u.LastUpdatedBy,
                CreatedAt = u.CreatedAt,
                UpdatedAt = u.UpdatedAt,
                DeletedAt = u.DeletedAt
            })
            .ToListAsync();
    }

    public async Task<IEnumerable<UserDto>> GetByRoleIdAsync(int roleId)
    {
        return await _context.Users
            .Include(u => u.Company)
            .Include(u => u.Role)
            .Where(u => u.RoleId == roleId && u.DeletedAt == null)
            .Select(u => new UserDto
            {
                Id = u.Id,
                CompanyId = u.CompanyId,
                CompanyName = u.Company != null ? u.Company.Name : null,
                RoleId = u.RoleId,
                RoleName = u.Role.Name,
                Email = u.Email,
                FullName = u.FullName,
                Phone = u.Phone,
                Active = u.Active,
                Metadata = u.Metadata,
                CreatedBy = u.CreatedBy,
                LastUpdatedBy = u.LastUpdatedBy,
                CreatedAt = u.CreatedAt,
                UpdatedAt = u.UpdatedAt,
                DeletedAt = u.DeletedAt
            })
            .ToListAsync();
    }

    public async Task<IEnumerable<UserDto>> GetByCountryIdAsync(int countryId)
    {
        return await _context.Users
            .Include(u => u.Company)
            .Include(u => u.Role)
            .Where(u => u.DeletedAt == null && u.Company != null && u.Company.CountryId == countryId && u.Company.DeletedAt == null)
            .Select(u => new UserDto
            {
                Id = u.Id,
                CompanyId = u.CompanyId,
                CompanyName = u.Company.Name,
                RoleId = u.RoleId,
                RoleName = u.Role.Name,
                Email = u.Email,
                FullName = u.FullName,
                Phone = u.Phone,
                Active = u.Active,
                Metadata = u.Metadata,
                CreatedBy = u.CreatedBy,
                LastUpdatedBy = u.LastUpdatedBy,
                CreatedAt = u.CreatedAt,
                UpdatedAt = u.UpdatedAt,
                DeletedAt = u.DeletedAt
            })
            .ToListAsync();
    }

    public async Task<User> CreateAsync(CreateUserDto createUserDto)
    {
        var user = new User
        {
            CompanyId = createUserDto.CompanyId,
            RoleId = createUserDto.RoleId,
            Email = createUserDto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(createUserDto.Password),
            FullName = createUserDto.FullName,
            Phone = createUserDto.Phone,
            Active = createUserDto.Active,
            Metadata = createUserDto.Metadata,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<User?> UpdateAsync(int id, UpdateUserDto updateUserDto)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null || user.DeletedAt != null) return null;

        if (updateUserDto.CompanyId.HasValue)
            user.CompanyId = updateUserDto.CompanyId;
        if (updateUserDto.RoleId.HasValue)
            user.RoleId = updateUserDto.RoleId.Value;
        if (!string.IsNullOrEmpty(updateUserDto.Email))
            user.Email = updateUserDto.Email;
        if (!string.IsNullOrEmpty(updateUserDto.Password))
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(updateUserDto.Password);
        if (!string.IsNullOrEmpty(updateUserDto.FullName))
            user.FullName = updateUserDto.FullName;
        if (updateUserDto.Phone != null)
            user.Phone = updateUserDto.Phone;
        if (updateUserDto.Active.HasValue)
            user.Active = updateUserDto.Active.Value;
        if (updateUserDto.Metadata != null)
            user.Metadata = updateUserDto.Metadata;

        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null || user.DeletedAt != null) return false;

        // Soft delete
        user.Active = false;
        user.DeletedAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Users.AnyAsync(u => u.Id == id && u.DeletedAt == null);
    }

    public async Task<bool> ExistsByEmailAsync(string email)
    {
        return await _context.Users.AnyAsync(u => u.Email == email && u.DeletedAt == null);
    }
}