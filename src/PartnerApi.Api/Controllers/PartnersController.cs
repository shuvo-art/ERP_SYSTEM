using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.RegularExpressions;
using PartnerApi.Core.DTOs;
using PartnerApi.Core.Entities;
using PartnerApi.Core.Interfaces;

namespace PartnerApi.Api.Controllers;

[ApiController]
[Route("api/v1/partners")]
public class PartnersController : ControllerBase
{
    private readonly IPartnerRepository _repository;
    private readonly ICloudinaryService _cloudinary;
    private readonly ILogger<PartnersController> _logger;
    private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public PartnersController(IPartnerRepository repository, ICloudinaryService cloudinary, ILogger<PartnersController> logger)
    {
        _repository = repository;
        _cloudinary = cloudinary;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int limit = 10)
    {
        var (data, total) = await _repository.GetAllAsync(search, page, limit);
        return Ok(new { data, total, page, limit });
    }

    [HttpGet("{idOrSlug}")]
    public async Task<IActionResult> Get(string idOrSlug)
    {
        Partner? partner;
        if (int.TryParse(idOrSlug, out int id))
            partner = await _repository.GetByIdAsync(id);
        else
            partner = await _repository.GetBySlugAsync(idOrSlug.ToLower());

        if (partner == null) return NotFound();
        return Ok(partner);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromForm] PartnerRequest request)
    {
        try
        {
            var partner = new Partner
            {
                Name = request.Name,
                Slug = GenerateSlug(request.Name),
                ShortDescription = request.ShortDescription,
                DetailsDescriptionTitle = request.DetailsDescriptionTitle,
                DetailsDescription = request.DetailsDescription,
                CompanyName = request.CompanyName,
                BrandName = request.BrandName,
                EstablishedIn = request.EstablishedIn,
                Website = request.Website
            };

            if (!string.IsNullOrEmpty(request.ProductSegmentsJson))
                partner.ProductSegments = JsonSerializer.Deserialize<List<ProductSegment>>(request.ProductSegmentsJson, _jsonOptions) ?? new();
            
            if (!string.IsNullOrEmpty(request.DocumentsJson))
                partner.Documents = JsonSerializer.Deserialize<List<PartnerDocument>>(request.DocumentsJson, _jsonOptions) ?? new();

            // Handle Files
            if (request.LogoFile != null) 
                partner.LogoUrl = await _cloudinary.UploadImageAsync(request.LogoFile, "partners/logos");
            
            if (request.BuildingImageFile != null) 
                partner.BuildingImageUrl = await _cloudinary.UploadImageAsync(request.BuildingImageFile, "partners/buildings");
            
            if (request.VideoFile != null)
                partner.VideoUrl = await _cloudinary.UploadFileAsync(request.VideoFile, "partners/videos");

            // Handle Product Segment Images
            if (request.ProductSegmentFiles != null && request.ProductSegmentFiles.Any())
            {
                for (int i = 0; i < Math.Min(request.ProductSegmentFiles.Count, partner.ProductSegments.Count); i++)
                {
                    partner.ProductSegments[i].ImageUrl = await _cloudinary.UploadImageAsync(request.ProductSegmentFiles[i], "partners/segments");
                }
            }

            // Handle Document Files
            if (request.DocumentFiles != null && request.DocumentFiles.Any())
            {
                for (int i = 0; i < Math.Min(request.DocumentFiles.Count, partner.Documents.Count); i++)
                {
                    partner.Documents[i].DocumentUrl = await _cloudinary.UploadFileAsync(request.DocumentFiles[i], "partners/documents");
                }
            }

            var id = await _repository.CreateAsync(partner);
            partner.Id = id;
            return CreatedAtAction(nameof(Get), new { idOrSlug = partner.Id }, partner);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating partner");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromForm] PartnerRequest request)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing == null) return NotFound();

        existing.Name = request.Name;
        existing.Slug = GenerateSlug(request.Name);
        existing.ShortDescription = request.ShortDescription;
        existing.DetailsDescriptionTitle = request.DetailsDescriptionTitle;
        existing.DetailsDescription = request.DetailsDescription;
        existing.CompanyName = request.CompanyName;
        existing.BrandName = request.BrandName;
        existing.EstablishedIn = request.EstablishedIn;
        existing.Website = request.Website;

        if (!string.IsNullOrEmpty(request.ProductSegmentsJson))
            existing.ProductSegments = JsonSerializer.Deserialize<List<ProductSegment>>(request.ProductSegmentsJson, _jsonOptions) ?? new();
            
        if (!string.IsNullOrEmpty(request.DocumentsJson))
            existing.Documents = JsonSerializer.Deserialize<List<PartnerDocument>>(request.DocumentsJson, _jsonOptions) ?? new();

        // Logo
        if (request.LogoFile != null) 
        {
            if (!string.IsNullOrEmpty(existing.LogoUrl)) await _cloudinary.DeleteFileAsync(existing.LogoUrl);
            existing.LogoUrl = await _cloudinary.UploadImageAsync(request.LogoFile, "partners/logos");
        }

        // Building Image
        if (request.BuildingImageFile != null) 
        {
            if (!string.IsNullOrEmpty(existing.BuildingImageUrl)) await _cloudinary.DeleteFileAsync(existing.BuildingImageUrl);
            existing.BuildingImageUrl = await _cloudinary.UploadImageAsync(request.BuildingImageFile, "partners/buildings");
        }

        // Video
        if (request.VideoFile != null)
        {
            if (!string.IsNullOrEmpty(existing.VideoUrl)) await _cloudinary.DeleteFileAsync(existing.VideoUrl);
            existing.VideoUrl = await _cloudinary.UploadFileAsync(request.VideoFile, "partners/videos");
        }

        // Handle Product Segment Images (Simplified: assumes matching order if provided)
        if (request.ProductSegmentFiles != null && request.ProductSegmentFiles.Any())
        {
            for (int i = 0; i < Math.Min(request.ProductSegmentFiles.Count, existing.ProductSegments.Count); i++)
            {
                existing.ProductSegments[i].ImageUrl = await _cloudinary.UploadImageAsync(request.ProductSegmentFiles[i], "partners/segments");
            }
        }

        // Handle Document Files
        if (request.DocumentFiles != null && request.DocumentFiles.Any())
        {
            for (int i = 0; i < Math.Min(request.DocumentFiles.Count, existing.Documents.Count); i++)
            {
                existing.Documents[i].DocumentUrl = await _cloudinary.UploadFileAsync(request.DocumentFiles[i], "partners/documents");
            }
        }

        await _repository.UpdateAsync(existing);
        return Ok(existing);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing == null) return NotFound();

        if (!string.IsNullOrEmpty(existing.LogoUrl)) await _cloudinary.DeleteFileAsync(existing.LogoUrl);
        if (!string.IsNullOrEmpty(existing.BuildingImageUrl)) await _cloudinary.DeleteFileAsync(existing.BuildingImageUrl);
        if (!string.IsNullOrEmpty(existing.VideoUrl)) await _cloudinary.DeleteFileAsync(existing.VideoUrl);
        
        foreach (var seg in existing.ProductSegments) 
        {
            if (!string.IsNullOrEmpty(seg.ImageUrl)) await _cloudinary.DeleteFileAsync(seg.ImageUrl);
        }

        foreach (var doc in existing.Documents)
        {
            if (!string.IsNullOrEmpty(doc.DocumentUrl)) await _cloudinary.DeleteFileAsync(doc.DocumentUrl);
        }

        await _repository.DeleteAsync(id);
        return NoContent();
    }

    private string GenerateSlug(string phrase)
    {
        string str = phrase.ToLower();
        str = Regex.Replace(str, @"[^a-z0-9\s-]", "");
        str = Regex.Replace(str, @"\s+", " ").Trim();
        str = str.Substring(0, str.Length <= 45 ? str.Length : 45).Trim();
        str = Regex.Replace(str, @"\s", "-");
        return str;
    }
}
