using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace ProductApi.Core.DTOs;

public class BrandRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;
    public IFormFile? Logo { get; set; }
}

public class CategoryRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;
    public IFormFile? Image { get; set; }
}

public class SubCategoryRequest
{
    [Required]
    [MinLength(1, ErrorMessage = "At least one Category ID is required.")]
    public List<int> CategoryIds { get; set; } = new();
    
    [Required]
    public string Name { get; set; } = string.Empty;
}

public class UnitRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;
}

public class CountryRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;
}

public class SubCategoryPatchRequest
{
    public string? Name { get; set; }
    public List<int>? CategoryIds { get; set; }
}
