using Microsoft.AspNetCore.Mvc;
using ReferenceProjectApi.Core.Entities;
using ReferenceProjectApi.Core.Interfaces;
using ReferenceProjectApi.Core.DTOs;
using System.Text.Json;

namespace ReferenceProjectApi.Api.Controllers;

[ApiController]
[Route("api/v1/reference-projects")]
public class ReferenceProjectsController : ControllerBase
{
    private readonly IReferenceProjectRepository _repository;
    private readonly ICloudinaryService _cloudinary;
    private readonly ILogger<ReferenceProjectsController> _logger;
    private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public ReferenceProjectsController(
        IReferenceProjectRepository repository, 
        ICloudinaryService cloudinary,
        ILogger<ReferenceProjectsController> logger)
    {
        _repository = repository;
        _cloudinary = cloudinary;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int limit = 10, [FromQuery] string? status = null, [FromQuery] bool? featured = null, [FromQuery] string? search = null)
    {
        try
        {
            var projects = await _repository.GetProjectsAsync(page, limit, status, featured, search);
            var total = await _repository.GetTotalCountAsync(status, featured, search);

            return Ok(new 
            {
                data = projects,
                total,
                page,
                limit
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting reference projects");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpGet("{idOrSlug}")]
    public async Task<IActionResult> Get(string idOrSlug)
    {
        try
        {
            ReferenceProject? project;
            if (int.TryParse(idOrSlug, out var id))
            {
                project = await _repository.GetByIdAsync(id);
            }
            else
            {
                project = await _repository.GetBySlugAsync(idOrSlug);
            }

            if (project == null) return NotFound();
            return Ok(project);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting reference project {IdOrSlug}", idOrSlug);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromForm] ReferenceProjectRequest request)
    {
        try
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var slug = request.ProjectName.ToLower()
                .Replace(" ", "-")
                .Replace(",", "")
                .Replace(".", "")
                .Trim();
            
            var project = new ReferenceProject
            {
                ProjectName = request.ProjectName,
                Slug = slug,
                ShortDescription = request.ShortDescription,
                Location = request.Location,
                Status = request.Status,
                StartDate = request.StartDate,
                CompletionDate = request.CompletionDate,
                Featured = request.Featured,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Parse JSON components
            if (!string.IsNullOrEmpty(request.ProjectOverviewJson))
            {
                project.ProjectOverview = JsonSerializer.Deserialize<ProjectOverview>(request.ProjectOverviewJson, _jsonOptions);
            }

            if (!string.IsNullOrEmpty(request.ProductsUsedJson))
            {
                project.ProductsUsed = JsonSerializer.Deserialize<List<ProjectProduct>>(request.ProductsUsedJson, _jsonOptions) ?? new();
            }

            // Handle Images
            if (request.HeroImage != null)
            {
                project.HeroImageUrl = await _cloudinary.UploadImageAsync(request.HeroImage, "reference-projects/hero");
            }

            if (request.GalleryImages != null && request.GalleryImages.Any())
            {
                foreach (var file in request.GalleryImages)
                {
                    var url = await _cloudinary.UploadImageAsync(file, "reference-projects/gallery");
                    if (!string.IsNullOrEmpty(url)) project.GalleryImages.Add(url);
                }
            }

            var id = await _repository.CreateAsync(project);
            project.Id = id;

            return CreatedAtAction(nameof(Get), new { idOrSlug = id }, project);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating reference project");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromForm] ReferenceProjectRequest request)
    {
        try
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null) return NotFound();

            existing.ProjectName = request.ProjectName;
            existing.ShortDescription = request.ShortDescription;
            existing.Location = request.Location;
            existing.Status = request.Status;
            existing.StartDate = request.StartDate;
            existing.CompletionDate = request.CompletionDate;
            existing.Featured = request.Featured;
            existing.UpdatedAt = DateTime.UtcNow;

            // Re-generate slug if name changed
            existing.Slug = request.ProjectName.ToLower().Replace(" ", "-").Replace(",", "").Replace(".", "").Trim();

            if (!string.IsNullOrEmpty(request.ProjectOverviewJson))
            {
                existing.ProjectOverview = JsonSerializer.Deserialize<ProjectOverview>(request.ProjectOverviewJson, _jsonOptions);
            }

            if (!string.IsNullOrEmpty(request.ProductsUsedJson))
            {
                existing.ProductsUsed = JsonSerializer.Deserialize<List<ProjectProduct>>(request.ProductsUsedJson, _jsonOptions) ?? new();
            }

            if (request.HeroImage != null)
            {
                if (!string.IsNullOrEmpty(existing.HeroImageUrl)) await _cloudinary.DeleteFileAsync(existing.HeroImageUrl);
                existing.HeroImageUrl = await _cloudinary.UploadImageAsync(request.HeroImage, "reference-projects/hero");
            }

            if (request.GalleryImages != null && request.GalleryImages.Any())
            {
                // Simple replacement logic for gallery
                foreach (var img in existing.GalleryImages) await _cloudinary.DeleteFileAsync(img);
                existing.GalleryImages.Clear();
                foreach (var file in request.GalleryImages)
                {
                    var url = await _cloudinary.UploadImageAsync(file, "reference-projects/gallery");
                    if (!string.IsNullOrEmpty(url)) existing.GalleryImages.Add(url);
                }
            }

            await _repository.UpdateAsync(existing);
            return Ok(existing);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating reference project {Id}", id);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null) return NotFound();
            
            // Delete images from cloud
            if (!string.IsNullOrEmpty(existing.HeroImageUrl)) await _cloudinary.DeleteFileAsync(existing.HeroImageUrl);
            foreach (var img in existing.GalleryImages) await _cloudinary.DeleteFileAsync(img);
            
            await _repository.DeleteAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting reference project {Id}", id);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }
}

