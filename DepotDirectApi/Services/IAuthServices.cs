using DepotDirectApi.Models;

namespace DepotDirectApi.Services;

public interface IUserService
{
    Task<User?> GetUserByUsernameAsync(string username);
    Task<User?> GetUserByIdAsync(int id);
    Task<bool> ValidatePasswordAsync(User user, string password);
    Task<User> CreateUserAsync(string username, string email, string password, string[] roles);
}

public interface ITokenService
{
    string GenerateToken(User user);
    bool ValidateToken(string token);
    int? GetUserIdFromToken(string token);
}