using System.Text.Json.Serialization;

namespace PartnerApi.Core.Entities;

public class ProductSegment
{
    public int Id { get; set; }
    public int PartnerId { get; set; }
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("image_url")]
    public string? ImageUrl { get; set; }
}
