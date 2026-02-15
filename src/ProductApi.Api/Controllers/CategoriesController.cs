using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductApi.Core.Entities;
using ProductApi.Core.Interfaces;
using ProductApi.Core.DTOs;
using ProductApi.Core.Helpers;

namespace ProductApi.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly IMasterDataRepository _repository;
    private readonly ICloudinaryService _cloudinary;

    public CategoriesController(IMasterDataRepository repository, ICloudinaryService cloudinary)
    {
        _repository = repository;
        _cloudinary = cloudinary;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] int? id, [FromQuery] string? slug) 
        => Ok(await _repository.GetCategoriesAsync(search, id, slug));

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Create([FromForm] CategoryRequest request)
    {
        var category = new CategoryMaster 
        { 
            Name = request.Name,
            Slug = SlugHelper.Generate(request.Name)
        };
        if (request.Image != null) category.Image = await _cloudinary.UploadImageAsync(request.Image, "categories");
        var id = await _repository.CreateCategoryAsync(category);
        category.Id = id;
        return CreatedAtAction(nameof(GetAll), new { id = category.Id }, category);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Update(int id, [FromForm] CategoryRequest request)
    {
        var category = new CategoryMaster 
        { 
            Id = id, 
            Name = request.Name,
            Slug = SlugHelper.Generate(request.Name)
        };
        if (request.Image != null) category.Image = await _cloudinary.UploadImageAsync(request.Image, "categories");
        var success = await _repository.UpdateCategoryAsync(category);
        return success ? Ok(category) : BadRequest();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _repository.DeleteCategoryAsync(id);
        return success ? NoContent() : NotFound();
    }
}
