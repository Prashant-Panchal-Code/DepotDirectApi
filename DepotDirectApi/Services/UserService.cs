using DepotDirectApi.Models;
using System.Security.Cryptography;
using System.Text;

namespace DepotDirectApi.Services;

public class InMemoryUserService : IUserService
{
    private readonly List<User> _users = new();
    private int _nextId = 1;

    public InMemoryUserService()
    {
        // Seed with some test users
        SeedUsers();
    }

    public Task<User?> GetUserByUsernameAsync(string username)
    {
        var user = _users.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(user);
    }

    public Task<User?> GetUserByIdAsync(int id)
    {
        var user = _users.FirstOrDefault(u => u.Id == id);
        return Task.FromResult(user);
    }

    public Task<bool> ValidatePasswordAsync(User user, string password)
    {
        var hashedPassword = HashPassword(password);
        return Task.FromResult(user.PasswordHash == hashedPassword);
    }

    public Task<User> CreateUserAsync(string username, string email, string password, string[] roles)
    {
        var user = new User
        {
            Id = _nextId++,
            Username = username,
            Email = email,
            PasswordHash = HashPassword(password),
            Roles = roles,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _users.Add(user);
        return Task.FromResult(user);
    }

    private string HashPassword(string password)
    {
        // Simple hashing for demo purposes - in production use BCrypt or similar
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password + "salt123"));
        return Convert.ToBase64String(hashBytes);
    }

    private void SeedUsers()
    {
        // Create test users
        _users.Add(new User
        {
            Id = _nextId++,
            Username = "admin",
            Email = "admin@depotdirect.com",
            PasswordHash = HashPassword("admin123"),
            Roles = new[] { "Admin", "User" },
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        });

        _users.Add(new User
        {
            Id = _nextId++,
            Username = "user",
            Email = "user@depotdirect.com",
            PasswordHash = HashPassword("user123"),
            Roles = new[] { "User" },
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        });
    }
}