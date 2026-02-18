using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductApi.Core.Entities;
using ProductApi.Core.Interfaces;
using ProductApi.Core.DTOs;
using ProductApi.Core.Helpers;
using System.Text.Json;

namespace ProductApi.Api.Controllers;

[ApiController]
[Route("api/v1/products")]
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
    public async Task<IActionResult> Create([FromForm] ProductRequest request)
    {
        try
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

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
                try
                {
                    // Attempt standard deserialization
                    product.Specifications = JsonSerializer.Deserialize<ProductSpecifications>(request.SpecificationsJson, _jsonOptions) ?? new();
                }
                catch (JsonException)
                {
                    // Fallback: Handle double-escaped JSON (common in CURL/CLI requests on Windows)
                    try
                    {
                        var unescaped = request.SpecificationsJson.Replace("\\\"", "\"").Replace("\\\\", "\\");
                        product.Specifications = JsonSerializer.Deserialize<ProductSpecifications>(unescaped, _jsonOptions) ?? new();
                    }
                    catch
                    {
                        return BadRequest(new { message = "Invalid JSON format in SpecificationsJson" });
                    }
                }
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

            // Upload Documents with custom names
            product.TechnicalDataSheets = await UploadDocsWithNames(request.TechnicalDataSheetFiles, request.TechnicalDataSheetNamesJson, "products/documents/tds");
            product.SafetyDataSheets = await UploadDocsWithNames(request.SafetyDataSheetFiles, request.SafetyDataSheetNamesJson, "products/documents/sds");
            product.Certificates = await UploadDocsWithNames(request.CertificateFiles, request.CertificateNamesJson, "products/documents/certificates");

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
    public async Task<IActionResult> Update(int id, [FromForm] ProductRequest request)
    {
        try
        {
            var existing = await _productRepository.GetProductByIdAsync(id);
            if (existing == null) return NotFound();

            existing.Name = request.Name;
            existing.Slug = SlugHelper.Generate(request.Name);
            existing.ShortDescription = request.ShortDescription;
            existing.CategoryId = request.CategoryId;
            existing.SubCategoryId = request.SubCategoryId;
            existing.BrandId = request.BrandId;
            existing.UnitId = request.UnitId;
            existing.CountryId = request.CountryId;
            existing.OverviewHtml = request.OverviewHtml;
            existing.AdvantageHtml = request.AdvantageHtml;
            existing.ApplicationRangeHtml = request.ApplicationRangeHtml;
            existing.PrecautionHtml = request.PrecautionHtml;
            existing.UpdatedAt = DateTime.UtcNow;

            if (!string.IsNullOrEmpty(request.SpecificationsJson))
            {
                try
                {
                    existing.Specifications = JsonSerializer.Deserialize<ProductSpecifications>(request.SpecificationsJson, _jsonOptions) ?? new();
                }
                catch (JsonException)
                {
                    try
                    {
                        var unescaped = request.SpecificationsJson.Replace("\\\"", "\"").Replace("\\\\", "\\");
                        existing.Specifications = JsonSerializer.Deserialize<ProductSpecifications>(unescaped, _jsonOptions) ?? new();
                    }
                    catch
                    {
                        return BadRequest(new { message = "Invalid JSON format in SpecificationsJson" });
                    }
                }
            }

            // Replace files if new ones provided
            if (request.MainImageFile != null)
            {
                if (!string.IsNullOrEmpty(existing.MainImage)) await _cloudinaryService.DeleteFileAsync(existing.MainImage);
                existing.MainImage = await _cloudinaryService.UploadImageAsync(request.MainImageFile, "products/main");
            }

            // For PUT, we usually replace collections if they are provided, or clear them.
            // But here we'll follow a "replace if provided" pattern for simplicity.
            if (request.RelatedImageFiles != null && request.RelatedImageFiles.Any())
            {
                foreach (var img in existing.RelatedImages) await _cloudinaryService.DeleteFileAsync(img);
                existing.RelatedImages.Clear();
                foreach (var file in request.RelatedImageFiles)
                {
                    existing.RelatedImages.Add(await _cloudinaryService.UploadImageAsync(file, "products/gallery"));
                }
            }

            if (request.TechnicalDataSheetFiles != null)
            {
                foreach (var doc in existing.TechnicalDataSheets) await _cloudinaryService.DeleteFileAsync(doc.Url);
                existing.TechnicalDataSheets = await UploadDocsWithNames(request.TechnicalDataSheetFiles, request.TechnicalDataSheetNamesJson, "products/documents/tds");
            }

            if (request.SafetyDataSheetFiles != null)
            {
                foreach (var doc in existing.SafetyDataSheets) await _cloudinaryService.DeleteFileAsync(doc.Url);
                existing.SafetyDataSheets = await UploadDocsWithNames(request.SafetyDataSheetFiles, request.SafetyDataSheetNamesJson, "products/documents/sds");
            }

            if (request.CertificateFiles != null)
            {
                foreach (var doc in existing.Certificates) await _cloudinaryService.DeleteFileAsync(doc.Url);
                existing.Certificates = await UploadDocsWithNames(request.CertificateFiles, request.CertificateNamesJson, "products/documents/certificates");
            }

            await _productRepository.UpdateProductAsync(existing);
            return Ok(existing);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product {Id}", id);
            return StatusCode(500, new { message = "Error updating product", details = ex.Message });
        }
    }

    [HttpPatch("{id}")]
    [Authorize(Roles = "Admin")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Patch(int id, [FromForm] ProductPatchRequest request)
    {
        try
        {
            var existing = await _productRepository.GetProductByIdAsync(id);
            if (existing == null) return NotFound();

            // Patch basic fields
            if (request.Name != null) { existing.Name = request.Name; existing.Slug = SlugHelper.Generate(request.Name); }
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

            // Patch Specifications (Merge behavior)
            if (!string.IsNullOrEmpty(request.SpecificationsJson))
            {
                ProductSpecifications? newSpecs = null;
                try
                {
                    newSpecs = JsonSerializer.Deserialize<ProductSpecifications>(request.SpecificationsJson, _jsonOptions);
                }
                catch (JsonException)
                {
                    try
                    {
                        var unescaped = request.SpecificationsJson.Replace("\\\"", "\"").Replace("\\\\", "\\");
                        newSpecs = JsonSerializer.Deserialize<ProductSpecifications>(unescaped, _jsonOptions);
                    }
                    catch
                    {
                        return BadRequest(new { message = "Invalid JSON format in SpecificationsJson" });
                    }
                }

                if (newSpecs != null)
                {
                    existing.Specifications.PackSizes = existing.Specifications.PackSizes.Union(newSpecs.PackSizes).ToList();
                    existing.Specifications.PackagingDetails = existing.Specifications.PackagingDetails.Union(newSpecs.PackagingDetails).ToList();
                    existing.Specifications.Colors = existing.Specifications.Colors.Union(newSpecs.Colors).ToList();
                    existing.Specifications.Thicknesses = existing.Specifications.Thicknesses.Union(newSpecs.Thicknesses).ToList();
                    existing.Specifications.Densities = existing.Specifications.Densities.Union(newSpecs.Densities).ToList();
                    existing.Specifications.Appearances = existing.Specifications.Appearances.Union(newSpecs.Appearances).ToList();
                    existing.Specifications.DosageCoverages = existing.Specifications.DosageCoverages.Union(newSpecs.DosageCoverages).ToList();
                    existing.Specifications.ShelfLife = existing.Specifications.ShelfLife.Union(newSpecs.ShelfLife).ToList();
                }
            }

            // Patch Files (Additive behavior)
            if (request.MainImageFile != null)
            {
                if (!string.IsNullOrEmpty(existing.MainImage)) await _cloudinaryService.DeleteFileAsync(existing.MainImage);
                existing.MainImage = await _cloudinaryService.UploadImageAsync(request.MainImageFile, "products/main");
            }

            if (request.RelatedImageFiles != null && request.RelatedImageFiles.Any())
            {
                foreach (var file in request.RelatedImageFiles)
                {
                    existing.RelatedImages.Add(await _cloudinaryService.UploadImageAsync(file, "products/gallery"));
                }
            }

            // Technical Data Sheets
            if (request.TechnicalDataSheetFiles != null && request.TechnicalDataSheetFiles.Any())
            {
                var newDocs = await UploadDocsWithNames(request.TechnicalDataSheetFiles, request.TechnicalDataSheetNamesJson, "products/documents/tds");
                existing.TechnicalDataSheets.AddRange(newDocs);
            }

            // Safety Data Sheets
            if (request.SafetyDataSheetFiles != null && request.SafetyDataSheetFiles.Any())
            {
                var newDocs = await UploadDocsWithNames(request.SafetyDataSheetFiles, request.SafetyDataSheetNamesJson, "products/documents/sds");
                existing.SafetyDataSheets.AddRange(newDocs);
            }

            // Certificates
            if (request.CertificateFiles != null && request.CertificateFiles.Any())
            {
                var newDocs = await UploadDocsWithNames(request.CertificateFiles, request.CertificateNamesJson, "products/documents/certificates");
                existing.Certificates.AddRange(newDocs);
            }

            existing.UpdatedAt = DateTime.UtcNow;
            await _productRepository.UpdateProductAsync(existing);
            return Ok(existing);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error patching product {Id}", id);
            return StatusCode(500, new { message = "Internal server error" });
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

            if (!string.IsNullOrEmpty(product.MainImage)) await _cloudinaryService.DeleteFileAsync(product.MainImage);
            foreach (var img in product.RelatedImages) await _cloudinaryService.DeleteFileAsync(img);
            foreach (var doc in product.TechnicalDataSheets) await _cloudinaryService.DeleteFileAsync(doc.Url);
            foreach (var doc in product.SafetyDataSheets) await _cloudinaryService.DeleteFileAsync(doc.Url);
            foreach (var doc in product.Certificates) await _cloudinaryService.DeleteFileAsync(doc.Url);

            await _productRepository.DeleteProductAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product");
            return StatusCode(500, new { message = ex.Message });
        }
    }

    private async Task<List<ProductDocument>> UploadDocsWithNames(List<IFormFile>? files, string? namesJson, string folder)
    {
        var docs = new List<ProductDocument>();
        if (files == null || !files.Any()) return docs;

        var nameList = new List<string>();
        if (!string.IsNullOrEmpty(namesJson))
        {
            namesJson = namesJson.Trim();
            // Handle JSON Array like ["Name 1", "Name 2"]
            if (namesJson.StartsWith("["))
            {
                try 
                { 
                    nameList = JsonSerializer.Deserialize<List<string>>(namesJson, _jsonOptions) ?? new(); 
                }
                catch (JsonException)
                {
                    // Fallback: Handle double-escaped JSON
                    try
                    {
                        var unescaped = namesJson.Replace("\\\"", "\"").Replace("\\\\", "\\");
                        nameList = JsonSerializer.Deserialize<List<string>>(unescaped, _jsonOptions) ?? new();
                    }
                    catch
                    {
                        // Final fallback: Treat as a single plain string
                        nameList.Add(namesJson);
                    }
                }
            }
            else
            {
                // Handle plain string (single name)
                nameList.Add(namesJson);
            }
        }

        for (int i = 0; i < files.Count; i++)
        {
            var file = files[i];
            
            // Logic for name override:
            // 1. If we have a specific name for this index, use it.
            // 2. If we only have ONE name provided but MULTIPLE files, 
            //    we'll append a number to the name for subsequent files (e.g. "Name Part 1", "Name Part 2")
            // 3. Fallback to original file name if no custom name provided.
            
            string customName;
            if (nameList.Count > i)
            {
                customName = nameList[i];
            }
            else if (nameList.Count == 1 && files.Count > 1)
            {
                customName = $"{nameList[0]} ({i + 1})";
            }
            else
            {
                customName = file.FileName;
            }

            var url = await _cloudinaryService.UploadFileAsync(file, folder, customName);
            docs.Add(new ProductDocument { Name = customName, Url = url });
        }
        return docs;
    }
}
