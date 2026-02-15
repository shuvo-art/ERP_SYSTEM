using Microsoft.AspNetCore.Http;
using ReferenceProjectApi.Core.Entities;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace ReferenceProjectApi.Core.DTOs;

public class ReferenceProjectRequest
{
    [Required]
    public string ProjectName { get; set; } = string.Empty;
    
    [Required]
    public int CategoryId { get; set; }
    
    public string? ShortDescription { get; set; }
    public string? DetailsDescription { get; set; } // rich text / HTML
    public string? Location { get; set; }
    public string? OwnerName { get; set; }
    public string? Contractor { get; set; }
    public string? EngineerName { get; set; }
    public string? ClientName { get; set; }
    
    public IFormFile? HeroImage { get; set; }
    public List<IFormFile>? GalleryImages { get; set; }
    public List<IFormFile>? DetailImages { get; set; }
    
    // JSON strings for complex types when using multipart/form-data
    public string? ProjectOverviewJson { get; set; }
    
    // Admin can ONLY select from existing products (IDs only)
    public string? ProductIdsJson { get; set; } // e.g. "[1, 4, 7]"
    
    public string Status { get; set; } = "ongoing";
    public DateTime? StartDate { get; set; }
    public DateTime? CompletionDate { get; set; }
    public bool Featured { get; set; }
}

public class ReferenceProjectResponse
{
    public int Id { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public int CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public string? Location { get; set; }
    public string? OwnerName { get; set; }
    public string? Contractor { get; set; }
    public string? EngineerName { get; set; }
    public string? ClientName { get; set; }
    public string? ShortDescription { get; set; }
    public string? DetailsDescription { get; set; }
    public string? HeroImageUrl { get; set; }
    public List<string> GalleryImages { get; set; } = new();
    public List<string> DetailImages { get; set; } = new();
    public List<ProductSimpleDto> ProductsUsed { get; set; } = new();
    public dynamic? ProjectOverview { get; set; } // Parsed JSON
    public string Status { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
    public DateTime? CompletionDate { get; set; }
    public bool Featured { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ProductSimpleDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
