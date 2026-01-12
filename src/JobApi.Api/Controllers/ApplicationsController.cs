using Microsoft.AspNetCore.Mvc;
using JobApi.Core.Entities;
using JobApi.Core.Interfaces;
using JobApi.Api.DTOs;
using System.Text.Json;

namespace JobApi.Api.Controllers;

[ApiController]
[Route("api/v1/applications")]
public class ApplicationsController : ControllerBase
{
    private readonly IApplicationRepository _applicationRepository;
    private readonly IJobRepository _jobRepository;
    private readonly ICloudinaryService _cloudinary;
    private readonly ILogger<ApplicationsController> _logger;
    private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public ApplicationsController(
        IApplicationRepository applicationRepository,
        IJobRepository jobRepository,
        ICloudinaryService cloudinary,
        ILogger<ApplicationsController> logger)
    {
        _applicationRepository = applicationRepository;
        _jobRepository = jobRepository;
        _cloudinary = cloudinary;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? job_id = null, [FromQuery] string? status = null, [FromQuery] string? search = null)
    {
        try
        {
            var applications = await _applicationRepository.GetAllAsync(job_id, status, search);
            var total = await _applicationRepository.GetTotalCountAsync(job_id, status, search);

            return Ok(new 
            { 
                data = applications,
                total,
                filters = new { job_id, status, search }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting applications");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id)
    {
        try
        {
            var application = await _applicationRepository.GetByIdAsync(id);
            if (application == null) return NotFound();
            return Ok(application);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting application {Id}", id);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpPost("/api/v1/jobs/{jobId}/apply")]
    public async Task<IActionResult> Apply(Guid jobId, [FromForm] ApplicationRequest request)
    {
        try
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var job = await _jobRepository.GetByIdAsync(jobId);
            if (job == null) return BadRequest(new { message = "Invalid job id" });

            var application = new JobApplication
            {
                JobId = jobId,
                JobTitle = job.Title,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                Phone = request.Phone,
                Address = request.Address,
                CoverMessage = request.CoverMessage,
                AppliedAt = DateTime.UtcNow,
                Status = "new"
            };

            if (!string.IsNullOrEmpty(request.ExperienceJson))
                application.Experience = JsonSerializer.Deserialize<List<ApplicationExperience>>(request.ExperienceJson, _jsonOptions) ?? new();
            
            if (!string.IsNullOrEmpty(request.EducationJson))
                application.Education = JsonSerializer.Deserialize<List<ApplicationEducation>>(request.EducationJson, _jsonOptions) ?? new();

            if (request.Resume != null)
            {
                // Upload resume as a raw file (PDF/Docx)
                application.ResumeUrl = await _cloudinary.UploadFileAsync(request.Resume, "jobs/resumes");
            }

            var id = await _applicationRepository.CreateAsync(application);
            
            return CreatedAtAction(nameof(Get), new { id = id }, new 
            { 
                message = "Application submitted successfully",
                application_id = id 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting application");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] ApplicationStatusUpdateDto request)
    {
        try
        {
            var existing = await _applicationRepository.GetByIdAsync(id);
            if (existing == null) return NotFound();

            await _applicationRepository.UpdateStatusAsync(id, request.Status, request.Notes);
            return Ok(new { message = "Application status updated" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating application status {Id}", id);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var existing = await _applicationRepository.GetByIdAsync(id);
            if (existing == null) return NotFound();
            
            if (!string.IsNullOrEmpty(existing.ResumeUrl)) await _cloudinary.DeleteFileAsync(existing.ResumeUrl);
            
            await _applicationRepository.DeleteAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting application {Id}", id);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }
}
