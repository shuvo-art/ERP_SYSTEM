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
            return StatusCode(500, new { message = ex.Message });
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating country");
            return StatusCode(500, new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] CountryRequest request)
    {
        try
        {
            var existing = (await _repository.GetCountriesAsync(id: id)).FirstOrDefault();
            if (existing == null) return NotFound();

            existing.Name = request.Name;

            var success = await _repository.UpdateCountryAsync(existing);
            return success ? Ok(existing) : BadRequest();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating country");
            return StatusCode(500, new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var success = await _repository.DeleteCountryAsync(id);
            return success ? NoContent() : NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting country");
            return StatusCode(500, new { message = ex.Message });
        }
    }
}
