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

    // Files and Documents
    public IFormFile? MainImageFile { get; set; }
    public List<IFormFile>? RelatedImageFiles { get; set; }
    
    public List<IFormFile>? TechnicalDataSheetFiles { get; set; }
    public string? TechnicalDataSheetNamesJson { get; set; } // ["Name A", "Name B"]
    
    public List<IFormFile>? SafetyDataSheetFiles { get; set; }
    public string? SafetyDataSheetNamesJson { get; set; }
    
    public List<IFormFile>? CertificateFiles { get; set; }
    public string? CertificateNamesJson { get; set; }
}

public class ProductPatchRequest
{
    public string? Name { get; set; }
    public string? ShortDescription { get; set; }
    public int? CategoryId { get; set; }
    public int? SubCategoryId { get; set; }
    public int? BrandId { get; set; }
    public int? UnitId { get; set; }
    public int? CountryId { get; set; }
    public string? OverviewHtml { get; set; }
    public string? AdvantageHtml { get; set; }
    public string? ApplicationRangeHtml { get; set; }
    public string? PrecautionHtml { get; set; }
    public string? SpecificationsJson { get; set; }
    public IFormFile? MainImageFile { get; set; }
    public List<IFormFile>? RelatedImageFiles { get; set; }
    public List<IFormFile>? TechnicalDataSheetFiles { get; set; }
    public string? TechnicalDataSheetNamesJson { get; set; }
    public List<IFormFile>? SafetyDataSheetFiles { get; set; }
    public string? SafetyDataSheetNamesJson { get; set; }
    public List<IFormFile>? CertificateFiles { get; set; }
    public string? CertificateNamesJson { get; set; }
}
