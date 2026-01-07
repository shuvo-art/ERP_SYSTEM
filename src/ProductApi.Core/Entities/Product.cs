using System.Text.Json.Serialization;

namespace ProductApi.Core.Entities;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Image { get; set; }
    
    [JsonPropertyName("related_images")]
    public List<string> RelatedImages { get; set; } = new();

    public string? Category { get; set; }

    [JsonPropertyName("sub_category")]
    public string? SubCategory { get; set; }

    public string? Brand { get; set; }

    [JsonPropertyName("application_range")]
    public string? ApplicationRange { get; set; }

    public Overview Overview { get; set; } = new();
    public List<string> Advantages { get; set; } = new();
    public List<string> Precautions { get; set; } = new();
    public Dictionary<string, List<string>> Documents { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class Overview
{
    public string? Details { get; set; }
    public List<Specification> Specifications { get; set; } = new();
}

public class Specification
{
    public string Title { get; set; } = string.Empty;
    public List<string> Items { get; set; } = new();
}
