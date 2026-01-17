using Microsoft.AspNetCore.Http;
using ReferenceProjectApi.Core.Entities;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace ReferenceProjectApi.Core.DTOs;

public class ReferenceProjectRequest
{
    [Required]
    public string ProjectName { get; set; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    
    public IFormFile? HeroImage { get; set; }
    public List<IFormFile>? GalleryImages { get; set; }
    
    // JSON strings for complex types when using multipart/form-data
    public string? ProjectOverviewJson { get; set; }
    public string? ProductsUsedJson { get; set; }
    
    public string Status { get; set; } = "ongoing";
    public DateTime? StartDate { get; set; }
    public DateTime? CompletionDate { get; set; }
    public bool Featured { get; set; }
}

public class ReferenceProjectResponse
{
    public int Id { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;
    public string? HeroImageUrl { get; set; }
    public List<string> GalleryImages { get; set; } = new();
    public string Location { get; set; } = string.Empty;
    public ProjectOverview? ProjectOverview { get; set; }
    public List<ProjectProduct> ProductsUsed { get; set; } = new();
    public string Status { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
    public DateTime? CompletionDate { get; set; }
    public bool Featured { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class ProjectOverviewDto
{
    public string ProjectName { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string? OwnerName { get; set; }
    public string? Contractor { get; set; }
    public string? Engineer { get; set; }
    public string? Client { get; set; }
}

public class ProjectProductDto
{
    public string ProductId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
}
