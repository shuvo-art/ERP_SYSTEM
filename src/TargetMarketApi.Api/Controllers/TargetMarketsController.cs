using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TargetMarketApi.Api.DTOs;
using TargetMarketApi.Core.Entities;
using TargetMarketApi.Core.Interfaces;

namespace TargetMarketApi.Api.Controllers;

[ApiController]
[Route("api/v1/target-markets")]
public class TargetMarketsController : ControllerBase
{
    private readonly ITargetMarketRepository _repository;
    private readonly ICloudinaryService _cloudinaryService;
    private readonly ILogger<TargetMarketsController> _logger;

    public TargetMarketsController(
        ITargetMarketRepository repository,
        ICloudinaryService cloudinaryService,
        ILogger<TargetMarketsController> logger)
    {
        _repository = repository;
        _cloudinaryService = cloudinaryService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int limit = 10)
    {
        try
        {
            var (data, total) = await _repository.GetAllAsync(search, page, limit);
            return Ok(new
            {
                data,
                total,
                page,
                limit
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting target markets");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var market = await _repository.GetByIdAsync(id);
            if (market == null) return NotFound();
            return Ok(market);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting target market {Id}", id);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromForm] TargetMarketRequest request)
    {
        try
        {
            var market = new TargetMarket
            {
                Name = request.Name,
                Description = request.Description,
                SubItems = request.SubItems
            };

            if (request.ImageFile != null)
            {
                market.ImageUrl = await _cloudinaryService.UploadImageAsync(request.ImageFile, "target-markets");
            }

            var id = await _repository.CreateAsync(market);
            market.Id = id;

            return CreatedAtAction(nameof(GetById), new { id = market.Id }, market);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating target market");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromForm] TargetMarketRequest request)
    {
        try
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null) return NotFound();

            existing.Name = request.Name;
            existing.Description = request.Description;
            existing.SubItems = request.SubItems;

            if (request.ImageFile != null)
            {
                if (!string.IsNullOrEmpty(existing.ImageUrl))
                {
                    await _cloudinaryService.DeleteImageAsync(existing.ImageUrl);
                }
                existing.ImageUrl = await _cloudinaryService.UploadImageAsync(request.ImageFile, "target-markets");
            }

            var success = await _repository.UpdateAsync(existing);
            if (!success) return BadRequest(new { message = "Update failed" });

            return Ok(existing);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating target market {Id}", id);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpPatch("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Patch(int id, [FromForm] TargetMarketPatchRequest request)
    {
        try
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null) return NotFound();

            if (request.Name != null) existing.Name = request.Name;
            if (request.Description != null) existing.Description = request.Description;
            if (request.SubItems != null) existing.SubItems = request.SubItems;

            if (request.ImageFile != null)
            {
                if (!string.IsNullOrEmpty(existing.ImageUrl))
                {
                    await _cloudinaryService.DeleteImageAsync(existing.ImageUrl);
                }
                existing.ImageUrl = await _cloudinaryService.UploadImageAsync(request.ImageFile, "target-markets");
            }

            var success = await _repository.UpdateAsync(existing);
            return success ? Ok(existing) : BadRequest(new { message = "Patch failed" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error patching target market {Id}", id);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null) return NotFound();

            if (!string.IsNullOrEmpty(existing.ImageUrl))
            {
                await _cloudinaryService.DeleteImageAsync(existing.ImageUrl);
            }

            var success = await _repository.DeleteAsync(id);
            return success ? NoContent() : BadRequest(new { message = "Delete failed" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting target market {Id}", id);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }
}
