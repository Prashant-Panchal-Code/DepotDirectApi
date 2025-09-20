using DepotDirectApi.Models.DTOs;
using DepotDirectApi.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DepotDirectApi.Controllers.Admin;

[ApiController]
[Route("api/admin/[controller]")]
[Authorize]
public class CompaniesController : BaseController
{
    private readonly ICompanyRepository _companyRepository;
    private readonly ILogger<CompaniesController> _logger;

    public CompaniesController(ICompanyRepository companyRepository, ILogger<CompaniesController> logger)
    {
        _companyRepository = companyRepository;
        _logger = logger;
    }

    /// <summary>
    /// Get all companies
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CompanyListItemDto>>> GetAllCompanies()
    {
        try
        {
            var companies = await _companyRepository.GetAllAsync();
            return Ok(companies);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving companies");
            return StatusCode(500, "An error occurred while retrieving companies");
        }
    }

    /// <summary>
    /// Get a company by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<CompanyResponseDto>> GetCompany(int id)
    {
        try
        {
            if (id <= 0)
                return BadRequest("Invalid company ID");

            var company = await _companyRepository.GetByIdAsync(id);
            
            if (company == null)
                return NotFound($"Company with ID {id} not found");

            return Ok(company);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving company with ID {CompanyId}", id);
            return StatusCode(500, "An error occurred while retrieving the company");
        }
    }

    /// <summary>
    /// Create a new company
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<CompanyResponseDto>> CreateCompany([FromBody] CreateCompanyDto createCompanyDto)
    {
        try
        { 
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var createdBy = GetCurrentUserId();
            var company = await _companyRepository.CreateAsync(createCompanyDto, createdBy);
            
            return CreatedAtAction(nameof(GetCompany), new { id = company.Id }, company);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation error while creating company");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating company");
            return StatusCode(500, "An error occurred while creating the company");
        }
    }

    /// <summary>
    /// Update an existing company
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<CompanyResponseDto>> UpdateCompany(int id, [FromBody] UpdateCompanyDto updateCompanyDto)
    {
        try
        {
            if (id <= 0)
                return BadRequest("Invalid company ID");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updatedBy = GetCurrentUserId();
            var company = await _companyRepository.UpdateAsync(id, updateCompanyDto, updatedBy);
            
            if (company == null)
                return NotFound($"Company with ID {id} not found");

            return Ok(company);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation error while updating company with ID {CompanyId}", id);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating company with ID {CompanyId}", id);
            return StatusCode(500, "An error occurred while updating the company");
        }
    }

    /// <summary>
    /// Delete a company (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteCompany(int id)
    {
        try
        {
            if (id <= 0)
                return BadRequest("Invalid company ID");

            var success = await _companyRepository.DeleteAsync(id);
            
            if (!success)
                return NotFound($"Company with ID {id} not found");

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting company with ID {CompanyId}", id);
            return StatusCode(500, "An error occurred while deleting the company");
        }
    }

    /// <summary>
    /// Get companies by country ID
    /// </summary>
    [HttpGet("by-country/{countryId}")]
    public async Task<ActionResult<IEnumerable<CompanyListItemDto>>> GetCompaniesByCountry(int countryId)
    {
        try
        {
            if (countryId <= 0)
                return BadRequest("Invalid country ID");

            var companies = await _companyRepository.GetByCountryIdAsync(countryId);
            return Ok(companies);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving companies for country ID {CountryId}", countryId);
            return StatusCode(500, "An error occurred while retrieving companies");
        }
    }

    /// <summary>
    /// Search companies
    /// </summary>
    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<CompanyListItemDto>>> SearchCompanies([FromQuery] string searchTerm)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return BadRequest("Search term is required");

            if (searchTerm.Length < 2)
                return BadRequest("Search term must be at least 2 characters long");

            var companies = await _companyRepository.SearchAsync(searchTerm);
            return Ok(companies);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching companies with term '{SearchTerm}'", searchTerm);
            return StatusCode(500, "An error occurred while searching companies");
        }
    }

    /// <summary>
    /// Check if a company exists
    /// </summary>
    [HttpHead("{id}")]
    public async Task<ActionResult> CompanyExists(int id)
    {
        try
        {
            if (id <= 0)
                return BadRequest();

            var exists = await _companyRepository.ExistsAsync(id);
            return exists ? Ok() : NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if company exists with ID {CompanyId}", id);
            return StatusCode(500);
        }
    }

    /// <summary>
    /// Get regions assigned to a company
    /// </summary>
    [HttpGet("{id}/regions")]
    public async Task<ActionResult<IEnumerable<RegionListItemDto>>> GetCompanyRegions(int id)
    {
        try
        {
            if (id <= 0)
                return BadRequest("Invalid company ID");

            var regions = await _companyRepository.GetRegionsByCompanyIdAsync(id);
            return Ok(regions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving regions for company ID {CompanyId}", id);
            return StatusCode(500, "An error occurred while retrieving company regions");
        }
    }
}