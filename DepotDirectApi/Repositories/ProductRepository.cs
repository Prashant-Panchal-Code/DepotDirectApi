using DepotDirectApi.Data;
using DepotDirectApi.Models.DTOs;
using DepotDirectApi.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace DepotDirectApi.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly DepotDirectDbContext _context;

    public ProductRepository(DepotDirectDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ProductListItemDto>> GetByRegionIdAsync(int regionId)
    {
        return await _context.Products
            .Where(p => p.RegionId == regionId && p.DeletedAt == null)
            .Include(p => p.Company)
            .Include(p => p.Region)
            .Select(p => new ProductListItemDto
            {
                Id = p.Id,
                ProductCode = p.ProductCode,
                ProductName = p.ProductName,
                ShortName = p.ShortName,
                CompanyId = p.CompanyId,
                CompanyName = p.Company.Name,
                RegionId = p.RegionId,
                RegionName = p.Region.Name,
                Active = p.Active,
                IsHazardous = p.IsHazardous,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            })
            .OrderBy(p => p.ProductName)
            .ToListAsync();
    }
}
