using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace JobApi.Core.DTOs;

public class JobRequest
{
    [Required]
    public string Title { get; set; } = string.Empty;
    [Required]
    public string Department { get; set; } = string.Empty;
    [Required]
    public int ExperienceYears { get; set; }
    [Required]
    public string JobType { get; set; } = string.Empty;
    [Required]
    public string ContractType { get; set; } = string.Empty;
    [Required]
    public string Location { get; set; } = string.Empty;
    [Required]
    public string Description { get; set; } = string.Empty;
    
    // JSON strings for arrays when using multipart/form-data
    public string? ResponsibilitiesJson { get; set; }
    public string? QualificationsJson { get; set; }
    public string? SkillsJson { get; set; }
    
    public string Status { get; set; } = "draft";
    [Required]
    public DateTime ApplicationDeadline { get; set; }
    public bool IsFeatured { get; set; }
    public IFormFile? BannerImage { get; set; }
}

public class ApplicationRequest
{
    [Required]
    public string FirstName { get; set; } = string.Empty;
    [Required]
    public string LastName { get; set; } = string.Empty;
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    [Required]
    public string Phone { get; set; } = string.Empty;
    [Required]
    public string Address { get; set; } = string.Empty;
    
    // JSON strings for complex types
    public string? ExperienceJson { get; set; }
    public string? EducationJson { get; set; }
    
    [Required]
    public IFormFile Resume { get; set; } = null!;
    public string? CoverMessage { get; set; }
}

public class ApplicationStatusUpdateDto
{
    [Required]
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
}
