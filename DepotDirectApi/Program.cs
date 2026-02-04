using DepotDirectApi.Data;
using DepotDirectApi.Repositories;
using DepotDirectApi.Services;
using Microsoft.EntityFrameworkCore;
using DepotDirectApi.Extensions;
using Microsoft.AspNetCore.Authentication;
using DepotDirectApi.Authentication;
using DepotDirectApi.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure application services
builder.Services.AddCorsPolicy();
builder.Services.AddSwaggerDocumentation();
builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddApplicationServices();

var app = builder.Build();

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowLocalhost");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();