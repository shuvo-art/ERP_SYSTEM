namespace ProductApi.Core.Entities;

public class CategoryMaster
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Image { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class SubCategoryMaster
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Many-to-Many Relationship
    public List<int> CategoryIds { get; set; } = new();
}

public class BrandMaster
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Logo { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class UnitMaster
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class CountryMaster
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
