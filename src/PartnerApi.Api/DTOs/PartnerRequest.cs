using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace PartnerApi.Api.DTOs;

public class PartnerRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;
    public string? ShortDescription { get; set; }
    public string? LongDescriptionTitle { get; set; }
    public string? LongDescription { get; set; }
    
    // JSON strings for complex types when using FromForm
    public string? CompanyProfileJson { get; set; }
    public string? ProductSegmentsJson { get; set; }
    
    public IFormFile? LogoFile { get; set; }
    public IFormFile? BuildingImageFile { get; set; }
    public IFormFile? BrochureFile { get; set; }
    
    // For mapping multiple product segment images
    public List<IFormFile>? ProductSegmentFiles { get; set; }
}

public class PartnerPatchRequest
{
    public string? Name { get; set; }
    public string? ShortDescription { get; set; }
    public string? LongDescriptionTitle { get; set; }
    public string? LongDescription { get; set; }
    public string? CompanyProfileJson { get; set; }
    public string? ProductSegmentsJson { get; set; }
    
    public IFormFile? LogoFile { get; set; }
    public IFormFile? BuildingImageFile { get; set; }
    public IFormFile? BrochureFile { get; set; }
    public List<IFormFile>? ProductSegmentFiles { get; set; }
}
