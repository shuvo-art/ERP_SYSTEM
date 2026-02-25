using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace PartnerApi.Core.DTOs;

public class PartnerRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;
    public string? ShortDescription { get; set; }
    public string? DetailsDescriptionTitle { get; set; }
    public string? DetailsDescription { get; set; }
    
    // Company Profile
    public string? CompanyName { get; set; }
    public string? BrandName { get; set; }
    public string? EstablishedIn { get; set; }
    public string? Website { get; set; }
    
    // JSON strings for complex types when using FromForm
    public string? ProductSegmentsJson { get; set; } // List of { Name: string }
    public string? DocumentsJson { get; set; } // List of { Name: string }
    
    public IFormFile? LogoFile { get; set; }
    public IFormFile? BuildingImageFile { get; set; }
    public IFormFile? VideoFile { get; set; }
    
    // For mapping multiple product segment and document files
    public List<IFormFile>? ProductSegmentFiles { get; set; }
    public List<IFormFile>? DocumentFiles { get; set; }
}

public class PartnerPatchRequest
{
    public string? Name { get; set; }
    public string? ShortDescription { get; set; }
    public string? DetailsDescriptionTitle { get; set; }
    public string? DetailsDescription { get; set; }
    
    public string? CompanyName { get; set; }
    public string? BrandName { get; set; }
    public string? EstablishedIn { get; set; }
    public string? Website { get; set; }

    public string? ProductSegmentsJson { get; set; }
    public string? DocumentsJson { get; set; }
    
    public IFormFile? LogoFile { get; set; }
    public IFormFile? BuildingImageFile { get; set; }
    public IFormFile? VideoFile { get; set; }
    
    public List<IFormFile>? ProductSegmentFiles { get; set; }
    public List<IFormFile>? DocumentFiles { get; set; }
}
