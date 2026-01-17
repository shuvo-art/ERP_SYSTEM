using Microsoft.AspNetCore.Mvc;
using JobApi.Core.Entities;
using JobApi.Core.Interfaces;
using JobApi.Core.DTOs;
using System.Text.Json;

namespace JobApi.Api.Controllers;

[ApiController]
[Route("api/v1/jobs")]
public class JobsController : ControllerBase
{
    private readonly IJobRepository _repository;
    private readonly ICloudinaryService _cloudinary;
    private readonly ILogger<JobsController> _logger;
    private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public JobsController(
        IJobRepository repository, 
        ICloudinaryService cloudinary,
        ILogger<JobsController> logger)
    {
        _repository = repository;
        _cloudinary = cloudinary;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? status = null, [FromQuery] string? department = null, [FromQuery] string? location = null)
    {
        try
        {
            var jobs = await _repository.GetAllAsync(status, department, location);
            return Ok(new { data = jobs });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting jobs");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpGet("{idOrSlug}")]
    public async Task<IActionResult> Get(string idOrSlug)
    {
        try
        {
            JobPosting? job;
            if (Guid.TryParse(idOrSlug, out var id))
            {
                job = await _repository.GetByIdAsync(id);
            }
            else
            {
                job = await _repository.GetBySlugAsync(idOrSlug);
            }

            if (job == null) return NotFound();
            return Ok(job);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting job {IdOrSlug}", idOrSlug);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromForm] JobRequest request)
    {
        try
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var slug = request.Title.ToLower()
                .Replace(" ", "-")
                .Replace(",", "")
                .Replace(".", "")
                .Trim();
            
            var job = new JobPosting
            {
                Title = request.Title,
                Slug = slug,
                Department = request.Department,
                ExperienceYears = request.ExperienceYears,
                JobType = request.JobType,
                ContractType = request.ContractType,
                Location = request.Location,
                Description = request.Description,
                Status = request.Status,
                ApplicationDeadline = request.ApplicationDeadline,
                IsFeatured = request.IsFeatured,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            if (!string.IsNullOrEmpty(request.ResponsibilitiesJson))
                job.Responsibilities = JsonSerializer.Deserialize<List<string>>(request.ResponsibilitiesJson, _jsonOptions) ?? new();
            
            if (!string.IsNullOrEmpty(request.QualificationsJson))
                job.Qualifications = JsonSerializer.Deserialize<List<string>>(request.QualificationsJson, _jsonOptions) ?? new();
            
            if (!string.IsNullOrEmpty(request.SkillsJson))
                job.Skills = JsonSerializer.Deserialize<List<string>>(request.SkillsJson, _jsonOptions) ?? new();

            if (request.BannerImage != null)
            {
                job.BannerImageUrl = await _cloudinary.UploadImageAsync(request.BannerImage, "jobs/banners");
            }

            var id = await _repository.CreateAsync(job);
            job.Id = id;

            return CreatedAtAction(nameof(Get), new { idOrSlug = id }, job);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating job");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromForm] JobRequest request)
    {
        try
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null) return NotFound();

            existing.Title = request.Title;
            existing.Department = request.Department;
            existing.ExperienceYears = request.ExperienceYears;
            existing.JobType = request.JobType;
            existing.ContractType = request.ContractType;
            existing.Location = request.Location;
            existing.Description = request.Description;
            existing.Status = request.Status;
            existing.ApplicationDeadline = request.ApplicationDeadline;
            existing.IsFeatured = request.IsFeatured;
            existing.UpdatedAt = DateTime.UtcNow;

            existing.Slug = request.Title.ToLower().Replace(" ", "-").Replace(",", "").Replace(".", "").Trim();

            if (!string.IsNullOrEmpty(request.ResponsibilitiesJson))
                existing.Responsibilities = JsonSerializer.Deserialize<List<string>>(request.ResponsibilitiesJson, _jsonOptions) ?? new();
            
            if (!string.IsNullOrEmpty(request.QualificationsJson))
                existing.Qualifications = JsonSerializer.Deserialize<List<string>>(request.QualificationsJson, _jsonOptions) ?? new();
            
            if (!string.IsNullOrEmpty(request.SkillsJson))
                existing.Skills = JsonSerializer.Deserialize<List<string>>(request.SkillsJson, _jsonOptions) ?? new();

            if (request.BannerImage != null)
            {
                if (!string.IsNullOrEmpty(existing.BannerImageUrl)) await _cloudinary.DeleteFileAsync(existing.BannerImageUrl);
                existing.BannerImageUrl = await _cloudinary.UploadImageAsync(request.BannerImage, "jobs/banners");
            }

            await _repository.UpdateAsync(existing);
            return Ok(existing);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating job {Id}", id);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null) return NotFound();
            
            if (!string.IsNullOrEmpty(existing.BannerImageUrl)) await _cloudinary.DeleteFileAsync(existing.BannerImageUrl);
            
            await _repository.DeleteAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting job {Id}", id);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }
}
