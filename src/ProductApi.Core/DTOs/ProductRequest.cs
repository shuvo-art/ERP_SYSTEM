using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace ProductApi.Core.DTOs;

public class ProductRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;
    public string? ShortDescription { get; set; }

    // Master Data IDs
    public int? CategoryId { get; set; }
    public int? SubCategoryId { get; set; }
    public int? BrandId { get; set; }
    public int? UnitId { get; set; }
    public int? CountryId { get; set; }

    // Rich Text Contents
    public string? OverviewHtml { get; set; }
    public string? AdvantageHtml { get; set; }
    public string? ApplicationRangeHtml { get; set; }
    public string? PrecautionHtml { get; set; }

    // Structured Specs (JSON Strings from Frontend)
    public string? SpecificationsJson { get; set; }

    // Files
    public IFormFile? MainImageFile { get; set; }
    public List<IFormFile>? RelatedImageFiles { get; set; }
    
    // Document names as parallel lists or within JSON is tricky with files.
    // Standard approach for multiple files with names in multipart:
    // Files are usually sent with specific keys like 'TechnicalDataSheetFiles'
}
