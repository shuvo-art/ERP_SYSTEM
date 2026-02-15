using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductApi.Core.Entities;
using ProductApi.Core.Interfaces;

namespace ProductApi.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class UnitsController : ControllerBase
{
    private readonly IMasterDataRepository _repository;

    public UnitsController(IMasterDataRepository repository)
    {
        _repository = repository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _repository.GetUnitsAsync());

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] UnitMaster unit)
    {
        var id = await _repository.CreateUnitAsync(unit);
        unit.Id = id;
        return CreatedAtAction(nameof(GetAll), new { id = unit.Id }, unit);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] UnitMaster unit)
    {
        unit.Id = id;
        var success = await _repository.UpdateUnitAsync(unit);
        return success ? Ok(unit) : BadRequest();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _repository.DeleteUnitAsync(id);
        return success ? NoContent() : NotFound();
    }
}
