using System;
using System.Collections.Generic;

namespace ReferenceProjectApi.Core.Entities;

public class ProjectCategory
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string? Slug { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public virtual ICollection<ReferenceProject> Projects { get; set; } = new List<ReferenceProject>();
}
