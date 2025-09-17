using DepotDirectApi.Authentication;
using DepotDirectApi.Data;
using DepotDirectApi.Models;
using DepotDirectApi.Models.DTOs;
using DepotDirectApi.Models.Entities;
using DepotDirectApi.Repositories;
using DepotDirectApi.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger to support Basic authentication only
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
        }
    });
});

// Register authentication services
builder.Services.AddScoped<IUserService, InMemoryUserService>();

// Configure PostgreSQL Database
builder.Services.AddDbContext<DepotDirectDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DepotDirect")));

// Register repositories
builder.Services.AddScoped<ICountryRepository, CountryRepository>();

// Configure Basic Authentication only
builder.Services.AddAuthentication("Basic")
    .AddScheme<BasicAuthenticationSchemeOptions, BasicAuthenticationHandler>("Basic", null);

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

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

// Public status endpoint
app.MapGet("/public/status", () => new { Status = "API is running", Timestamp = DateTime.UtcNow })
    .WithName("GetStatus")
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