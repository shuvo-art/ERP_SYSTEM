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
    private readonly ILogger<SubCategoriesController> _logger;

    public SubCategoriesController(IMasterDataRepository repository, ILogger<SubCategoriesController> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] int? id, [FromQuery] string? slug)
    {
        try
        {
            var results = await _repository.GetSubCategoriesAsync(search, id, slug);
            return Ok(results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching subcategories");
            return StatusCode(500, new { message = ex.Message });
        }
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] SubCategoryRequest request)
    {
        try
        {
            var subCategory = new SubCategoryMaster 
            { 
                CategoryIds = request.CategoryIds, 
                Name = request.Name,
                Slug = SlugHelper.Generate(request.Name)
            };
            var id = await _repository.CreateSubCategoryAsync(subCategory);
            subCategory.Id = id;
            return CreatedAtAction(nameof(GetAll), new { id = subCategory.Id }, subCategory);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating subcategory");
            return StatusCode(500, new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] SubCategoryRequest request)
    {
        try
        {
            var existingList = (await _repository.GetSubCategoriesAsync(id: id)).ToList();
            var existing = existingList.FirstOrDefault();
            if (existing == null) return NotFound();

            existing.Name = request.Name;
            existing.CategoryIds = request.CategoryIds;
            existing.Slug = SlugHelper.Generate(request.Name);

            var success = await _repository.UpdateSubCategoryAsync(existing);
            return success ? Ok(existing) : BadRequest();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating subcategory");
            return StatusCode(500, new { message = ex.Message });
        }
    }

    [HttpPatch("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Patch(int id, [FromBody] SubCategoryPatchRequest request)
    {
        try
        {
            var existingList = (await _repository.GetSubCategoriesAsync(id: id)).ToList();
            var existing = existingList.FirstOrDefault();
            if (existing == null) return NotFound();

            bool isUpdated = false;

            if (!string.IsNullOrEmpty(request.Name))
            {
                existing.Name = request.Name;
                existing.Slug = SlugHelper.Generate(request.Name);
                isUpdated = true;
            }

            if (request.CategoryIds != null && request.CategoryIds.Any())
            {
                // Additive behavior: Merge existing IDs with new IDs
                existing.CategoryIds = existing.CategoryIds
                    .Union(request.CategoryIds)
                    .Distinct()
                    .ToList();
                isUpdated = true;
            }

            if (!isUpdated) return BadRequest("No updates provided.");

            var success = await _repository.UpdateSubCategoryAsync(existing);
            return success ? Ok(existing) : BadRequest();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error patching subcategory");
            return StatusCode(500, new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var success = await _repository.DeleteSubCategoryAsync(id);
            return success ? NoContent() : NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting subcategory");
            return StatusCode(500, new { message = ex.Message });
        }
    }
}
