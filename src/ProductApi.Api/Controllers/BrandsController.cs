using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductApi.Core.Entities;
using ProductApi.Core.Interfaces;
using ProductApi.Core.DTOs;
using ProductApi.Core.Helpers;

namespace ProductApi.Api.Controllers;

[ApiController]
[Route("api/v1/brands")]
public class BrandsController : ControllerBase
{
    private readonly IMasterDataRepository _repository;
    private readonly ICloudinaryService _cloudinary;
    private readonly ILogger<BrandsController> _logger;

    public BrandsController(IMasterDataRepository repository, ICloudinaryService cloudinary, ILogger<BrandsController> logger)
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
            var brands = await _repository.GetBrandsAsync(search, id, slug);
            return Ok(brands);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching brands");
            return StatusCode(500, new { message = "An unexpected error occurred." });
        }
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Create([FromForm] BrandRequest request)
    {
        try
        {
            var brand = new BrandMaster 
            { 
                Name = request.Name,
                Slug = SlugHelper.Generate(request.Name)
            };
            if (request.Logo != null) brand.Logo = await _cloudinary.UploadImageAsync(request.Logo, "brands");
            var id = await _repository.CreateBrandAsync(brand);
            brand.Id = id;
            return CreatedAtAction(nameof(GetAll), new { id = brand.Id }, brand);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating brand");
            return StatusCode(500, new { message = "An unexpected error occurred." });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Update(int id, [FromForm] BrandRequest request)
    {
        try
        {
            // 1. Fetch existing record
            var existing = (await _repository.GetBrandsAsync(id: id)).FirstOrDefault();
            if (existing == null) return NotFound();

            // 2. Merge changes
            existing.Name = request.Name;
            existing.Slug = SlugHelper.Generate(request.Name);
            
            // Only update logo if a new file is uploaded
            if (request.Logo != null)
            {
                // Optional: Delete old image from Cloudinary
                if (!string.IsNullOrEmpty(existing.Logo)) await _cloudinary.DeleteFileAsync(existing.Logo);
                existing.Logo = await _cloudinary.UploadImageAsync(request.Logo, "brands");
            }

            // 3. Save merged record
            var success = await _repository.UpdateBrandAsync(existing);
            return success ? Ok(existing) : BadRequest();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating brand");
            return StatusCode(500, new { message = "An unexpected error occurred." });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var brand = (await _repository.GetBrandsAsync(id: id)).FirstOrDefault();
            if (brand != null && !string.IsNullOrEmpty(brand.Logo)) await _cloudinary.DeleteFileAsync(brand.Logo);
            
            var success = await _repository.DeleteBrandAsync(id);
            return success ? NoContent() : NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting brand");
            return StatusCode(500, new { message = "An unexpected error occurred." });
        }
    }
}
