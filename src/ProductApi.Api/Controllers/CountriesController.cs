using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductApi.Core.Entities;
using ProductApi.Core.Interfaces;
using ProductApi.Core.DTOs;

namespace ProductApi.Api.Controllers;

[ApiController]
[Route("api/v1/countries")]
public class CountriesController : ControllerBase
{
    private readonly IMasterDataRepository _repository;
    private readonly ILogger<CountriesController> _logger;

    public CountriesController(IMasterDataRepository repository, ILogger<CountriesController> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] int? id) 
    {
        try
        {
            var results = await _repository.GetCountriesAsync(search, id);
            return Ok(results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching countries");
            return StatusCode(500, new { message = "An error occurred while fetching countries." });
        }
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CountryRequest request)
    {
        try
        {
            var country = new CountryMaster { Name = request.Name };
            var id = await _repository.CreateCountryAsync(country);
            country.Id = id;
            return CreatedAtAction(nameof(GetAll), new { id = country.Id }, country);
        }
        catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
        {
            _logger.LogWarning(ex, "Duplicate country name attempted: {Name}", request.Name);
            return Conflict(new { message = "A country with this name already exists." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating country");
            return StatusCode(500, new { message = "An error occurred while creating the country." });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] CountryRequest request)
    {
        try
        {
            var existing = (await _repository.GetCountriesAsync(id: id)).FirstOrDefault();
            if (existing == null) return NotFound(new { message = "Country not found." });

            existing.Name = request.Name;

            var success = await _repository.UpdateCountryAsync(existing);
            return success ? Ok(existing) : BadRequest(new { message = "Failed to update country." });
        }
        catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
        {
            _logger.LogWarning(ex, "Duplicate country name attempted: {Name}", request.Name);
            return Conflict(new { message = "A country with this name already exists." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating country");
            return StatusCode(500, new { message = "An error occurred while updating the country." });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var success = await _repository.DeleteCountryAsync(id);
            return success ? NoContent() : NotFound(new { message = "Country not found." });
        }
        catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Number == 547) // Foreign key violation
        {
            _logger.LogWarning(ex, "Attempted to delete a country that is in use (ID: {Id})", id);
            return Conflict(new { message = "Cannot delete this country because it is associated with other records." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting country");
            return StatusCode(500, new { message = "An error occurred while deleting the country." });
        }
    }
}
