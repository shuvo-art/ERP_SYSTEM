using System.Text.Json.Serialization;

namespace PartnerApi.Core.Entities;

public class Partner
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    
    [JsonPropertyName("short_description")]
    public string? ShortDescription { get; set; }
    
    [JsonPropertyName("long_description_title")]
    public string? LongDescriptionTitle { get; set; }
    
    [JsonPropertyName("long_description")]
    public string? LongDescription { get; set; }
    
    [JsonPropertyName("logo_url")]
    public string? LogoUrl { get; set; }
    
    [JsonPropertyName("building_image_url")]
    public string? BuildingImageUrl { get; set; }
    
    [JsonPropertyName("company_profile")]
    public Dictionary<string, string> CompanyProfile { get; set; } = new();
    
    [JsonPropertyName("product_segments")]
    public List<ProductSegment> ProductSegments { get; set; } = new();
    
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }
    
    [JsonPropertyName("updated_at")]
    public DateTime? UpdatedAt { get; set; }
}

public class ProductSegment
{
    public string Title { get; set; } = string.Empty;
    
    [JsonPropertyName("image_url")]
    public string? ImageUrl { get; set; }
}
