using DepotDirectApi.Data;
using DepotDirectApi.Models.DTOs;
using DepotDirectApi.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace DepotDirectApi.Repositories;

public class BreakRuleRepository : IBreakRuleRepository
{
    private readonly DepotDirectDbContext _context;
    private readonly ILogger<BreakRuleRepository> _logger;

    public BreakRuleRepository(DepotDirectDbContext context, ILogger<BreakRuleRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<BreakRuleListItemDto>> GetAllAsync()
    {
        return await _context.BreakRules
            .Include(br => br.Company)
            .Where(br => br.Active)
            .Select(br => new BreakRuleListItemDto
            {
                Id = br.Id,
                RuleName = br.RuleName,
                CompanyId = br.CompanyId,
                CompanyName = br.Company.Name,
                MaxContinuousDriveMins = br.MaxContinuousDriveMins,
                MinBreakDurationMins = br.MinBreakDurationMins,
                MaxDailyDriveMins = br.MaxDailyDriveMins,
                MinDailyRestMins = br.MinDailyRestMins,
                Active = br.Active,
                CreatedAt = br.CreatedAt,
                UpdatedAt = br.UpdatedAt
            })
            .OrderBy(br => br.CompanyName)
            .ThenBy(br => br.RuleName)
            .ToListAsync();
    }

    public async Task<BreakRuleResponseDto?> GetByIdAsync(int id)
    {
        return await _context.BreakRules
            .Include(br => br.Company)
            .Where(br => br.Id == id)
            .Select(br => new BreakRuleResponseDto
            {
                Id = br.Id,
                RuleName = br.RuleName,
                CompanyId = br.CompanyId,
                MaxContinuousDriveMins = br.MaxContinuousDriveMins,
                MinBreakDurationMins = br.MinBreakDurationMins,
                MaxDailyDriveMins = br.MaxDailyDriveMins,
                MinDailyRestMins = br.MinDailyRestMins,
                Active = br.Active,
                CreatedAt = br.CreatedAt,
                UpdatedAt = br.UpdatedAt,
                Company = new CompanyDto
                {
                    Id = br.Company.Id,
                    Name = br.Company.Name,
                    CountryId = br.Company.CountryId,
                    CreatedAt = br.Company.CreatedAt,
                    UpdatedAt = br.Company.UpdatedAt
                }
            })
            .FirstOrDefaultAsync();
    }

    public async Task<BreakRuleResponseDto> CreateAsync(CreateBreakRuleDto createBreakRuleDto, int? createdBy = null)
    {
        // Check if rule name already exists for this company
        if (await ExistsByRuleNameAndCompanyAsync(createBreakRuleDto.RuleName, createBreakRuleDto.CompanyId))
        {
            throw new ArgumentException($"Break rule with name '{createBreakRuleDto.RuleName}' already exists for this company");
        }

        // Verify company exists
        var companyExists = await _context.Companies.AnyAsync(c => c.Id == createBreakRuleDto.CompanyId);
        if (!companyExists)
        {
            throw new ArgumentException($"Company with ID {createBreakRuleDto.CompanyId} not found");
        }

        var breakRule = new BreakRule
        {
            RuleName = createBreakRuleDto.RuleName,
            CompanyId = createBreakRuleDto.CompanyId,
            MaxContinuousDriveMins = createBreakRuleDto.MaxContinuousDriveMins,
            MinBreakDurationMins = createBreakRuleDto.MinBreakDurationMins,
            MaxDailyDriveMins = createBreakRuleDto.MaxDailyDriveMins,
            MinDailyRestMins = createBreakRuleDto.MinDailyRestMins,
            Active = createBreakRuleDto.Active ?? true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.BreakRules.Add(breakRule);
        await _context.SaveChangesAsync();

        return await GetByIdAsync(breakRule.Id) ?? throw new InvalidOperationException("Failed to retrieve created break rule");
    }

    public async Task<BreakRuleResponseDto?> UpdateAsync(int id, UpdateBreakRuleDto updateBreakRuleDto, int? updatedBy = null)
    {
        var breakRule = await _context.BreakRules.FindAsync(id);
        if (breakRule == null)
            return null;

        // Check if rule name already exists for this company (excluding current rule)
        if (updateBreakRuleDto.RuleName != null && 
            await ExistsByRuleNameAndCompanyAsync(updateBreakRuleDto.RuleName, breakRule.CompanyId, id))
        {
            throw new ArgumentException($"Break rule with name '{updateBreakRuleDto.RuleName}' already exists for this company");
        }

        // Update only provided fields
        if (updateBreakRuleDto.RuleName != null)
            breakRule.RuleName = updateBreakRuleDto.RuleName;
        if (updateBreakRuleDto.MaxContinuousDriveMins.HasValue)
            breakRule.MaxContinuousDriveMins = updateBreakRuleDto.MaxContinuousDriveMins.Value;
        if (updateBreakRuleDto.MinBreakDurationMins.HasValue)
            breakRule.MinBreakDurationMins = updateBreakRuleDto.MinBreakDurationMins.Value;
        if (updateBreakRuleDto.MaxDailyDriveMins.HasValue)
            breakRule.MaxDailyDriveMins = updateBreakRuleDto.MaxDailyDriveMins.Value;
        if (updateBreakRuleDto.MinDailyRestMins.HasValue)
            breakRule.MinDailyRestMins = updateBreakRuleDto.MinDailyRestMins.Value;
        if (updateBreakRuleDto.Active.HasValue)
            breakRule.Active = updateBreakRuleDto.Active.Value;

        breakRule.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return await GetByIdAsync(id);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var breakRule = await _context.BreakRules.FindAsync(id);
        if (breakRule == null)
            return false;

        // Check if break rule is being used by any drivers
        var isBeingUsed = await _context.Drivers.AnyAsync(d => d.BreakRuleId == id);
        if (isBeingUsed)
        {
            throw new InvalidOperationException("Cannot delete break rule that is assigned to drivers");
        }

        _context.BreakRules.Remove(breakRule);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.BreakRules.AnyAsync(br => br.Id == id);
    }

    public async Task<bool> ExistsByRuleNameAndCompanyAsync(string ruleName, int companyId, int? excludeId = null)
    {
        var query = _context.BreakRules.Where(br => br.RuleName == ruleName && br.CompanyId == companyId);
        
        if (excludeId.HasValue)
            query = query.Where(br => br.Id != excludeId.Value);

        return await query.AnyAsync();
    }

    public async Task<IEnumerable<BreakRuleListItemDto>> GetByCompanyIdAsync(int companyId)
    {
        return await _context.BreakRules
            .Include(br => br.Company)
            .Where(br => br.CompanyId == companyId && br.Active)
            .Select(br => new BreakRuleListItemDto
            {
                Id = br.Id,
                RuleName = br.RuleName,
                CompanyId = br.CompanyId,
                CompanyName = br.Company.Name,
                MaxContinuousDriveMins = br.MaxContinuousDriveMins,
                MinBreakDurationMins = br.MinBreakDurationMins,
                MaxDailyDriveMins = br.MaxDailyDriveMins,
                MinDailyRestMins = br.MinDailyRestMins,
                Active = br.Active,
                CreatedAt = br.CreatedAt,
                UpdatedAt = br.UpdatedAt
            })
            .OrderBy(br => br.RuleName)
            .ToListAsync();
    }

    public async Task<IEnumerable<BreakRuleListItemDto>> SearchAsync(string searchTerm)
    {
        var normalizedSearchTerm = searchTerm.ToLower();

        return await _context.BreakRules
            .Include(br => br.Company)
            .Where(br => br.Active && 
                        (br.RuleName.ToLower().Contains(normalizedSearchTerm) ||
                         br.Company.Name.ToLower().Contains(normalizedSearchTerm)))
            .Select(br => new BreakRuleListItemDto
            {
                Id = br.Id,
                RuleName = br.RuleName,
                CompanyId = br.CompanyId,
                CompanyName = br.Company.Name,
                MaxContinuousDriveMins = br.MaxContinuousDriveMins,
                MinBreakDurationMins = br.MinBreakDurationMins,
                MaxDailyDriveMins = br.MaxDailyDriveMins,
                MinDailyRestMins = br.MinDailyRestMins,
                Active = br.Active,
                CreatedAt = br.CreatedAt,
                UpdatedAt = br.UpdatedAt
            })
            .OrderBy(br => br.CompanyName)
            .ThenBy(br => br.RuleName)
            .ToListAsync();
    }
}