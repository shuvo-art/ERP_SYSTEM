using System;
using System.Collections.Generic;

namespace JobApi.Core.Entities;

public class JobPosting
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public int ExperienceYears { get; set; }
    public string JobType { get; set; } = string.Empty; // Full Time, Part Time, etc.
    public string ContractType { get; set; } = string.Empty; // Permanent, Contract, etc.
    public string Location { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Responsibilities { get; set; } = new();
    public List<string> Qualifications { get; set; } = new();
    public List<string> Skills { get; set; } = new();
    public string Status { get; set; } = "draft"; // active | closed | draft
    public DateTime ApplicationDeadline { get; set; }
    public bool IsFeatured { get; set; }
    public string? BannerImageUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
