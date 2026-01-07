using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace ProductApi.Api.DTOs;

public class ProductRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    
    public IFormFile? ImageFile { get; set; }
    public List<IFormFile>? RelatedImageFiles { get; set; }
    
    public string? Category { get; set; }
    public string? SubCategory { get; set; }
    public string? Brand { get; set; }
    public string? ApplicationRange { get; set; }

    public string? OverviewDetails { get; set; }
    
    // JSON string for specifications: "[{\"title\":\"Size\", \"items\":[\"10L\"]}]"
    public string? SpecificationsJson { get; set; }

    // Or send the entire overview as one JSON: "{\"details\": \"...\", \"specifications\": [...]}"
    public string? OverviewJson { get; set; }
    
    public List<string> Advantages { get; set; } = new();
    public List<string> Precautions { get; set; } = new();

    // Specific file lists for each document type
    public List<IFormFile>? TechnicalDataSheetFiles { get; set; }
    public List<IFormFile>? SafetyDataSheetFiles { get; set; }
    public List<IFormFile>? SalesBrochureFiles { get; set; }
    public List<IFormFile>? CompanyProfileFiles { get; set; }
}
