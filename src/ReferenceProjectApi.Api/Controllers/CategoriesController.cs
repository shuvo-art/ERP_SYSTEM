using Microsoft.AspNetCore.Mvc;
using ReferenceProjectApi.Core.Entities;
using ReferenceProjectApi.Core.Interfaces;
using Microsoft.AspNetCore.Http;

namespace ReferenceProjectApi.Api.Controllers;

[ApiController]
[Route("api/v1/categories")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ICloudinaryService _cloudinaryService;

    public CategoriesController(ICategoryRepository categoryRepository, ICloudinaryService cloudinaryService)
    {
        _categoryRepository = categoryRepository;
        _cloudinaryService = cloudinaryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetCategories()
    {
        var categories = await _categoryRepository.GetAllAsync();
        return Ok(categories);
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> CreateCategory([FromForm] string name, [FromForm] IFormFile? image, [FromForm] string? slug)
    {
        if (string.IsNullOrEmpty(name)) return BadRequest("Category name is required.");

        var imageUrl = string.Empty;
        if (image != null)
        {
            imageUrl = await _cloudinaryService.UploadImageAsync(image, "categories");
        }

        var category = new ProjectCategory
        {
            Name = name,
            ImageUrl = imageUrl,
            Slug = slug ?? name.ToLower().Replace(" ", "-"),
            CreatedAt = DateTime.UtcNow
        };

        var id = await _categoryRepository.CreateAsync(category);
        return CreatedAtAction(nameof(GetCategories), new { id }, category);
    }
}
