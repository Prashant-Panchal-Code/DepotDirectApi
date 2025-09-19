using DepotDirectApi.Data;
using DepotDirectApi.Models.DTOs;
using DepotDirectApi.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace DepotDirectApi.Repositories;

public class RoleRepository : IRoleRepository
{
    private readonly DepotDirectDbContext _context;

    public RoleRepository(DepotDirectDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<RoleDto>> GetAllAsync()
    {
        return await _context.Roles
            .Select(r => new RoleDto
            {
                Id = r.Id,
                Name = r.Name,
                Description = r.Description,
                Permissions = r.Permissions,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt
            })
            .ToListAsync();
    }

    public async Task<RoleDto?> GetByIdAsync(int id)
    {
        return await _context.Roles
            .Where(r => r.Id == id)
            .Select(r => new RoleDto
            {
                Id = r.Id,
                Name = r.Name,
                Description = r.Description,
                Permissions = r.Permissions,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<RoleDto?> GetByNameAsync(string name)
    {
        return await _context.Roles
            .Where(r => r.Name == name)
            .Select(r => new RoleDto
            {
                Id = r.Id,
                Name = r.Name,
                Description = r.Description,
                Permissions = r.Permissions,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<Role> CreateAsync(CreateRoleDto createRoleDto)
    {
        var role = new Role
        {
            Name = createRoleDto.Name,
            Description = createRoleDto.Description,
            Permissions = createRoleDto.Permissions,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Roles.Add(role);
        await _context.SaveChangesAsync();
        return role;
    }

    public async Task<Role?> UpdateAsync(int id, UpdateRoleDto updateRoleDto)
    {
        var role = await _context.Roles.FindAsync(id);
        if (role == null) return null;

        if (!string.IsNullOrEmpty(updateRoleDto.Name))
            role.Name = updateRoleDto.Name;
        if (updateRoleDto.Description != null)
            role.Description = updateRoleDto.Description;
        if (updateRoleDto.Permissions != null)
            role.Permissions = updateRoleDto.Permissions;

        role.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return role;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var role = await _context.Roles.FindAsync(id);
        if (role == null) return false;

        _context.Roles.Remove(role);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Roles.AnyAsync(r => r.Id == id);
    }

    public async Task<bool> ExistsByNameAsync(string name)
    {
        return await _context.Roles.AnyAsync(r => r.Name == name);
    }
}