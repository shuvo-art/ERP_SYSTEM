using System;
using System.Collections.Generic;

namespace JobApi.Core.Entities;

public class JobApplication
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid JobId { get; set; }
    public string JobTitle { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public List<ApplicationExperience> Experience { get; set; } = new();
    public List<ApplicationEducation> Education { get; set; } = new();
    public string ResumeUrl { get; set; } = string.Empty;
    public string? CoverMessage { get; set; }
    public string Status { get; set; } = "new"; // new | reviewed | shortlisted | rejected | hired
    public string? Notes { get; set; }
    public DateTime AppliedAt { get; set; } = DateTime.UtcNow;
}

public class ApplicationExperience
{
    public string Position { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;
    public string? Department { get; set; }
    public string? Address { get; set; }
    public string From { get; set; } = string.Empty;
    public string To { get; set; } = string.Empty;
    public bool Current { get; set; }
}

public class ApplicationEducation
{
    public string Institution { get; set; } = string.Empty;
    public string Degree { get; set; } = string.Empty;
    public string Major { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string From { get; set; } = string.Empty;
    public string To { get; set; } = string.Empty;
}
