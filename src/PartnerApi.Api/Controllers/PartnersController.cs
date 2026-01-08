using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.RegularExpressions;
using PartnerApi.Api.DTOs;
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
                LongDescriptionTitle = request.LongDescriptionTitle,
                LongDescription = request.LongDescription
            };

            if (!string.IsNullOrEmpty(request.CompanyProfileJson))
                partner.CompanyProfile = JsonSerializer.Deserialize<Dictionary<string, string>>(request.CompanyProfileJson, _jsonOptions) ?? new();
            
            if (!string.IsNullOrEmpty(request.ProductSegmentsJson))
                partner.ProductSegments = JsonSerializer.Deserialize<List<ProductSegment>>(request.ProductSegmentsJson, _jsonOptions) ?? new();

            // Handle Files
            if (request.LogoFile != null) partner.LogoUrl = await _cloudinary.UploadImageAsync(request.LogoFile, "partners/logos");
            if (request.BuildingImageFile != null) partner.BuildingImageUrl = await _cloudinary.UploadImageAsync(request.BuildingImageFile, "partners/buildings");
            if (request.BrochureFile != null) partner.CompanyProfile["brochure_url"] = await _cloudinary.UploadFileAsync(request.BrochureFile, "partners/brochures");

            // Handle Product Segment Images (if provided separately)
            if (request.ProductSegmentFiles != null && request.ProductSegmentFiles.Any())
            {
                for (int i = 0; i < Math.Min(request.ProductSegmentFiles.Count, partner.ProductSegments.Count); i++)
                {
                    partner.ProductSegments[i].ImageUrl = await _cloudinary.UploadImageAsync(request.ProductSegmentFiles[i], "partners/segments");
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
        existing.LongDescriptionTitle = request.LongDescriptionTitle;
        existing.LongDescription = request.LongDescription;

        if (!string.IsNullOrEmpty(request.CompanyProfileJson))
            existing.CompanyProfile = JsonSerializer.Deserialize<Dictionary<string, string>>(request.CompanyProfileJson, _jsonOptions) ?? new();
            
        if (!string.IsNullOrEmpty(request.ProductSegmentsJson))
            existing.ProductSegments = JsonSerializer.Deserialize<List<ProductSegment>>(request.ProductSegmentsJson, _jsonOptions) ?? new();

        if (request.LogoFile != null) 
        {
            await _cloudinary.DeleteFileAsync(existing.LogoUrl!);
            existing.LogoUrl = await _cloudinary.UploadImageAsync(request.LogoFile, "partners/logos");
        }
        if (request.BuildingImageFile != null) 
        {
            await _cloudinary.DeleteFileAsync(existing.BuildingImageUrl!);
            existing.BuildingImageUrl = await _cloudinary.UploadImageAsync(request.BuildingImageFile, "partners/buildings");
        }
        if (request.BrochureFile != null)
        {
            if (existing.CompanyProfile.TryGetValue("brochure_url", out var oldUrl)) await _cloudinary.DeleteFileAsync(oldUrl);
            existing.CompanyProfile["brochure_url"] = await _cloudinary.UploadFileAsync(request.BrochureFile, "partners/brochures");
        }

        await _repository.UpdateAsync(existing);
        return Ok(existing);
    }

    [HttpPatch("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Patch(int id, [FromForm] PartnerPatchRequest request)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing == null) return NotFound();

        if (request.Name != null) { existing.Name = request.Name; existing.Slug = GenerateSlug(request.Name); }
        if (request.ShortDescription != null) existing.ShortDescription = request.ShortDescription;
        if (request.LongDescriptionTitle != null) existing.LongDescriptionTitle = request.LongDescriptionTitle;
        if (request.LongDescription != null) existing.LongDescription = request.LongDescription;

        if (!string.IsNullOrEmpty(request.CompanyProfileJson))
            existing.CompanyProfile = JsonSerializer.Deserialize<Dictionary<string, string>>(request.CompanyProfileJson, _jsonOptions) ?? existing.CompanyProfile;
            
        if (!string.IsNullOrEmpty(request.ProductSegmentsJson))
            existing.ProductSegments = JsonSerializer.Deserialize<List<ProductSegment>>(request.ProductSegmentsJson, _jsonOptions) ?? existing.ProductSegments;

        if (request.LogoFile != null) 
        {
            await _cloudinary.DeleteFileAsync(existing.LogoUrl!);
            existing.LogoUrl = await _cloudinary.UploadImageAsync(request.LogoFile, "partners/logos");
        }
        if (request.BuildingImageFile != null) 
        {
            await _cloudinary.DeleteFileAsync(existing.BuildingImageUrl!);
            existing.BuildingImageUrl = await _cloudinary.UploadImageAsync(request.BuildingImageFile, "partners/buildings");
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

        await _cloudinary.DeleteFileAsync(existing.LogoUrl!);
        await _cloudinary.DeleteFileAsync(existing.BuildingImageUrl!);
        if (existing.CompanyProfile.TryGetValue("brochure_url", out var brochureUrl)) await _cloudinary.DeleteFileAsync(brochureUrl);
        foreach (var seg in existing.ProductSegments) await _cloudinary.DeleteFileAsync(seg.ImageUrl!);

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
