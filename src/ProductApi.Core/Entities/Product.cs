using System.Text.Json.Serialization;

namespace ProductApi.Core.Entities;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? ShortDescription { get; set; }
    public string? MainImage { get; set; }
    
    // Master data relations (Stored as IDs in DB for performance and integrity)
    public int? CategoryId { get; set; }
    public int? SubCategoryId { get; set; }
    public int? BrandId { get; set; }
    public int? UnitId { get; set; }
    public int? CountryId { get; set; }

    // Display names for API response
    public string? CategoryName { get; set; }
    public string? SubCategoryName { get; set; }
    public string? BrandName { get; set; }
    public string? UnitName { get; set; }
    public string? CountryName { get; set; }

    // Rich Text Content
    public string? OverviewHtml { get; set; }
    public string? AdvantageHtml { get; set; }
    public string? ApplicationRangeHtml { get; set; }
    public string? PrecautionHtml { get; set; }

    // Structured Specifications (Stored as JSON in DB)
    public ProductSpecifications Specifications { get; set; } = new();

    // Media and Documents
    public List<string> RelatedImages { get; set; } = new();
    public List<ProductDocument> TechnicalDataSheets { get; set; } = new(); // TDS
    public List<ProductDocument> SafetyDataSheets { get; set; } = new();    // SDS
    public List<ProductDocument> Certificates { get; set; } = new();        // Product Certificates

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

public class ProductSpecifications
{
    public List<string> PackSizes { get; set; } = new();
    public List<string> PackagingDetails { get; set; } = new();
    public List<string> Colors { get; set; } = new();
    public List<string> Thicknesses { get; set; } = new();
    public List<string> Densities { get; set; } = new();
    public List<string> Appearances { get; set; } = new();
    public List<string> DosageCoverages { get; set; } = new();
    public List<string> ShelfLife { get; set; } = new();
}

public class ProductDocument
{
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}
