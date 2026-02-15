using System;
using System.Collections.Generic;

namespace ReferenceProjectApi.Core.Entities;

public class ReferenceProject
{
    public int Id { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public string? Location { get; set; }
    public string? OwnerName { get; set; }
    public string? Contractor { get; set; }
    public string? EngineerName { get; set; }
    public string? ClientName { get; set; }
    public string? ShortDescription { get; set; }
    public string? DetailsDescription { get; set; } // rich text / HTML
    public string Status { get; set; } = "ongoing"; // completed | ongoing | upcoming
    public DateTime? StartDate { get; set; }
    public DateTime? CompletionDate { get; set; }
    public bool Featured { get; set; }
    public string? HeroImageUrl { get; set; }
    public string? ProjectOverviewJson { get; set; } // Store as NVARCHAR(MAX)
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Foregin Key for Category
    public int CategoryId { get; set; }
    public virtual ProjectCategory Category { get; set; } = null!;

    // Navigation properties
    public virtual ICollection<ProjectGalleryImage> GalleryImages { get; set; } = new List<ProjectGalleryImage>();
    public virtual ICollection<ProjectDetailImage> DetailImages { get; set; } = new List<ProjectDetailImage>();
    public virtual ICollection<ProjectProductJunction> ProjectProducts { get; set; } = new List<ProjectProductJunction>();
}
