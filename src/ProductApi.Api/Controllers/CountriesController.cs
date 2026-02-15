using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductApi.Core.Entities;
using ProductApi.Core.Interfaces;

namespace ProductApi.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class CountriesController : ControllerBase
{
    private readonly IMasterDataRepository _repository;

    public CountriesController(IMasterDataRepository repository)
    {
        _repository = repository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _repository.GetCountriesAsync());

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CountryMaster country)
    {
        var id = await _repository.CreateCountryAsync(country);
        country.Id = id;
        return CreatedAtAction(nameof(GetAll), new { id = country.Id }, country);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] CountryMaster country)
    {
        country.Id = id;
        var success = await _repository.UpdateCountryAsync(country);
        return success ? Ok(country) : BadRequest();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _repository.DeleteCountryAsync(id);
        return success ? NoContent() : NotFound();
    }
}
