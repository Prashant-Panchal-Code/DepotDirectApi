using DepotDirectApi.Models;
using DepotDirectApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DepotDirectApi.Controllers.User
{
    [Route("api/[controller]")]
    [Authorize]
    [Tags("User Profile")]
    public class ProfileController : BaseController
    {
        private readonly IUserService _userService;
        private readonly ILogger<ProfileController> _logger;

        public ProfileController(IUserService userService, ILogger<ProfileController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        /// <summary>
        /// Get current user's profile information
        /// </summary>
        /// <returns>User profile data</returns>
        [HttpGet]
        [ProducesResponseType(typeof(UserInfo), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<UserInfo>> GetProfile()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == 0)
                {
                    _logger.LogWarning("Invalid user ID in token");
                    return BadRequest(CreateErrorResponse("Invalid user ID"));
                }

                var userProfile = await _userService.GetUserByIdAsync(userId);
                if (userProfile == null)
                {
                    _logger.LogWarning("User profile not found for user ID {UserId}", userId);
                    return NotFound(CreateErrorResponse("User not found"));
                }

                var profile = new UserInfo
                {
                    Id = userProfile.Id,
                    Username = userProfile.Username,
                    Email = userProfile.Email,
                    Roles = userProfile.Roles
                };

                _logger.LogInformation("Retrieved profile for user {UserId}: {Username}", userId, userProfile.Username);
                return Ok(profile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user profile");
                return StatusCode(500, CreateErrorResponse("An error occurred while retrieving the profile"));
            }
        }
    }
}