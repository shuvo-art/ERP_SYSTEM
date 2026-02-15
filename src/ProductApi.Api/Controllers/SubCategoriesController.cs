using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductApi.Core.Entities;
using ProductApi.Core.Interfaces;

namespace ProductApi.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class SubCategoriesController : ControllerBase
{
    private readonly IMasterDataRepository _repository;

    public SubCategoriesController(IMasterDataRepository repository)
    {
        _repository = repository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _repository.GetSubCategoriesAsync());

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] SubCategoryMaster subCategory)
    {
        var id = await _repository.CreateSubCategoryAsync(subCategory);
        subCategory.Id = id;
        return CreatedAtAction(nameof(GetAll), new { id = subCategory.Id }, subCategory);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] SubCategoryMaster subCategory)
    {
        subCategory.Id = id;
        var success = await _repository.UpdateSubCategoryAsync(subCategory);
        return success ? Ok(subCategory) : BadRequest();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _repository.DeleteSubCategoryAsync(id);
        return success ? NoContent() : NotFound();
    }
}
