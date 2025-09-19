using Microsoft.AspNetCore.Mvc;

namespace DepotDirectApi.Controllers;

[ApiController]
[Route("public")]
public class PublicController : ControllerBase
{
    [HttpGet("status")]
    public IActionResult GetStatus()
    {
        return Ok(new
        {
            Status = "OK",
            Message = "DepotDirect API is running",
            Timestamp = DateTime.UtcNow,
            Version = "1.0.0",
            CorsEnabled = true
        });
    }

    [HttpGet("info")]
    public IActionResult GetInfo()
    {
        return Ok(new
        {
            ApplicationName = "DepotDirect API",
            Description = "Logistics and warehouse management system",
            Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production",
            Framework = ".NET 9.0",
            Features = new[]
            {
                "User Management",
                "Company Management", 
                "Country Management",
                "Region Management",
                "Depot Management",
                "Authentication",
                "CORS Support"
            }
        });
    }
}