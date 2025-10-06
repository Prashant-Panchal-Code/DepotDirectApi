using DepotDirectApi.Authentication;
using DepotDirectApi.Data;
using DepotDirectApi.Models;
using DepotDirectApi.Models.DTOs;
using DepotDirectApi.Models.Entities;
using DepotDirectApi.Repositories;
using DepotDirectApi.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure CORS to allow localhost origins
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost", policy =>
    {
        policy.WithOrigins(
            "http://localhost:3000",    // React default
            "http://localhost:3001",    // React alternative
            "http://localhost:4200",    // Angular default
            "http://localhost:5000",    // .NET default
            "http://localhost:5001",    // .NET HTTPS
            "http://localhost:8080",    // Vue default
            "http://localhost:8081",    // Vue alternative
            "http://127.0.0.1:3000",    // Alternative localhost format
            "http://127.0.0.1:4200",
            "http://127.0.0.1:8080"
        )
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials();
    });
    
    // Optional: Add a more permissive policy for development
    options.AddPolicy("AllowAllLocalhost", policy =>
    {
        policy.SetIsOriginAllowed(origin =>
        {
            var uri = new Uri(origin);
            return uri.Host == "localhost" || uri.Host == "127.0.0.1";
        })
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials();
    });
});

// Configure Swagger to support both Basic and JWT authentication
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "DepotDirect API", Version = "v1" });
    
    // Add Basic Authentication
    c.AddSecurityDefinition("Basic", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "basic",
        In = ParameterLocation.Header,
        Description = "Basic Authorization header using username and password."
    });
    
    // Add JWT Bearer Authentication
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\""
    });
    
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Basic"
                }
            },
            Array.Empty<string>()
        },
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Register authentication services
builder.Services.AddScoped<IUserService, InMemoryUserService>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

// Configure PostgreSQL Database
builder.Services.AddDbContext<DepotDirectDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DepotDirect")));

// Register repositories
builder.Services.AddScoped<ICountryRepository, CountryRepository>();
builder.Services.AddScoped<ICompanyRepository, CompanyRepository>();
builder.Services.AddScoped<IRegionRepository, RegionRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserRegionRepository, UserRegionRepository>();

// Configure Authentication (JWT + Basic)
var jwtSecretKey = builder.Configuration["Jwt:SecretKey"] ?? "your-super-secret-key-that-is-at-least-32-characters-long-for-production-use!";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "DepotDirectApi";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "DepotDirectClients";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSecretKey)),
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
})
.AddScheme<BasicAuthenticationSchemeOptions, BasicAuthenticationHandler>("Basic", null);

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Bearer", policy =>
    {
        policy.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
        policy.RequireAuthenticatedUser();
    });
    
    options.AddPolicy("Basic", policy =>
    {
        policy.AuthenticationSchemes.Add("Basic");
        policy.RequireAuthenticatedUser();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

// Add CORS middleware (must be before authentication)
app.UseCors("AllowAllLocalhost");

// Add authentication and authorization middleware
app.UseAuthentication();
app.UseAuthorization();

// Weather summaries for the demo endpoint
var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

// Authentication endpoints (keeping as minimal API for simplicity)
app.MapPost("/auth/login", async (LoginRequest request, IUserService userService) =>
{
    if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
    {
        return Results.BadRequest("Username and password are required");
    }

    var user = await userService.GetUserByUsernameAsync(request.Username);
    if (user == null || !user.IsActive)
    {
        return Results.Unauthorized();
    }

    var isValidPassword = await userService.ValidatePasswordAsync(user, request.Password);
    if (!isValidPassword)
    {
        return Results.Unauthorized();
    }

    // For basic auth, we just return user info without token
    var response = new
    {
        Message = "Login successful. Use Basic Auth with your username/password for API calls.",
        User = new UserInfo
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Roles = user.Roles
        }
    };

    return Results.Ok(response);
})
.WithName("Login")
.WithOpenApi();

// Demo weather endpoint
app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

// Map all controllers
app.MapControllers();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

// Login request model
record LoginRequest(string Username, string Password);