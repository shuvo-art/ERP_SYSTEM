using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductApi.Core.Entities;
using ProductApi.Core.Interfaces;

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
    public async Task<IActionResult> GetAll() => Ok(await _repository.GetCategoriesAsync());

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Create([FromForm] string name, [FromForm] IFormFile? image)
    {
        var category = new CategoryMaster { Name = name };
        if (image != null) category.Image = await _cloudinary.UploadImageAsync(image, "categories");
        var id = await _repository.CreateCategoryAsync(category);
        category.Id = id;
        return CreatedAtAction(nameof(GetAll), new { id = category.Id }, category);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Update(int id, [FromForm] string name, [FromForm] IFormFile? image)
    {
        var category = new CategoryMaster { Id = id, Name = name };
        if (image != null) category.Image = await _cloudinary.UploadImageAsync(image, "categories");
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
