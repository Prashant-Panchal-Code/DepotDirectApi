using DepotDirectApi.Models.DTOs;
using DepotDirectApi.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DepotDirectApi.Controllers.Admin;

[Authorize]
[ApiController]
[Route("api/admin/[controller]")]
public class RolesController : BaseController
{
    private readonly IRoleRepository _roleRepository;

    public RolesController(IRoleRepository roleRepository)
    {
        _roleRepository = roleRepository;
    }

    /// <summary>
    /// Get all roles
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<RoleDto>>> GetAll()
    {
        try
        {
            var roles = await _roleRepository.GetAllAsync();
            return Ok(roles);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    /// <summary>
    /// Get a specific role by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<RoleDto>> GetById(int id)
    {
        try
        {
            var role = await _roleRepository.GetByIdAsync(id);
            
            if (role == null)
                return NotFound(new { message = "Role not found" });
                
            return Ok(role);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    /// <summary>
    /// Get a role by name
    /// </summary>
    [HttpGet("by-name/{name}")]
    public async Task<ActionResult<RoleDto>> GetByName(string name)
    {
        try
        {
            var role = await _roleRepository.GetByNameAsync(name);
            
            if (role == null)
                return NotFound(new { message = "Role not found" });
                
            return Ok(role);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    /// <summary>
    /// Create a new role
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<RoleDto>> Create([FromBody] CreateRoleDto createRoleDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Check if role name already exists
            if (await _roleRepository.ExistsByNameAsync(createRoleDto.Name))
                return Conflict(new { message = $"Role with name '{createRoleDto.Name}' already exists" });

            var role = await _roleRepository.CreateAsync(createRoleDto);
            var roleDto = await _roleRepository.GetByIdAsync(role.Id);
            
            return CreatedAtAction(nameof(GetById), new { id = role.Id }, roleDto);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    /// <summary>
    /// Update an existing role
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<RoleDto>> Update(int id, [FromBody] UpdateRoleDto updateRoleDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Check if role name already exists (if name is being updated)
            if (!string.IsNullOrEmpty(updateRoleDto.Name))
            {
                var existingRole = await _roleRepository.GetByNameAsync(updateRoleDto.Name);
                if (existingRole != null && existingRole.Id != id)
                    return Conflict(new { message = $"Role with name '{updateRoleDto.Name}' already exists" });
            }

            var role = await _roleRepository.UpdateAsync(id, updateRoleDto);
            
            if (role == null)
                return NotFound(new { message = "Role not found" });

            var roleDto = await _roleRepository.GetByIdAsync(role.Id);
            return Ok(roleDto);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    /// <summary>
    /// Delete a role
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        try
        {
            var success = await _roleRepository.DeleteAsync(id);
            
            if (!success)
                return NotFound(new { message = "Role not found" });
                
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    /// <summary>
    /// Check if a role exists
    /// </summary>
    [HttpGet("{id}/exists")]
    public async Task<ActionResult<bool>> Exists(int id)
    {
        try
        {
            var exists = await _roleRepository.ExistsAsync(id);
            return Ok(new { exists });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }
}