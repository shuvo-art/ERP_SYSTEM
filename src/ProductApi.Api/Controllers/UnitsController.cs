using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductApi.Core.Entities;
using ProductApi.Core.Interfaces;
using ProductApi.Core.DTOs;

namespace ProductApi.Api.Controllers;

[ApiController]
[Route("api/v1/units")]
public class UnitsController : ControllerBase
{
    private readonly IMasterDataRepository _repository;
    private readonly ILogger<UnitsController> _logger;

    public UnitsController(IMasterDataRepository repository, ILogger<UnitsController> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] int? id) 
    {
        try
        {
            var results = await _repository.GetUnitsAsync(search, id);
            return Ok(results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching units");
            return StatusCode(500, new { message = "An unexpected error occurred." });
        }
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] UnitRequest request)
    {
        try
        {
            var unit = new UnitMaster { Name = request.Name };
            var id = await _repository.CreateUnitAsync(unit);
            unit.Id = id;
            return CreatedAtAction(nameof(GetAll), new { id = unit.Id }, unit);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating unit");
            return StatusCode(500, new { message = "An unexpected error occurred." });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] UnitRequest request)
    {
        try
        {
            var existing = (await _repository.GetUnitsAsync(id: id)).FirstOrDefault();
            if (existing == null) return NotFound();

            existing.Name = request.Name;

            var success = await _repository.UpdateUnitAsync(existing);
            return success ? Ok(existing) : BadRequest();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating unit");
            return StatusCode(500, new { message = "An unexpected error occurred." });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var success = await _repository.DeleteUnitAsync(id);
            return success ? NoContent() : NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting unit");
            return StatusCode(500, new { message = "An unexpected error occurred." });
        }
    }
}
