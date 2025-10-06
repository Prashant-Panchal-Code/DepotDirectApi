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
    [Authorize(Roles = "Admin")] // Fixed: Changed from "admin" to "Admin" to match JWT token
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
        /// Get all countries
        /// </summary>
        /// <returns>List of all countries</returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<CountryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<List<CountryDto>>> GetCountries()
        {
            try
            {
                // Get all countries without pagination or search
                var result = await _countryRepository.GetAllAsync(1, int.MaxValue, null);
                
                var response = result.Data.Select(c => new CountryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    IsoCode = c.IsoCode,
                    Metadata = c.Metadata != null ? JsonSerializer.Deserialize<object>(c.Metadata.RootElement.GetRawText()) : null,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt,
                    CreatedBy = c.CreatedBy,
                    LastUpdatedBy = c.LastUpdatedBy
                }).ToList();

                _logger.LogInformation("Retrieved {Count} countries", response.Count);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving countries");
                return StatusCode(500, "An error occurred while retrieving countries");
            }
        }

        /// <summary>
        /// Get all countries with company and region counts
        /// </summary>
        /// <returns>List of all countries with statistics</returns>
        [HttpGet("with-stats")]
        [ProducesResponseType(typeof(List<CountryWithStatsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<List<CountryWithStatsDto>>> GetCountriesWithStats()
        {
            try
            {
                var result = await _countryRepository.GetAllWithStatsAsync();
                
                _logger.LogInformation("Retrieved {Count} countries with stats", result.Count);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving countries with stats");
                return StatusCode(500, "An error occurred while retrieving countries with statistics");
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