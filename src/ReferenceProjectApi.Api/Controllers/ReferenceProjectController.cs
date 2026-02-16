using Microsoft.AspNetCore.Mvc;
using ReferenceProjectApi.Core.Entities;
using ReferenceProjectApi.Core.Interfaces;
using ReferenceProjectApi.Core.DTOs;
using System.Text.Json;

namespace ReferenceProjectApi.Api.Controllers;

[ApiController]
[Route("api/v1/reference-projects")]
public class ReferenceProjectController : ControllerBase
{
    private readonly IReferenceProjectRepository _repository;
    private readonly ICloudinaryService _cloudinaryService;
    private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public ReferenceProjectController(IReferenceProjectRepository repository, ICloudinaryService cloudinaryService)
    {
        _repository = repository;
        _cloudinaryService = cloudinaryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetProjects([FromQuery] int page = 1, [FromQuery] int limit = 10, [FromQuery] string? status = null, [FromQuery] bool? featured = null, [FromQuery] string? search = null, [FromQuery] int? categoryId = null)
    {
        var projects = await _repository.GetProjectsAsync(page, limit, status, featured, search, categoryId);
        var total = await _repository.GetTotalCountAsync(status, featured, search, categoryId);

        var response = projects.Select(p => MapToResponse(p));

        return Ok(new { TotalCount = total, Page = page, Limit = limit, Data = response });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var project = await _repository.GetByIdAsync(id);
        if (project == null) return NotFound();
        return Ok(MapToResponse(project));
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Create([FromForm] ReferenceProjectRequest request)
    {
        // 1. Upload Images
        string? heroImageUrl = null;
        if (request.HeroImage != null)
            heroImageUrl = await _cloudinaryService.UploadImageAsync(request.HeroImage, "projects/hero");

        var galleryImageUrls = new List<string>();
        if (request.GalleryImages != null)
        {
            foreach (var img in request.GalleryImages)
                galleryImageUrls.Add(await _cloudinaryService.UploadImageAsync(img, "projects/gallery"));
        }

        var detailImageUrls = new List<string>();
        if (request.DetailImages != null)
        {
            foreach (var img in request.DetailImages)
                detailImageUrls.Add(await _cloudinaryService.UploadImageAsync(img, "projects/details"));
        }

        // 2. Parse JSON fields
        var productIds = new List<int>();
        if (!string.IsNullOrEmpty(request.ProductIdsJson))
        {
            productIds = JsonSerializer.Deserialize<List<int>>(request.ProductIdsJson, _jsonOptions) ?? new();
        }

        // 3. Create Entity
        var project = new ReferenceProject
        {
            ProjectName = request.ProjectName,
            CategoryId = request.CategoryId,
            Location = request.Location,
            OwnerName = request.OwnerName,
            Contractor = request.Contractor,
            EngineerName = request.EngineerName,
            ClientName = request.ClientName,
            ShortDescription = request.ShortDescription,
            DetailsDescription = request.DetailsDescription,
            Status = request.Status,
            StartDate = request.StartDate,
            CompletionDate = request.CompletionDate,
            Featured = request.Featured,
            HeroImageUrl = heroImageUrl,
            ProjectOverviewJson = request.ProjectOverviewJson,
            Slug = request.ProjectName.ToLower().Replace(" ", "-") + "-" + DateTime.UtcNow.Ticks,
            CreatedAt = DateTime.UtcNow
        };

        // Add Gallery Images
        foreach (var url in galleryImageUrls)
            project.GalleryImages.Add(new ProjectGalleryImage { ImageUrl = url });

        // Add Detail Images
        foreach (var url in detailImageUrls)
            project.DetailImages.Add(new ProjectDetailImage { ImageUrl = url });

        // Add Product Links (Junction)
        foreach (var pid in productIds.Distinct())
            project.ProjectProducts.Add(new ProjectProductJunction { ProductId = pid });

        var id = await _repository.CreateAsync(project);
        return CreatedAtAction(nameof(GetById), new { id }, MapToResponse(project));
    }

    [HttpPut("{id}")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Update(int id, [FromForm] ReferenceProjectRequest request)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing == null) return NotFound();

        // 1. Update Images
        if (request.HeroImage != null)
        {
            if (!string.IsNullOrEmpty(existing.HeroImageUrl))
                await _cloudinaryService.DeleteFileAsync(existing.HeroImageUrl);
            existing.HeroImageUrl = await _cloudinaryService.UploadImageAsync(request.HeroImage, "projects/hero");
        }

        if (request.GalleryImages != null && request.GalleryImages.Any())
        {
            foreach (var img in existing.GalleryImages)
                await _cloudinaryService.DeleteFileAsync(img.ImageUrl);
            
            existing.GalleryImages.Clear();
            foreach (var img in request.GalleryImages)
            {
                var url = await _cloudinaryService.UploadImageAsync(img, "projects/gallery");
                existing.GalleryImages.Add(new ProjectGalleryImage { ImageUrl = url, ProjectId = id });
            }
        }

        if (request.DetailImages != null && request.DetailImages.Any())
        {
            foreach (var img in existing.DetailImages)
                await _cloudinaryService.DeleteFileAsync(img.ImageUrl);
            
            existing.DetailImages.Clear();
            foreach (var img in request.DetailImages)
            {
                var url = await _cloudinaryService.UploadImageAsync(img, "projects/details");
                existing.DetailImages.Add(new ProjectDetailImage { ImageUrl = url, ProjectId = id });
            }
        }

        // 2. Parse JSON fields
        if (!string.IsNullOrEmpty(request.ProductIdsJson))
        {
            existing.ProjectProducts.Clear();
            var productIds = JsonSerializer.Deserialize<List<int>>(request.ProductIdsJson, _jsonOptions) ?? new();
            foreach (var pid in productIds.Distinct())
            {
                existing.ProjectProducts.Add(new ProjectProductJunction { ProjectId = id, ProductId = pid });
            }
        }

        // 3. Update Basic Fields
        existing.ProjectName = request.ProjectName;
        existing.CategoryId = request.CategoryId;
        existing.Location = request.Location;
        existing.OwnerName = request.OwnerName;
        existing.Contractor = request.Contractor;
        existing.EngineerName = request.EngineerName;
        existing.ClientName = request.ClientName;
        existing.ShortDescription = request.ShortDescription;
        existing.DetailsDescription = request.DetailsDescription;
        existing.Status = request.Status;
        existing.StartDate = request.StartDate;
        existing.CompletionDate = request.CompletionDate;
        existing.Featured = request.Featured;
        existing.ProjectOverviewJson = request.ProjectOverviewJson;
        existing.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(existing);
        return Ok(MapToResponse(existing));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var project = await _repository.GetByIdAsync(id);
        if (project == null) return NotFound();

        // Optional: Delete images from Cloudinary
        if (!string.IsNullOrEmpty(project.HeroImageUrl))
            await _cloudinaryService.DeleteFileAsync(project.HeroImageUrl);
        
        foreach (var img in project.GalleryImages)
            await _cloudinaryService.DeleteFileAsync(img.ImageUrl);
            
        foreach (var img in project.DetailImages)
            await _cloudinaryService.DeleteFileAsync(img.ImageUrl);

        await _repository.DeleteAsync(id);
        return NoContent();
    }

    private ReferenceProjectResponse MapToResponse(ReferenceProject p)
    {
        return new ReferenceProjectResponse
        {
            Id = p.Id,
            ProjectName = p.ProjectName,
            Slug = p.Slug,
            CategoryId = p.CategoryId,
            CategoryName = p.Category?.Name,
            Location = p.Location,
            OwnerName = p.OwnerName,
            Contractor = p.Contractor,
            EngineerName = p.EngineerName,
            ClientName = p.ClientName,
            ShortDescription = p.ShortDescription,
            DetailsDescription = p.DetailsDescription,
            HeroImageUrl = p.HeroImageUrl,
            GalleryImages = p.GalleryImages.Select(gi => gi.ImageUrl).ToList(),
            DetailImages = p.DetailImages.Select(di => di.ImageUrl).ToList(),
            ProductsUsed = p.ProjectProducts.Select(pp => new ProductSimpleDto 
            { 
                Id = pp.ProductId, 
                Name = pp.Product?.Name ?? "Unknown Product" 
            }).ToList(),
            ProjectOverview = string.IsNullOrEmpty(p.ProjectOverviewJson) 
                ? null 
                : JsonSerializer.Deserialize<dynamic>(p.ProjectOverviewJson, _jsonOptions),
            Status = p.Status,
            StartDate = p.StartDate,
            CompletionDate = p.CompletionDate,
            Featured = p.Featured,
            CreatedAt = p.CreatedAt
        };
    }
}
