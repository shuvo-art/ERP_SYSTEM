using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace TargetMarketApi.Core.DTOs;

public class TargetMarketRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<string> SubItems { get; set; } = new();
    public IFormFile? ImageFile { get; set; }
}

public class TargetMarketPatchRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public List<string>? SubItems { get; set; }
    public IFormFile? ImageFile { get; set; }
}
