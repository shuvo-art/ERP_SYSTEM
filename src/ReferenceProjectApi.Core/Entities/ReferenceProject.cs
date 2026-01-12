using System;
using System.Collections.Generic;

namespace ReferenceProjectApi.Core.Entities;

public class ReferenceProject
{
    public int Id { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;
    public string? HeroImageUrl { get; set; }
    public List<string> GalleryImages { get; set; } = new();
    public string Location { get; set; } = string.Empty;
    
    // Project Overview flattened or as object? Sticking to entity structure request
    // Storing as JSON or separate columns. Let's use a class property for cleaner code, repository will map it.
    public ProjectOverview? ProjectOverview { get; set; }
    
    public List<ProjectProduct> ProductsUsed { get; set; } = new();
    
    public string Status { get; set; } = "ongoing"; // completed | ongoing | upcoming
    public DateTime? StartDate { get; set; }
    public DateTime? CompletionDate { get; set; }
    public bool Featured { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class ProjectOverview
{
    public string ProjectName { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string? OwnerName { get; set; }
    public string? Contractor { get; set; }
    public string? Engineer { get; set; }
    public string? Client { get; set; }
}

public class ProjectProduct
{
    public string ProductId { get; set; } = string.Empty; // From Product Microservice
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
}
