using DepotDirectApi.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

namespace DepotDirectApi.Authentication;

public class BasicAuthenticationSchemeOptions : AuthenticationSchemeOptions
{
}

public class BasicAuthenticationHandler : AuthenticationHandler<BasicAuthenticationSchemeOptions>
{
    private readonly IUserService _userService;

    public BasicAuthenticationHandler(
        IOptionsMonitor<BasicAuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IUserService userService)
        : base(options, logger, encoder)
    {
        _userService = userService;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.ContainsKey("Authorization"))
        {
            return AuthenticateResult.Fail("Missing Authorization header");
        }

        var authHeader = Request.Headers["Authorization"].ToString();
        if (!authHeader.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
        {
            return AuthenticateResult.Fail("Invalid Authorization header");
        }

        var token = authHeader.Substring("Basic ".Length).Trim();
        var credentialsString = Encoding.UTF8.GetString(Convert.FromBase64String(token));
        var credentials = credentialsString.Split(':', 2);

        if (credentials.Length != 2)
        {
            return AuthenticateResult.Fail("Invalid credentials format");
        }

        var username = credentials[0];
        var password = credentials[1];

        var user = await _userService.GetUserByUsernameAsync(username);
        if (user == null || !user.IsActive)
        {
            return AuthenticateResult.Fail("Invalid username or password");
        }

        var isValidPassword = await _userService.ValidatePasswordAsync(user, password);
        if (!isValidPassword)
        {
            return AuthenticateResult.Fail("Invalid username or password");
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Email, user.Email)
        };

        // Add role claims
        foreach (var role in user.Roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return AuthenticateResult.Success(ticket);
    }
}