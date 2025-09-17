using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace DepotDirectApi.Configuration;

public static class OAuth2Configuration
{
    public static void AddOAuth2Authentication(this IServiceCollection services, IConfiguration configuration)
    {
        // Add Google OAuth 2.0 Authentication
        services.AddAuthentication()
            .AddGoogle("Google", options =>
            {
                options.ClientId = configuration["OAuth:Google:ClientId"] ?? "";
                options.ClientSecret = configuration["OAuth:Google:ClientSecret"] ?? "";
                options.CallbackPath = "/signin-google";
                
                // Request additional scopes
                options.Scope.Add("email");
                options.Scope.Add("profile");
                
                // Save tokens
                options.SaveTokens = true;
            });

        // Add Microsoft OAuth 2.0 Authentication
        services.AddAuthentication()
            .AddMicrosoftAccount("Microsoft", options =>
            {
                options.ClientId = configuration["OAuth:Microsoft:ClientId"] ?? "";
                options.ClientSecret = configuration["OAuth:Microsoft:ClientSecret"] ?? "";
                options.CallbackPath = "/signin-microsoft";
                
                // Request additional scopes
                options.Scope.Add("email");
                options.Scope.Add("profile");
                
                // Save tokens
                options.SaveTokens = true;
            });

        // Add OpenID Connect for custom OAuth providers
        services.AddAuthentication()
            .AddOpenIdConnect("CustomOIDC", options =>
            {
                options.Authority = configuration["OAuth:Custom:Authority"] ?? "";
                options.ClientId = configuration["OAuth:Custom:ClientId"] ?? "";
                options.ClientSecret = configuration["OAuth:Custom:ClientSecret"] ?? "";
                options.ResponseType = OpenIdConnectResponseType.Code;
                options.CallbackPath = "/signin-oidc";
                
                // Configure scopes
                options.Scope.Add("openid");
                options.Scope.Add("profile");
                options.Scope.Add("email");
                
                // Save tokens
                options.SaveTokens = true;
            });
    }
}