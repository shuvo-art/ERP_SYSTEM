using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductApi.Core.Entities;
using ProductApi.Core.Interfaces;
using ProductApi.Api.DTOs;
using System.Text.Json;

namespace ProductApi.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductRepository _productRepository;
    private readonly IFileService _fileService;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(
        IProductRepository productRepository, 
        IFileService fileService,
        ILogger<ProductsController> logger)
    {
        _productRepository = productRepository;
        _fileService = fileService;
        _logger = logger;
    }

    /// <summary>
    /// Get all products
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var products = await _productRepository.GetAllProductsAsync();
            return Ok(products);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all products");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get product by id
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var product = await _productRepository.GetProductByIdAsync(id);
            if (product == null) return NotFound();
            return Ok(product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting product {Id}", id);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Create a new product (Admin only) - Supports multipart/form-data
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromForm] ProductRequest request)
    {
        try
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var product = new Product
            {
                Name = request.Name,
                Description = request.Description,
                Category = request.Category,
                SubCategory = request.SubCategory,
                Brand = request.Brand,
                ApplicationRange = request.ApplicationRange,
                Overview = new Overview { Details = request.OverviewDetails },
                Advantages = request.Advantages,
                Precautions = request.Precautions
            };

            // Handle Overview
            if (!string.IsNullOrEmpty(request.OverviewJson))
            {
                product.Overview = JsonSerializer.Deserialize<Overview>(
                    request.OverviewJson, 
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
            }
            else 
            {
                product.Overview = new Overview { Details = request.OverviewDetails };
                if (!string.IsNullOrEmpty(request.SpecificationsJson))
                {
                    product.Overview.Specifications = JsonSerializer.Deserialize<List<Specification>>(
                        request.SpecificationsJson, 
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
                }
            }

            // Handle Image Upload
            if (request.ImageFile != null)
            {
                product.Image = await _fileService.SaveFileAsync(request.ImageFile, "products/main");
            }

            // Handle Related Images
            if (request.RelatedImageFiles != null && request.RelatedImageFiles.Any())
            {
                foreach (var file in request.RelatedImageFiles)
                {
                    product.RelatedImages.Add(await _fileService.SaveFileAsync(file, "products/related"));
                }
            }

            // Handle Documents
            await MapDocumentsFromRequest(request, product);

            var id = await _productRepository.CreateProductAsync(product);
            product.Id = id;

            return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Update a product (Admin only) - Supports multipart/form-data
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromForm] ProductRequest request)
    {
        try
        {
            var existingProduct = await _productRepository.GetProductByIdAsync(id);
            if (existingProduct == null) return NotFound();

            // Use existing product as the template
            var product = existingProduct;
            product.UpdatedAt = DateTime.UtcNow;

            // Only update fields if they are provided in the request
            if (!string.IsNullOrEmpty(request.Name)) product.Name = request.Name;
            if (!string.IsNullOrEmpty(request.Description)) product.Description = request.Description;
            if (!string.IsNullOrEmpty(request.Category)) product.Category = request.Category;
            if (!string.IsNullOrEmpty(request.SubCategory)) product.SubCategory = request.SubCategory;
            if (!string.IsNullOrEmpty(request.Brand)) product.Brand = request.Brand;
            if (!string.IsNullOrEmpty(request.ApplicationRange)) product.ApplicationRange = request.ApplicationRange;

            if (request.Advantages != null && request.Advantages.Any()) product.Advantages = request.Advantages;
            if (request.Precautions != null && request.Precautions.Any()) product.Precautions = request.Precautions;

            // Handle Overview
            if (!string.IsNullOrEmpty(request.OverviewJson))
            {
                product.Overview = JsonSerializer.Deserialize<Overview>(
                    request.OverviewJson, 
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
            }
            else if (!string.IsNullOrEmpty(request.OverviewDetails) || !string.IsNullOrEmpty(request.SpecificationsJson))
            {
                if (product.Overview == null) product.Overview = new Overview();
                
                if (!string.IsNullOrEmpty(request.OverviewDetails)) 
                    product.Overview.Details = request.OverviewDetails;

                if (!string.IsNullOrEmpty(request.SpecificationsJson))
                {
                    product.Overview.Specifications = JsonSerializer.Deserialize<List<Specification>>(
                        request.SpecificationsJson, 
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
                }
            }

            // Handle new file uploads (only if provided and NOT empty)
            if (request.ImageFile != null && request.ImageFile.Length > 0)
            {
                await _fileService.DeleteFileAsync(existingProduct.Image ?? "", "products/main");
                product.Image = await _fileService.SaveFileAsync(request.ImageFile, "products/main");
            }

            if (request.RelatedImageFiles != null && request.RelatedImageFiles.Any(f => f.Length > 0))
            {
                foreach (var img in existingProduct.RelatedImages) await _fileService.DeleteFileAsync(img, "products/related");
                product.RelatedImages = new List<string>();
                foreach (var file in request.RelatedImageFiles.Where(f => f.Length > 0))
                {
                    product.RelatedImages.Add(await _fileService.SaveFileAsync(file, "products/related"));
                }
            }

            // Documents replacement logic
            if (request.TechnicalDataSheetFiles?.Any(f => f.Length > 0) == true || 
                request.SafetyDataSheetFiles?.Any(f => f.Length > 0) == true ||
                request.SalesBrochureFiles?.Any(f => f.Length > 0) == true ||
                request.CompanyProfileFiles?.Any(f => f.Length > 0) == true)
            {
                await MapDocumentsFromRequest(request, product);
            }

            var success = await _productRepository.UpdateProductAsync(product);
            if (!success) return BadRequest(new { message = "Update failed" });

            return Ok(product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product {Id}", id);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Delete a product (Admin only)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var product = await _productRepository.GetProductByIdAsync(id);
            if (product == null) return NotFound();

            // Cleanup files
            await _fileService.DeleteFileAsync(product.Image ?? "", "products/main");
            foreach (var img in product.RelatedImages) await _fileService.DeleteFileAsync(img, "products/related");
            foreach (var docType in product.Documents)
            {
                foreach (var file in docType.Value)
                {
                    await _fileService.DeleteFileAsync(file, $"documents/{docType.Key}");
                }
            }

            var success = await _productRepository.DeleteProductAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product {Id}", id);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    private async Task MapDocumentsFromRequest(ProductRequest request, Product product)
    {
        if (request.TechnicalDataSheetFiles != null)
        {
            if (!product.Documents.ContainsKey("technical_data_sheet")) product.Documents["technical_data_sheet"] = new();
            foreach (var file in request.TechnicalDataSheetFiles.Where(f => f.Length > 0))
                product.Documents["technical_data_sheet"].Add(await _fileService.SaveFileAsync(file, "documents/technical_data_sheet"));
        }

        if (request.SafetyDataSheetFiles != null)
        {
            if (!product.Documents.ContainsKey("safety_data_sheet")) product.Documents["safety_data_sheet"] = new();
            foreach (var file in request.SafetyDataSheetFiles.Where(f => f.Length > 0))
                product.Documents["safety_data_sheet"].Add(await _fileService.SaveFileAsync(file, "documents/safety_data_sheet"));
        }

        if (request.SalesBrochureFiles != null)
        {
            if (!product.Documents.ContainsKey("sales_brochure")) product.Documents["sales_brochure"] = new();
            foreach (var file in request.SalesBrochureFiles.Where(f => f.Length > 0))
                product.Documents["sales_brochure"].Add(await _fileService.SaveFileAsync(file, "documents/sales_brochure"));
        }

        if (request.CompanyProfileFiles != null)
        {
            if (!product.Documents.ContainsKey("company_profile")) product.Documents["company_profile"] = new();
            foreach (var file in request.CompanyProfileFiles.Where(f => f.Length > 0))
                product.Documents["company_profile"].Add(await _fileService.SaveFileAsync(file, "documents/company_profile"));
        }
    }
}
