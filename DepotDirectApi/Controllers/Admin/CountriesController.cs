using DepotDirectApi.Models.DTOs;
using DepotDirectApi.Models.Entities;
using DepotDirectApi.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;

namespace DepotDirectApi.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/[controller]")]
    [Authorize(Roles = "Admin")]
    [Tags("Admin - Countries")]
    public class CountriesController : BaseController
    {
        private readonly ICountryRepository _countryRepository;
        private readonly ILogger<CountriesController> _logger;

        public CountriesController(ICountryRepository countryRepository, ILogger<CountriesController> logger)
        {
            _countryRepository = countryRepository;
            _logger = logger;
        }

        /// <summary>
        /// Get all countries with pagination and optional search
        /// </summary>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="pageSize">Items per page (default: 50, max: 100)</param>
        /// <param name="search">Search term for country name</param>
        /// <returns>Paginated list of countries</returns>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<CountryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<PagedResult<CountryDto>>> GetCountries(
            [FromQuery] int page = 1, 
            [FromQuery] int pageSize = 50, 
            [FromQuery] string? search = null)
        {
            try
            {
                // Validate pagination parameters
                if (page < 1) page = 1;
                if (pageSize < 1) pageSize = 50;
                if (pageSize > 100) pageSize = 100;

                var result = await _countryRepository.GetAllAsync(page, pageSize, search);
                
                var response = new PagedResult<CountryDto>
                {
                    Data = result.Data.Select(c => new CountryDto
                    {
                        Id = c.Id,
                        Name = c.Name,
                        IsoCode = c.IsoCode,
                        Metadata = c.Metadata != null ? JsonSerializer.Deserialize<object>(c.Metadata.RootElement.GetRawText()) : null,
                        CreatedAt = c.CreatedAt,
                        UpdatedAt = c.UpdatedAt,
                        CreatedBy = c.CreatedBy,
                        LastUpdatedBy = c.LastUpdatedBy
                    }).ToList(),
                    TotalCount = result.TotalCount,
                    Page = result.Page,
                    PageSize = result.PageSize,
                    TotalPages = result.TotalPages,
                    HasNextPage = result.HasNextPage,
                    HasPreviousPage = result.HasPreviousPage
                };

                _logger.LogInformation("Retrieved {Count} countries for page {Page}", response.Data.Count, page);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving countries");
                return StatusCode(500, "An error occurred while retrieving countries");
            }
        }

        /// <summary>
        /// Get a specific country by ID with statistics
        /// </summary>
        /// <param name="id">Country ID</param>
        /// <returns>Country details with statistics</returns>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(CountryDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<CountryDto>> GetCountry(int id)
        {
            try
            {
                var country = await _countryRepository.GetWithStatsAsync(id);
                if (country == null)
                {
                    _logger.LogWarning("Country with ID {CountryId} not found", id);
                    return NotFound($"Country with ID {id} not found");
                }

                _logger.LogInformation("Retrieved country {CountryId}: {CountryName}", id, country.Name);
                return Ok(country);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving country {CountryId}", id);
                return StatusCode(500, "An error occurred while retrieving the country");
            }
        }

        /// <summary>
        /// Create a new country
        /// </summary>
        /// <param name="dto">Country creation data</param>
        /// <returns>Created country</returns>
        [HttpPost]
        [ProducesResponseType(typeof(CountryDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<CountryDto>> CreateCountry([FromBody] CountryCreateDto dto)
        {
            try
            {
                // Validation
                if (string.IsNullOrWhiteSpace(dto.Name))
                {
                    return BadRequest("Country name is required");
                }

                if (await _countryRepository.ExistsByNameAsync(dto.Name))
                {
                    return Conflict($"A country with name '{dto.Name}' already exists");
                }

                if (!string.IsNullOrWhiteSpace(dto.IsoCode) && await _countryRepository.ExistsByIsoCodeAsync(dto.IsoCode))
                {
                    return Conflict($"A country with ISO code '{dto.IsoCode}' already exists");
                }

                var userId = GetCurrentUserId();

                var country = new Country
                {
                    Name = dto.Name.Trim(),
                    IsoCode = dto.IsoCode?.Trim(),
                    Metadata = dto.Metadata != null ? JsonDocument.Parse(JsonSerializer.Serialize(dto.Metadata)) : null,
                    CreatedBy = userId,
                    LastUpdatedBy = userId
                };

                var createdCountry = await _countryRepository.CreateAsync(country);
                
                var response = new CountryDto
                {
                    Id = createdCountry.Id,
                    Name = createdCountry.Name,
                    IsoCode = createdCountry.IsoCode,
                    Metadata = createdCountry.Metadata != null ? JsonSerializer.Deserialize<object>(createdCountry.Metadata.RootElement.GetRawText()) : null,
                    CreatedAt = createdCountry.CreatedAt,
                    UpdatedAt = createdCountry.UpdatedAt,
                    CreatedBy = createdCountry.CreatedBy,
                    LastUpdatedBy = createdCountry.LastUpdatedBy
                };

                _logger.LogInformation("Created country {CountryId}: {CountryName} by user {UserId}", 
                    createdCountry.Id, createdCountry.Name, userId);

                return CreatedAtAction(nameof(GetCountry), new { id = createdCountry.Id }, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating country");
                return StatusCode(500, "An error occurred while creating the country");
            }
        }

        /// <summary>
        /// Update an existing country
        /// </summary>
        /// <param name="id">Country ID</param>
        /// <param name="dto">Country update data</param>
        /// <returns>Updated country</returns>
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(CountryDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<CountryDto>> UpdateCountry(int id, [FromBody] CountryUpdateDto dto)
        {
            try
            {
                if (!await _countryRepository.ExistsAsync(id))
                {
                    return NotFound($"Country with ID {id} not found");
                }

                // Validation
                if (!string.IsNullOrWhiteSpace(dto.Name) && await _countryRepository.ExistsByNameAsync(dto.Name, id))
                {
                    return Conflict($"A country with name '{dto.Name}' already exists");
                }

                if (!string.IsNullOrWhiteSpace(dto.IsoCode) && await _countryRepository.ExistsByIsoCodeAsync(dto.IsoCode, id))
                {
                    return Conflict($"A country with ISO code '{dto.IsoCode}' already exists");
                }

                var userId = GetCurrentUserId();

                var existingCountry = await _countryRepository.GetByIdAsync(id);
                if (existingCountry == null)
                {
                    return NotFound($"Country with ID {id} not found");
                }

                // Update only provided fields
                if (!string.IsNullOrWhiteSpace(dto.Name))
                    existingCountry.Name = dto.Name.Trim();
                
                if (dto.IsoCode != null) // Allow setting to null
                    existingCountry.IsoCode = string.IsNullOrWhiteSpace(dto.IsoCode) ? null : dto.IsoCode.Trim();
                
                if (dto.Metadata != null)
                    existingCountry.Metadata = JsonDocument.Parse(JsonSerializer.Serialize(dto.Metadata));

                existingCountry.LastUpdatedBy = userId;

                var updatedCountry = await _countryRepository.UpdateAsync(id, existingCountry);
                if (updatedCountry == null)
                {
                    return NotFound($"Country with ID {id} not found");
                }

                var response = new CountryDto
                {
                    Id = updatedCountry.Id,
                    Name = updatedCountry.Name,
                    IsoCode = updatedCountry.IsoCode,
                    Metadata = updatedCountry.Metadata != null ? JsonSerializer.Deserialize<object>(updatedCountry.Metadata.RootElement.GetRawText()) : null,
                    CreatedAt = updatedCountry.CreatedAt,
                    UpdatedAt = updatedCountry.UpdatedAt,
                    CreatedBy = updatedCountry.CreatedBy,
                    LastUpdatedBy = updatedCountry.LastUpdatedBy
                };

                _logger.LogInformation("Updated country {CountryId}: {CountryName} by user {UserId}", 
                    id, updatedCountry.Name, userId);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating country {CountryId}", id);
                return StatusCode(500, "An error occurred while updating the country");
            }
        }

        /// <summary>
        /// Delete a country (soft delete)
        /// </summary>
        /// <param name="id">Country ID</param>
        /// <returns>No content if successful</returns>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> DeleteCountry(int id)
        {
            try
            {
                if (!await _countryRepository.ExistsAsync(id))
                {
                    return NotFound($"Country with ID {id} not found");
                }

                var deleted = await _countryRepository.DeleteAsync(id);
                if (!deleted)
                {
                    return NotFound($"Country with ID {id} not found");
                }

                var userId = GetCurrentUserId();
                _logger.LogInformation("Deleted country {CountryId} by user {UserId}", id, userId);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting country {CountryId}", id);
                return StatusCode(500, "An error occurred while deleting the country");
            }
        }
    }
}