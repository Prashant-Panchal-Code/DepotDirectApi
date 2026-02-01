using DepotDirectApi.Data;
using DepotDirectApi.Models.DTOs;
using DepotDirectApi.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace DepotDirectApi.Repositories;

public class ParkingRepository : IParkingRepository
{
    private readonly DepotDirectDbContext _context;

    public ParkingRepository(DepotDirectDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ParkingListItemDto>> GetAllAsync()
    {
        return await _context.Parkings
            .Where(p => p.DeletedAt == null)
            .Include(p => p.Company)
            .Include(p => p.Country)
            .Select(p => new ParkingListItemDto
            {
                Id = p.Id,
                ParkingCode = p.ParkingCode,
                ParkingName = p.ParkingName,
                Town = p.Town,
                Active = p.Active,
                ParkingSpaces = p.ParkingSpaces,
                CompanyId = p.CompanyId,
                CompanyName = p.Company.Name,
                CountryId = p.CountryId,
                CountryName = p.Country.Name,
                Latitude = p.Latitude,
                Longitude = p.Longitude,
                LatLong = p.LatLong,
                Street = p.Street,
                PostalCode = p.PostalCode,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            })
            .OrderBy(p => p.ParkingName)
            .ToListAsync();
    }

    public async Task<ParkingResponseDto?> GetByIdAsync(int id)
    {
        var parking = await _context.Parkings
            .Where(p => p.Id == id && p.DeletedAt == null)
            .Include(p => p.Company)
            .Include(p => p.Country)
            .Include(p => p.RegionParkings)
            .ThenInclude(rp => rp.Region)
            .FirstOrDefaultAsync();

        if (parking == null)
            return null;

        return new ParkingResponseDto
        {
            Id = parking.Id,
            ParkingCode = parking.ParkingCode,
            ParkingName = parking.ParkingName,
            Shortcode = parking.Shortcode,
            Latitude = parking.Latitude,
            Longitude = parking.Longitude,
            LatLong = parking.LatLong,
            Street = parking.Street,
            PostalCode = parking.PostalCode,
            Town = parking.Town,
            Active = parking.Active,
            ManagerName = parking.ManagerName,
            ManagerPhone = parking.ManagerPhone,
            ManagerEmail = parking.ManagerEmail,
            EmergencyContact = parking.EmergencyContact,
            ParkingSpaces = parking.ParkingSpaces,
            CountryId = parking.CountryId,
            CompanyId = parking.CompanyId,
            Metadata = parking.Metadata,
            CreatedBy = parking.CreatedBy,
            LastUpdatedBy = parking.LastUpdatedBy,
            CreatedAt = parking.CreatedAt,
            UpdatedAt = parking.UpdatedAt,
            DeletedAt = parking.DeletedAt,
            Country = parking.Country != null ? new CountryDto
            {
                Id = parking.Country.Id,
                Name = parking.Country.Name,
                IsoCode = parking.Country.IsoCode,
                Metadata = parking.Country.Metadata,
                CreatedBy = parking.Country.CreatedBy,
                LastUpdatedBy = parking.Country.LastUpdatedBy,
                CreatedAt = parking.Country.CreatedAt,
                UpdatedAt = parking.Country.UpdatedAt
            } : null,
            Company = parking.Company != null ? new CompanyDto
            {
                Id = parking.Company.Id,
                Name = parking.Company.Name,
                CompanyCode = parking.Company.CompanyCode,
                CountryId = parking.Company.CountryId,
                Description = parking.Company.Description,
                CreatedAt = parking.Company.CreatedAt,
                UpdatedAt = parking.Company.UpdatedAt,
                CreatedBy = parking.Company.CreatedBy,
                LastUpdatedBy = parking.Company.LastUpdatedBy
            } : null,
            Regions = parking.RegionParkings
                .Where(rp => rp.DeletedAt == null)
                .Select(rp => new RegionDto
                {
                    Id = rp.Region.Id,
                    Name = rp.Region.Name,
                    RegionCode = rp.Region.RegionCode,
                    CompanyId = rp.Region.CompanyId,
                    Metadata = rp.Region.Metadata,
                    CreatedBy = rp.Region.CreatedBy,
                    LastUpdatedBy = rp.Region.LastUpdatedBy,
                    CreatedAt = rp.Region.CreatedAt,
                    UpdatedAt = rp.Region.UpdatedAt
                })
                .ToList()
        };
    }

    public async Task<ParkingResponseDto> CreateAsync(CreateParkingDto createParkingDto, int? createdBy = null)
    {
        // Validate region exists and get company_id and country_id from region
        var region = await _context.Regions
            .Where(r => r.Id == createParkingDto.RegionId && r.DeletedAt == null)
            .Include(r => r.Company)
            .FirstOrDefaultAsync();

        if (region == null)
            throw new ArgumentException($"Region with ID {createParkingDto.RegionId} does not exist.");

        var companyId = region.CompanyId;
        var countryId = region.Company.CountryId;

        // Check if parking code is unique within the country
        var codeExists = await ExistsByParkingCodeAndCountryAsync(createParkingDto.ParkingCode, countryId);
        if (codeExists)
            throw new ArgumentException($"Parking code '{createParkingDto.ParkingCode}' already exists in this country.");

        if (region.Company == null)
            throw new InvalidOperationException($"Region {createParkingDto.RegionId} has no associated company.");

        var parking = new Parking
        {
            ParkingCode = createParkingDto.ParkingCode,
            ParkingName = createParkingDto.ParkingName,
            CountryId = countryId,
            CompanyId = companyId,
            CreatedBy = createdBy,
            LastUpdatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Parkings.Add(parking);
        await _context.SaveChangesAsync();

        // Create the region-parking mapping
        var regionParking = new RegionParking
        {
            ParkingId = parking.Id,
            RegionId = createParkingDto.RegionId,
            ParkingCode = createParkingDto.ParkingCode,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.RegionParkings.Add(regionParking);
        await _context.SaveChangesAsync();

        return await GetByIdAsync(parking.Id) ?? throw new InvalidOperationException("Failed to retrieve created parking.");
    }

    public async Task<ParkingResponseDto?> UpdateAsync(int id, UpdateParkingDto updateParkingDto, int? updatedBy = null)
    {
        var parking = await _context.Parkings
            .Where(p => p.Id == id && p.DeletedAt == null)
            .FirstOrDefaultAsync();

        if (parking == null)
            return null;

        // Check unique code
        if (!string.IsNullOrEmpty(updateParkingDto.ParkingCode) && updateParkingDto.ParkingCode != parking.ParkingCode)
        {
            var codeExists = await ExistsByParkingCodeAndCountryAsync(updateParkingDto.ParkingCode, parking.CountryId, id);
            if (codeExists)
                throw new ArgumentException($"Parking code '{updateParkingDto.ParkingCode}' already exists in this country.");
        }

        if (!string.IsNullOrEmpty(updateParkingDto.ParkingCode)) parking.ParkingCode = updateParkingDto.ParkingCode;
        if (!string.IsNullOrEmpty(updateParkingDto.ParkingName)) parking.ParkingName = updateParkingDto.ParkingName;
        if (updateParkingDto.Shortcode != null) parking.Shortcode = updateParkingDto.Shortcode;
        if (updateParkingDto.Latitude.HasValue) parking.Latitude = updateParkingDto.Latitude;
        if (updateParkingDto.Longitude.HasValue) parking.Longitude = updateParkingDto.Longitude;
        if (updateParkingDto.Street != null) parking.Street = updateParkingDto.Street;
        if (updateParkingDto.PostalCode != null) parking.PostalCode = updateParkingDto.PostalCode;
        if (updateParkingDto.Town != null) parking.Town = updateParkingDto.Town;
        if (updateParkingDto.Active.HasValue) parking.Active = updateParkingDto.Active.Value;
        if (updateParkingDto.ManagerName != null) parking.ManagerName = updateParkingDto.ManagerName;
        if (updateParkingDto.ManagerPhone != null) parking.ManagerPhone = updateParkingDto.ManagerPhone;
        if (updateParkingDto.ManagerEmail != null) parking.ManagerEmail = updateParkingDto.ManagerEmail;
        if (updateParkingDto.EmergencyContact != null) parking.EmergencyContact = updateParkingDto.EmergencyContact;
        if (updateParkingDto.ParkingSpaces.HasValue) parking.ParkingSpaces = updateParkingDto.ParkingSpaces;
        if (updateParkingDto.Metadata != null) parking.Metadata = updateParkingDto.Metadata;

        parking.LastUpdatedBy = updatedBy;
        parking.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return await GetByIdAsync(id);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var parking = await _context.Parkings
            .Where(p => p.Id == id && p.DeletedAt == null)
            .FirstOrDefaultAsync();

        if (parking == null)
            return false;

        parking.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Parkings.AnyAsync(p => p.Id == id && p.DeletedAt == null);
    }

    public async Task<bool> ExistsByParkingCodeAndCountryAsync(string parkingCode, int countryId, int? excludeId = null)
    {
        var query = _context.Parkings
            .Where(p => p.ParkingCode == parkingCode && p.CountryId == countryId && p.DeletedAt == null);

        if (excludeId.HasValue)
            query = query.Where(p => p.Id != excludeId.Value);

        return await query.AnyAsync();
    }

    public async Task<IEnumerable<ParkingListItemDto>> GetByCompanyIdAsync(int companyId)
    {
        return await _context.Parkings
            .Where(p => p.CompanyId == companyId && p.DeletedAt == null)
            .Include(p => p.Company)
            .Include(p => p.Country)
            .Select(p => new ParkingListItemDto
            {
                Id = p.Id,
                ParkingCode = p.ParkingCode,
                ParkingName = p.ParkingName,
                Town = p.Town,
                Active = p.Active,
                ParkingSpaces = p.ParkingSpaces,
                CompanyId = p.CompanyId,
                CompanyName = p.Company.Name,
                CountryId = p.CountryId,
                CountryName = p.Country.Name,
                Latitude = p.Latitude,
                Longitude = p.Longitude,
                LatLong = p.LatLong,
                Street = p.Street,
                PostalCode = p.PostalCode,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            })
            .OrderBy(p => p.ParkingName)
            .ToListAsync();
    }

    public async Task<IEnumerable<ParkingListItemDto>> GetByCountryIdAsync(int countryId)
    {
        return await _context.Parkings
            .Where(p => p.CountryId == countryId && p.DeletedAt == null)
            .Include(p => p.Company)
            .Include(p => p.Country)
            .Select(p => new ParkingListItemDto
            {
                Id = p.Id,
                ParkingCode = p.ParkingCode,
                ParkingName = p.ParkingName,
                Town = p.Town,
                Active = p.Active,
                ParkingSpaces = p.ParkingSpaces,
                CompanyId = p.CompanyId,
                CompanyName = p.Company.Name,
                CountryId = p.CountryId,
                CountryName = p.Country.Name,
                Latitude = p.Latitude,
                Longitude = p.Longitude,
                LatLong = p.LatLong,
                Street = p.Street,
                PostalCode = p.PostalCode,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            })
            .OrderBy(p => p.ParkingName)
            .ToListAsync();
    }

    public async Task<IEnumerable<ParkingListItemDto>> GetByRegionIdAsync(int regionId)
    {
        return await _context.RegionParkings
            .Where(rp => rp.RegionId == regionId && rp.DeletedAt == null)
            .Include(rp => rp.Parking)
            .ThenInclude(p => p.Company)
            .Include(rp => rp.Parking)
            .ThenInclude(p => p.Country)
            .Where(rp => rp.Parking.DeletedAt == null)
            .Select(rp => new ParkingListItemDto
            {
                Id = rp.Parking.Id,
                ParkingCode = rp.Parking.ParkingCode,
                ParkingName = rp.Parking.ParkingName,
                Town = rp.Parking.Town,
                Active = rp.Parking.Active,
                ParkingSpaces = rp.Parking.ParkingSpaces,
                CompanyId = rp.Parking.CompanyId,
                CompanyName = rp.Parking.Company.Name,
                CountryId = rp.Parking.CountryId,
                CountryName = rp.Parking.Country.Name,
                Latitude = rp.Parking.Latitude,
                Longitude = rp.Parking.Longitude,
                LatLong = rp.Parking.LatLong,
                Street = rp.Parking.Street,
                PostalCode = rp.Parking.PostalCode,
                CreatedAt = rp.Parking.CreatedAt,
                UpdatedAt = rp.Parking.UpdatedAt
            })
            .OrderBy(p => p.ParkingName)
            .ToListAsync();
    }

    public async Task<IEnumerable<ParkingListItemDto>> SearchAsync(string searchTerm)
    {
        var normalized = searchTerm.ToLower().Trim();

        return await _context.Parkings
            .Where(p => p.DeletedAt == null &&
                        (p.ParkingCode.ToLower().Contains(normalized) ||
                         p.ParkingName.ToLower().Contains(normalized) ||
                         (p.Town != null && p.Town.ToLower().Contains(normalized))))
            .Include(p => p.Company)
            .Include(p => p.Country)
            .Select(p => new ParkingListItemDto
            {
                Id = p.Id,
                ParkingCode = p.ParkingCode,
                ParkingName = p.ParkingName,
                Town = p.Town,
                Active = p.Active,
                ParkingSpaces = p.ParkingSpaces,
                CompanyId = p.CompanyId,
                CompanyName = p.Company.Name,
                CountryId = p.CountryId,
                CountryName = p.Country.Name,
                Latitude = p.Latitude,
                Longitude = p.Longitude,
                LatLong = p.LatLong,
                Street = p.Street,
                PostalCode = p.PostalCode,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            })
            .OrderBy(p => p.ParkingName)
            .ToListAsync();
    }

    public async Task<RegionParkingDto> AssignParkingToRegionAsync(int parkingId, int regionId, string? parkingCode = null, int? createdBy = null)
    {
        var parkingExists = await ExistsAsync(parkingId);
        if (!parkingExists)
            throw new ArgumentException($"Parking with ID {parkingId} does not exist.");

        var region = await _context.Regions
            .Where(r => r.Id == regionId && r.DeletedAt == null)
            .FirstOrDefaultAsync();

        if (region == null)
            throw new ArgumentException($"Region with ID {regionId} does not exist.");

        var already = await IsParkingAssignedToRegionAsync(parkingId, regionId);
        if (already)
            throw new ArgumentException($"Parking {parkingId} is already assigned to Region {regionId}.");

        var parking = await _context.Parkings.FindAsync(parkingId);
        if (parking!.CompanyId != region.CompanyId)
            throw new ArgumentException("Parking and Region must belong to the same company.");

        var regionParking = new RegionParking
        {
            ParkingId = parkingId,
            RegionId = regionId,
            ParkingCode = parkingCode,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.RegionParkings.Add(regionParking);
        await _context.SaveChangesAsync();

        var result = await _context.RegionParkings
            .Where(rp => rp.Id == regionParking.Id)
            .Include(rp => rp.Parking)
            .Include(rp => rp.Region)
            .Select(rp => new RegionParkingDto
            {
                Id = rp.Id,
                ParkingId = rp.ParkingId,
                ParkingName = rp.Parking.ParkingName,
                ParkingCode = rp.Parking.ParkingCode,
                RegionId = rp.RegionId,
                RegionName = rp.Region.Name,
                RegionParkingCode = rp.ParkingCode,
                Metadata = rp.Metadata,
                CreatedBy = rp.CreatedBy,
                CreatedAt = rp.CreatedAt,
                UpdatedAt = rp.UpdatedAt
            })
            .FirstOrDefaultAsync();

        return result ?? throw new InvalidOperationException("Failed to retrieve created region-parking mapping.");
    }

    public async Task<bool> RemoveParkingFromRegionAsync(int parkingId, int regionId)
    {
        var rp = await _context.RegionParkings
            .Where(r => r.ParkingId == parkingId && r.RegionId == regionId && r.DeletedAt == null)
            .FirstOrDefaultAsync();

        if (rp == null)
            return false;

        rp.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> IsParkingAssignedToRegionAsync(int parkingId, int regionId)
    {
        return await _context.RegionParkings
            .AnyAsync(r => r.ParkingId == parkingId && r.RegionId == regionId && r.DeletedAt == null);
    }
}