using DepotDirectApi.Models.DTOs;
using DepotDirectApi.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DepotDirectApi.Controllers.Admin;

[Authorize]
[ApiController]
[Route("api/admin/[controller]")]
public class UsersController : BaseController
{
    private readonly IUserRepository _userRepository;

    public UsersController(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    /// <summary>
    /// Get all users
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetAll()
    {
        try
        {
            var users = await _userRepository.GetAllAsync();
            return Ok(users);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    /// <summary>
    /// Get a specific user by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetById(int id)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(id);
            
            if (user == null)
                return NotFound(new { message = "User not found" });
                
            return Ok(user);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    /// <summary>
    /// Get a user by email
    /// </summary>
    [HttpGet("by-email/{email}")]
    public async Task<ActionResult<UserDto>> GetByEmail(string email)
    {
        try
        {
            var user = await _userRepository.GetByEmailAsync(email);
            
            if (user == null)
                return NotFound(new { message = "User not found" });
                
            return Ok(user);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    /// <summary>
    /// Get users by company ID
    /// </summary>
    [HttpGet("by-company/{companyId}")]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetByCompanyId(int companyId)
    {
        try
        {
            var users = await _userRepository.GetByCompanyIdAsync(companyId);
            return Ok(users);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    /// <summary>
    /// Get users by role ID
    /// </summary>
    [HttpGet("by-role/{roleId}")]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetByRoleId(int roleId)
    {
        try
        {
            var users = await _userRepository.GetByRoleIdAsync(roleId);
            return Ok(users);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    /// <summary>
    /// Create a new user
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<UserDto>> Create([FromBody] CreateUserDto createUserDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Check if email already exists
            if (await _userRepository.ExistsByEmailAsync(createUserDto.Email))
                return Conflict(new { message = $"User with email '{createUserDto.Email}' already exists" });

            var user = await _userRepository.CreateAsync(createUserDto);
            var userDto = await _userRepository.GetByIdAsync(user.Id);
            
            return CreatedAtAction(nameof(GetById), new { id = user.Id }, userDto);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    /// <summary>
    /// Update an existing user
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<UserDto>> Update(int id, [FromBody] UpdateUserDto updateUserDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Check if email already exists (if email is being updated)
            if (!string.IsNullOrEmpty(updateUserDto.Email))
            {
                var existingUser = await _userRepository.GetByEmailAsync(updateUserDto.Email);
                if (existingUser != null && existingUser.Id != id)
                    return Conflict(new { message = $"User with email '{updateUserDto.Email}' already exists" });
            }

            var user = await _userRepository.UpdateAsync(id, updateUserDto);
            
            if (user == null)
                return NotFound(new { message = "User not found" });

            var userDto = await _userRepository.GetByIdAsync(user.Id);
            return Ok(userDto);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    /// <summary>
    /// Delete a user (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        try
        {
            var success = await _userRepository.DeleteAsync(id);
            
            if (!success)
                return NotFound(new { message = "User not found" });
                
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    /// <summary>
    /// Check if a user exists
    /// </summary>
    [HttpGet("{id}/exists")]
    public async Task<ActionResult<bool>> Exists(int id)
    {
        try
        {
            var exists = await _userRepository.ExistsAsync(id);
            return Ok(new { exists });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }
}