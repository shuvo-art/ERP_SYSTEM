using System.Text.Json.Serialization;

namespace PartnerApi.Core.Entities;

public class PartnerDocument
{
    public int Id { get; set; }
    public int PartnerId { get; set; }
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("document_url")]
    public string? DocumentUrl { get; set; }
}
