using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DepotDirectApi.Controllers
{
    /// <summary>
    /// Base controller with common functionality for all controllers
    /// </summary>
    [ApiController]
    public abstract class BaseController : ControllerBase
    {
        /// <summary>
        /// Get the current authenticated user ID
        /// </summary>
        /// <returns>User ID or 0 if not found</returns>
        protected int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return int.TryParse(userIdClaim?.Value, out int userId) ? userId : 0;
        }

        /// <summary>
        /// Get the current authenticated username
        /// </summary>
        /// <returns>Username or empty string if not found</returns>
        protected string GetCurrentUsername()
        {
            return User.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;
        }

        /// <summary>
        /// Check if the current user has a specific role
        /// </summary>
        /// <param name="role">Role to check</param>
        /// <returns>True if user has the role</returns>
        protected bool IsInRole(string role)
        {
            return User.IsInRole(role);
        }

        /// <summary>
        /// Get all roles for the current user
        /// </summary>
        /// <returns>List of user roles</returns>
        protected List<string> GetUserRoles()
        {
            return User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
        }

        /// <summary>
        /// Create a standardized error response
        /// </summary>
        /// <param name="message">Error message</param>
        /// <param name="details">Optional error details</param>
        /// <returns>Error response object</returns>
        protected object CreateErrorResponse(string message, string? details = null)
        {
            var response = new
            {
                Error = message,
                Timestamp = DateTime.UtcNow,
                RequestId = HttpContext.TraceIdentifier
            };

            if (!string.IsNullOrEmpty(details))
            {
                return new
                {
                    response.Error,
                    Details = details,
                    response.Timestamp,
                    response.RequestId
                };
            }

            return response;
        }
    }
}