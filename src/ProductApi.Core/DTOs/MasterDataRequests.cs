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
    public int CategoryId { get; set; }
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
