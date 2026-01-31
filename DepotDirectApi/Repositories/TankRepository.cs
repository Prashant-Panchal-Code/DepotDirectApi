using DepotDirectApi.Data;
using DepotDirectApi.Models.DTOs;
using DepotDirectApi.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace DepotDirectApi.Repositories;

public class TankRepository : ITankRepository
{
    private readonly DepotDirectDbContext _context;

    public TankRepository(DepotDirectDbContext context)
    {
        _context = context;
    }

    public async Task<SiteTankDto> CreateTankAsync(CreateTankDto dto, int? createdBy = null)
    {
        // Validate site exists
        var siteExists = await _context.Sites.AnyAsync(s => s.Id == dto.SiteId && s.DeletedAt == null);
        if (!siteExists)
            throw new ArgumentException($"Site with ID {dto.SiteId} does not exist.");

        // Validate product if provided
        if (dto.ProductId.HasValue)
        {
            var productExists = await _context.Products.AnyAsync(p => p.Id == dto.ProductId.Value && p.DeletedAt == null);
            if (!productExists)
                throw new ArgumentException($"Product with ID {dto.ProductId.Value} does not exist.");
        }

        // Check uniqueness of tank code within the site
        var codeExists = await _context.SiteTanks.AnyAsync(t => t.SiteId == dto.SiteId && t.TankCode == dto.TankCode);
        if (codeExists)
            throw new ArgumentException($"Tank code '{dto.TankCode}' already exists for site {dto.SiteId}.");

        // Create tank with minimal required fields; other numeric fields default to 0
        var tank = new SiteTank
        {
            SiteId = dto.SiteId,
            ProductId = dto.ProductId,
            TankCode = dto.TankCode,
            CapacityL = 0,
            SafeFillL = 0,
            DeadstockL = 0,
            Active = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        try
        {
            _context.Add(tank);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException dbEx)
        {
            var inner = dbEx.InnerException?.Message ?? dbEx.Message;
            throw new ArgumentException($"Database error creating tank: {inner}");
        }

        return new SiteTankDto
        {
            Id = tank.Id,
            SiteId = tank.SiteId,
            ProductId = tank.ProductId,
            TankCode = tank.TankCode,
            CapacityL = tank.CapacityL,
            SafeFillL = tank.SafeFillL,
            DeadstockL = tank.DeadstockL,
            DischargeRateLpm = tank.DischargeRateLpm,
            Active = tank.Active,
            Metadata = tank.Metadata,
            CreatedAt = tank.CreatedAt,
            UpdatedAt = tank.UpdatedAt
        };
    }

    public async Task<SiteTankDto?> UpdateTankAsync(int tankId, UpdateTankDto dto, int? updatedBy = null)
    {
        var tank = await _context.Set<SiteTank>().FindAsync(tankId);
        if (tank == null) return null;

        if (dto.ProductId.HasValue) tank.ProductId = dto.ProductId.Value;
        if (dto.CapacityL.HasValue) tank.CapacityL = dto.CapacityL.Value;
        if (dto.SafeFillL.HasValue) tank.SafeFillL = dto.SafeFillL.Value;
        if (dto.DeadstockL.HasValue) tank.DeadstockL = dto.DeadstockL.Value;
        if (dto.DischargeRateLpm.HasValue) tank.DischargeRateLpm = dto.DischargeRateLpm.Value;
        if (dto.Active.HasValue) tank.Active = dto.Active.Value;
        if (dto.Metadata != null) tank.Metadata = dto.Metadata;

        tank.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return new SiteTankDto
        {
            Id = tank.Id,
            SiteId = tank.SiteId,
            ProductId = tank.ProductId,
            TankCode = tank.TankCode,
            CapacityL = tank.CapacityL,
            SafeFillL = tank.SafeFillL,
            DeadstockL = tank.DeadstockL,
            DischargeRateLpm = tank.DischargeRateLpm,
            Active = tank.Active,
            Metadata = tank.Metadata,
            CreatedAt = tank.CreatedAt,
            UpdatedAt = tank.UpdatedAt
        };
    }

    public async Task<bool> DeleteTankAsync(int tankId)
    {
        var tank = await _context.Set<SiteTank>().FindAsync(tankId);
        if (tank == null) return false;

        // Soft delete: mark inactive
        tank.Active = false;
        tank.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<IEnumerable<SiteTankDto>> GetTanksBySiteAsync(int siteId)
    {
        return await _context.Set<SiteTank>()
            .Where(t => t.SiteId == siteId && t.Active)
            .Select(t => new SiteTankDto
            {
                Id = t.Id,
                SiteId = t.SiteId,
                ProductId = t.ProductId,
                TankCode = t.TankCode,
                CapacityL = t.CapacityL,
                SafeFillL = t.SafeFillL,
                DeadstockL = t.DeadstockL,
                DischargeRateLpm = t.DischargeRateLpm,
                Active = t.Active,
                Metadata = t.Metadata,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt
            })
            .ToListAsync();
    }

    public async Task<SiteTankWithInventoryDto?> GetTankWithInventoryAsync(int tankId)
    {
        var tank = await _context.Set<SiteTank>()
            .Include(t => t.TankReadings)
            .Where(t => t.Id == tankId)
            .FirstOrDefaultAsync();

        if (tank == null) return null;

        var dto = new SiteTankWithInventoryDto
        {
            Id = tank.Id,
            SiteId = tank.SiteId,
            ProductId = tank.ProductId,
            TankCode = tank.TankCode,
            CapacityL = tank.CapacityL,
            SafeFillL = tank.SafeFillL,
            DeadstockL = tank.DeadstockL,
            DischargeRateLpm = tank.DischargeRateLpm,
            Active = tank.Active,
            Metadata = tank.Metadata,
            CreatedAt = tank.CreatedAt,
            UpdatedAt = tank.UpdatedAt,
            Readings = tank.TankReadings.Select(r => new TankReadingDto
            {
                Id = r.Id,
                TankId = r.TankId,
                ReadingTimestamp = r.ReadingTimestamp,
                ReadingMethod = r.ReadingMethod,
                CurrentVolumeL = r.CurrentVolumeL,
                SalesSinceLastReadingL = r.SalesSinceLastReadingL,
                AvgDailySalesL = r.AvgDailySalesL
            }).OrderByDescending(r => r.ReadingTimestamp).ToList()
        };

        return dto;
    }

    public async Task<SiteTankFullDto?> GetTankFullDetailsAsync(int tankId)
    {
        var tank = await _context.SiteTanks
            .Include(t => t.TankReadings)
            .Include(t => t.TankDeliveries)
            .Where(t => t.Id == tankId)
            .FirstOrDefaultAsync();

        if (tank == null) return null;

        var lastReadings = tank.TankReadings
            .OrderByDescending(r => r.ReadingTimestamp)
            .Take(10)
            .Select(r => new TankReadingDto
            {
                Id = r.Id,
                TankId = r.TankId,
                ReadingTimestamp = r.ReadingTimestamp,
                ReadingMethod = r.ReadingMethod,
                CurrentVolumeL = r.CurrentVolumeL,
                SalesSinceLastReadingL = r.SalesSinceLastReadingL,
                AvgDailySalesL = r.AvgDailySalesL
            }).ToList();

        var deliveries = tank.TankDeliveries
            .OrderByDescending(d => d.CreatedAt)
            .Take(20)
            .Select(d => new TankDeliveryDto
            {
                Id = d.Id,
                TankId = d.TankId,
                Status = d.Status,
                PlannedQuantityL = d.PlannedQuantityL,
                ConfirmedQuantityL = d.ConfirmedQuantityL,
                ScheduledArrival = d.ScheduledArrival,
                ActualArrival = d.ActualArrival
            }).ToList();

        // Sales patterns from sales_patterns table
        var salesPatterns = await _context.SalesPatterns
            .Where(sp => sp.TankId == tankId)
            .Select(sp => new SalesPatternDto
            {
                Id = sp.Id,
                TankId = sp.TankId,
                DayOfWeek = sp.DayOfWeek,
                HourOfDay = sp.HourOfDay,
                WeightFactor = sp.WeightFactor,
                AvgHourlySalesL = sp.AvgHourlySalesL
            })
            .ToListAsync();

        var result = new SiteTankFullDto
        {
            Id = tank.Id,
            SiteId = tank.SiteId,
            ProductId = tank.ProductId,
            TankCode = tank.TankCode,
            CapacityL = tank.CapacityL,
            SafeFillL = tank.SafeFillL,
            DeadstockL = tank.DeadstockL,
            DischargeRateLpm = tank.DischargeRateLpm,
            Active = tank.Active,
            Metadata = tank.Metadata,
            CreatedAt = tank.CreatedAt,
            UpdatedAt = tank.UpdatedAt,
            LastReadings = lastReadings,
            Deliveries = deliveries,
            SalesPatterns = salesPatterns
        };

        return result;
    }

    public async Task<IEnumerable<SiteTankFullDto>> GetTanksFullBySiteAsync(int siteId)
    {
        var tanks = await _context.SiteTanks
            .Where(t => t.SiteId == siteId && t.Active)
            .Include(t => t.TankReadings)
            .Include(t => t.TankDeliveries)
            .ToListAsync();

        var results = new List<SiteTankFullDto>();

        foreach (var tank in tanks)
        {
            var lastReadings = tank.TankReadings
                .OrderByDescending(r => r.ReadingTimestamp)
                .Take(10)
                .Select(r => new TankReadingDto
                {
                    Id = r.Id,
                    TankId = r.TankId,
                    ReadingTimestamp = r.ReadingTimestamp,
                    ReadingMethod = r.ReadingMethod,
                    CurrentVolumeL = r.CurrentVolumeL,
                    SalesSinceLastReadingL = r.SalesSinceLastReadingL,
                    AvgDailySalesL = r.AvgDailySalesL
                }).ToList();

            var deliveries = tank.TankDeliveries
                .OrderByDescending(d => d.CreatedAt)
                .Take(20)
                .Select(d => new TankDeliveryDto
                {
                    Id = d.Id,
                    TankId = d.TankId,
                    Status = d.Status,
                    PlannedQuantityL = d.PlannedQuantityL,
                    ConfirmedQuantityL = d.ConfirmedQuantityL,
                    ScheduledArrival = d.ScheduledArrival,
                    ActualArrival = d.ActualArrival
                }).ToList();

            var salesPatterns = await _context.SalesPatterns
                .Where(sp => sp.TankId == tank.Id)
                .Select(sp => new SalesPatternDto
                {
                    Id = sp.Id,
                    TankId = sp.TankId,
                    DayOfWeek = sp.DayOfWeek,
                    HourOfDay = sp.HourOfDay,
                    WeightFactor = sp.WeightFactor,
                    AvgHourlySalesL = sp.AvgHourlySalesL
                })
                .ToListAsync();

            results.Add(new SiteTankFullDto
            {
                Id = tank.Id,
                SiteId = tank.SiteId,
                ProductId = tank.ProductId,
                TankCode = tank.TankCode,
                CapacityL = tank.CapacityL,
                SafeFillL = tank.SafeFillL,
                DeadstockL = tank.DeadstockL,
                DischargeRateLpm = tank.DischargeRateLpm,
                Active = tank.Active,
                Metadata = tank.Metadata,
                CreatedAt = tank.CreatedAt,
                UpdatedAt = tank.UpdatedAt,
                LastReadings = lastReadings,
                Deliveries = deliveries,
                SalesPatterns = salesPatterns
            });
        }

        return results;
    }

    public async Task<TankReadingDto> CreateTankReadingAsync(int tankId, CreateTankReadingDto dto, int? createdBy = null)
    {
        var tank = await _context.SiteTanks.FindAsync(tankId);
        if (tank == null) throw new ArgumentException($"Tank with ID {tankId} does not exist.");

        var reading = new TankReading
        {
            TankId = tankId,
            ReadingMethod = dto.ReadingMethod,
            CurrentVolumeL = dto.CurrentVolumeL,
            ReadingTimestamp = dto.ReadingTimestamp ?? DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Metadata = dto.Metadata
        };

        _context.TankReadings.Add(reading);
        await _context.SaveChangesAsync();

        return new TankReadingDto
        {
            Id = reading.Id,
            TankId = reading.TankId,
            ReadingTimestamp = reading.ReadingTimestamp,
            ReadingMethod = reading.ReadingMethod,
            CurrentVolumeL = reading.CurrentVolumeL,
            SalesSinceLastReadingL = reading.SalesSinceLastReadingL,
            AvgDailySalesL = reading.AvgDailySalesL
        };
    }
}
