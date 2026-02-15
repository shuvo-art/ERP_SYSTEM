using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductApi.Core.Entities;
using ProductApi.Core.Interfaces;
using ProductApi.Core.DTOs;
using ProductApi.Core.Helpers;

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
    public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] int? id, [FromQuery] string? slug) 
        => Ok(await _repository.GetSubCategoriesAsync(search, id, slug));

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] SubCategoryRequest request)
    {
        var subCategory = new SubCategoryMaster 
        { 
            CategoryId = request.CategoryId, 
            Name = request.Name,
            Slug = SlugHelper.Generate(request.Name)
        };
        var id = await _repository.CreateSubCategoryAsync(subCategory);
        subCategory.Id = id;
        return CreatedAtAction(nameof(GetAll), new { id = subCategory.Id }, subCategory);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] SubCategoryRequest request)
    {
        var subCategory = new SubCategoryMaster 
        { 
            Id = id, 
            CategoryId = request.CategoryId, 
            Name = request.Name,
            Slug = SlugHelper.Generate(request.Name)
        };
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
