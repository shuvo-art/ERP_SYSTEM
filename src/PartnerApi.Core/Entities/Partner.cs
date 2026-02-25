using System.Text.Json.Serialization;

namespace PartnerApi.Core.Entities;

public class Partner
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty; // Title/Company Name
    public string Slug { get; set; } = string.Empty;
    
    [JsonPropertyName("short_description")]
    public string? ShortDescription { get; set; } // Rich text
    
    [JsonPropertyName("logo_url")]
    public string? LogoUrl { get; set; }
    
    [JsonPropertyName("building_image_url")]
    public string? BuildingImageUrl { get; set; } // Upload Image for short Details
    
    [JsonPropertyName("details_description_title")]
    public string? DetailsDescriptionTitle { get; set; }
    
    [JsonPropertyName("details_description")]
    public string? DetailsDescription { get; set; } // Rich text
    
    [JsonPropertyName("video_url")]
    public string? VideoUrl { get; set; }

    // Company Profile fields (Flattened)
    [JsonPropertyName("company_name")]
    public string? CompanyName { get; set; }
    
    [JsonPropertyName("brand_name")]
    public string? BrandName { get; set; }
    
    [JsonPropertyName("established_in")]
    public string? EstablishedIn { get; set; }
    
    [JsonPropertyName("website")]
    public string? Website { get; set; }

    [JsonPropertyName("product_segments")]
    public List<ProductSegment> ProductSegments { get; set; } = new();

    [JsonPropertyName("documents")]
    public List<PartnerDocument> Documents { get; set; } = new();
    
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }
    
    [JsonPropertyName("updated_at")]
    public DateTime? UpdatedAt { get; set; }
}
