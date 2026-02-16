using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductApi.Core.Entities;
using ProductApi.Core.Interfaces;
using ProductApi.Core.DTOs;
using ProductApi.Core.Helpers;
using System.Text.Json;

namespace ProductApi.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductRepository _productRepository;
    private readonly ICloudinaryService _cloudinaryService;
    private readonly ILogger<ProductsController> _logger;
    private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public ProductsController(
        IProductRepository productRepository, 
        ICloudinaryService cloudinaryService,
        ILogger<ProductsController> logger)
    {
        _productRepository = productRepository;
        _cloudinaryService = cloudinaryService;
        _logger = logger;
    }

    /// <summary>
    /// Get all products with filtering and pagination
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int? categoryId, 
        [FromQuery] int? brandId, 
        [FromQuery] string? search,
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var (products, total) = await _productRepository.GetAllProductsAsync(categoryId, brandId, search, page, pageSize);
            return Ok(new { data = products, total, page, pageSize });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all products");
            return StatusCode(500, new { message = "Error retrieving products" });
        }
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var product = await _productRepository.GetProductByIdAsync(id);
        if (product == null) return NotFound();
        return Ok(product);
    }

    /// <summary>
    /// Get product by Slug (Industry standard for SEO URLs)
    /// </summary>
    [HttpGet("slug/{slug}")]
    public async Task<IActionResult> GetBySlug(string slug)
    {
        var product = await _productRepository.GetProductBySlugAsync(slug);
        if (product == null) return NotFound();
        return Ok(product);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Create(
        [FromForm] ProductRequest request,
        [FromForm] List<IFormFile>? TechnicalDataSheetFiles,
        [FromForm] List<IFormFile>? SafetyDataSheetFiles,
        [FromForm] List<IFormFile>? CertificateFiles)
    {
        try
        {
            var product = new Product
            {
                Name = request.Name,
                Slug = SlugHelper.Generate(request.Name),
                ShortDescription = request.ShortDescription,
                CategoryId = request.CategoryId,
                SubCategoryId = request.SubCategoryId,
                BrandId = request.BrandId,
                UnitId = request.UnitId,
                CountryId = request.CountryId,
                OverviewHtml = request.OverviewHtml,
                AdvantageHtml = request.AdvantageHtml,
                ApplicationRangeHtml = request.ApplicationRangeHtml,
                PrecautionHtml = request.PrecautionHtml
            };

            // Parse Specifications
            if (!string.IsNullOrEmpty(request.SpecificationsJson))
            {
                product.Specifications = JsonSerializer.Deserialize<ProductSpecifications>(request.SpecificationsJson, _jsonOptions) ?? new();
            }

            // Upload Main Image
            if (request.MainImageFile != null)
            {
                product.MainImage = await _cloudinaryService.UploadImageAsync(request.MainImageFile, "products/main");
            }

            // Upload Related Images
            if (request.RelatedImageFiles != null)
            {
                foreach (var file in request.RelatedImageFiles)
                {
                    product.RelatedImages.Add(await _cloudinaryService.UploadImageAsync(file, "products/gallery"));
                }
            }

            // Upload Documents (TDS, SDS, Certificates)
            product.TechnicalDataSheets = await UploadDocs(TechnicalDataSheetFiles, "products/documents/tds");
            product.SafetyDataSheets = await UploadDocs(SafetyDataSheetFiles, "products/documents/sds");
            product.Certificates = await UploadDocs(CertificateFiles, "products/documents/certificates");

            var id = await _productRepository.CreateProductAsync(product);
            product.Id = id;

            return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product");
            return StatusCode(500, new { message = "Error creating product", details = ex.Message });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Update(
        int id, 
        [FromForm] ProductRequest request,
        [FromForm] List<IFormFile>? TechnicalDataSheetFiles,
        [FromForm] List<IFormFile>? SafetyDataSheetFiles,
        [FromForm] List<IFormFile>? CertificateFiles)
    {
        try
        {
            // 1. Fetch existing product
            var existing = await _productRepository.GetProductByIdAsync(id);
            if (existing == null) return NotFound();

            // 2. Merge Basic Info (Only if provided or the field is required)
            // Name is [Required] in DTO, so we always update it and slug
            existing.Name = request.Name;
            existing.Slug = SlugHelper.Generate(request.Name);

            // Conditional updates for nullable fields to preserve existing data
            if (request.ShortDescription != null) existing.ShortDescription = request.ShortDescription;
            if (request.CategoryId != null) existing.CategoryId = request.CategoryId;
            if (request.SubCategoryId != null) existing.SubCategoryId = request.SubCategoryId;
            if (request.BrandId != null) existing.BrandId = request.BrandId;
            if (request.UnitId != null) existing.UnitId = request.UnitId;
            if (request.CountryId != null) existing.CountryId = request.CountryId;
            
            if (request.OverviewHtml != null) existing.OverviewHtml = request.OverviewHtml;
            if (request.AdvantageHtml != null) existing.AdvantageHtml = request.AdvantageHtml;
            if (request.ApplicationRangeHtml != null) existing.ApplicationRangeHtml = request.ApplicationRangeHtml;
            if (request.PrecautionHtml != null) existing.PrecautionHtml = request.PrecautionHtml;

            if (!string.IsNullOrEmpty(request.SpecificationsJson))
            {
                existing.Specifications = JsonSerializer.Deserialize<ProductSpecifications>(request.SpecificationsJson, _jsonOptions) ?? new();
            }

            // 3. Update Files (Preserve if null)
            if (request.MainImageFile != null)
            {
                if (!string.IsNullOrEmpty(existing.MainImage)) await _cloudinaryService.DeleteFileAsync(existing.MainImage);
                existing.MainImage = await _cloudinaryService.UploadImageAsync(request.MainImageFile, "products/main");
            }

            // Append new related images/docs if provided
            if (request.RelatedImageFiles != null && request.RelatedImageFiles.Any())
            {
                foreach (var file in request.RelatedImageFiles)
                {
                    existing.RelatedImages.Add(await _cloudinaryService.UploadImageAsync(file, "products/gallery"));
                }
            }

            if (TechnicalDataSheetFiles != null && TechnicalDataSheetFiles.Any())
                existing.TechnicalDataSheets.AddRange(await UploadDocs(TechnicalDataSheetFiles, "products/documents/tds"));
            
            if (SafetyDataSheetFiles != null && SafetyDataSheetFiles.Any())
                existing.SafetyDataSheets.AddRange(await UploadDocs(SafetyDataSheetFiles, "products/documents/sds"));

            if (CertificateFiles != null && CertificateFiles.Any())
                existing.Certificates.AddRange(await UploadDocs(CertificateFiles, "products/documents/certificates"));

            // 4. Save merged product
            var success = await _productRepository.UpdateProductAsync(existing);
            return success ? Ok(existing) : BadRequest();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product {Id}", id);
            return StatusCode(500, new { message = "Error updating product", details = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var product = await _productRepository.GetProductByIdAsync(id);
            if (product == null) return NotFound();

            // Cleanup files from Cloudinary
            if (!string.IsNullOrEmpty(product.MainImage)) await _cloudinaryService.DeleteFileAsync(product.MainImage);
            foreach (var img in product.RelatedImages) await _cloudinaryService.DeleteFileAsync(img);
            foreach (var doc in product.TechnicalDataSheets) await _cloudinaryService.DeleteFileAsync(doc.Url);
            foreach (var doc in product.SafetyDataSheets) await _cloudinaryService.DeleteFileAsync(doc.Url);
            foreach (var doc in product.Certificates) await _cloudinaryService.DeleteFileAsync(doc.Url);

            var success = await _productRepository.DeleteProductAsync(id);
            return success ? NoContent() : NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product");
            return StatusCode(500, new { message = ex.Message });
        }
    }

    private async Task<List<ProductDocument>> UploadDocs(List<IFormFile>? files, string folder)
    {
        var docs = new List<ProductDocument>();
        if (files == null) return docs;

        foreach (var file in files)
        {
            var url = await _cloudinaryService.UploadFileAsync(file, folder);
            docs.Add(new ProductDocument { Name = file.FileName, Url = url });
        }
        return docs;
    }
}
