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
    private readonly ILogger<CategoriesController> _logger;

    public CategoriesController(IMasterDataRepository repository, ICloudinaryService cloudinary, ILogger<CategoriesController> logger)
    {
        _repository = repository;
        _cloudinary = cloudinary;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] int? id, [FromQuery] string? slug)
    {
        try
        {
            var categories = await _repository.GetCategoriesAsync(search, id, slug);
            return Ok(categories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching categories");
            return StatusCode(500, new { message = ex.Message });
        }
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Create([FromForm] CategoryRequest request)
    {
        try
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating category");
            return StatusCode(500, new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Update(int id, [FromForm] CategoryRequest request)
    {
        try
        {
            // 1. Fetch existing
            var existing = (await _repository.GetCategoriesAsync(id: id)).FirstOrDefault();
            if (existing == null) return NotFound();

            // 2. Merge changes
            existing.Name = request.Name;
            existing.Slug = SlugHelper.Generate(request.Name);

            // Only upload if a new file is provided
            if (request.Image != null)
            {
                if (!string.IsNullOrEmpty(existing.Image)) await _cloudinary.DeleteFileAsync(existing.Image);
                existing.Image = await _cloudinary.UploadImageAsync(request.Image, "categories");
            }

            // 3. Save
            var success = await _repository.UpdateCategoryAsync(existing);
            return success ? Ok(existing) : BadRequest();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating category");
            return StatusCode(500, new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var category = (await _repository.GetCategoriesAsync(id: id)).FirstOrDefault();
            if (category != null && !string.IsNullOrEmpty(category.Image)) await _cloudinary.DeleteFileAsync(category.Image);

            var success = await _repository.DeleteCategoryAsync(id);
            return success ? NoContent() : NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting category");
            return StatusCode(500, new { message = ex.Message });
        }
    }
}
