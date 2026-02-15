using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductApi.Core.Entities;
using ProductApi.Core.Interfaces;

namespace ProductApi.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class BrandsController : ControllerBase
{
    private readonly IMasterDataRepository _repository;
    private readonly ICloudinaryService _cloudinary;

    public BrandsController(IMasterDataRepository repository, ICloudinaryService cloudinary)
    {
        _repository = repository;
        _cloudinary = cloudinary;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _repository.GetBrandsAsync());

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Create([FromForm] string name, [FromForm] IFormFile? logo)
    {
        var brand = new BrandMaster { Name = name };
        if (logo != null) brand.Logo = await _cloudinary.UploadImageAsync(logo, "brands");
        var id = await _repository.CreateBrandAsync(brand);
        brand.Id = id;
        return CreatedAtAction(nameof(GetAll), new { id = brand.Id }, brand);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Update(int id, [FromForm] string name, [FromForm] IFormFile? logo)
    {
        var brand = new BrandMaster { Id = id, Name = name };
        if (logo != null) brand.Logo = await _cloudinary.UploadImageAsync(logo, "brands");
        var success = await _repository.UpdateBrandAsync(brand);
        return success ? Ok(brand) : BadRequest();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _repository.DeleteBrandAsync(id);
        return success ? NoContent() : NotFound();
    }
}
