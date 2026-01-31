using DepotDirectApi.Models.DTOs;
using DepotDirectApi.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DepotDirectApi.Controllers.Admin;

[Authorize]
[ApiController]
[Route("api/admin/[controller]")]
public class ProductsController : BaseController
{
    private readonly IProductRepository _productRepository;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(IProductRepository productRepository, ILogger<ProductsController> logger)
    {
        _productRepository = productRepository;
        _logger = logger;
    }

    /// <summary>
    /// Get products by region id
    /// </summary>
    [HttpGet("by-region/{regionId}")]
    public async Task<ActionResult<IEnumerable<ProductListItemDto>>> GetByRegion(int regionId)
    {
        try
        {
            var products = await _productRepository.GetByRegionIdAsync(regionId);
            return Ok(products);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving products for region {RegionId}", regionId);
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }
}
