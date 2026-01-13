using ContactApi.Api.DTOs;
using ContactApi.Core.Entities;
using ContactApi.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ContactApi.Api.Controllers
{
    [ApiController]
    [Route("api/v1/distributors")]
    public class DistributorsController : ControllerBase
    {
        private readonly IDistributorRepository _distributorRepository;

        public DistributorsController(IDistributorRepository distributorRepository)
        {
            _distributorRepository = distributorRepository;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] DistributorRequest request)
        {
            var distributor = new Distributor
            {
                Name = request.Name,
                Address = request.Address,
                Phone = request.Phone,
                Country = request.Country,
                Email = request.Email,
                Website = request.Website,
                IsActive = request.IsActive,
                DisplayOrder = request.DisplayOrder
            };

            var id = await _distributorRepository.CreateAsync(distributor);
            return CreatedAtAction(nameof(GetById), new { id = id }, distributor);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] bool? @public)
        {
            // If public is true, only return active ones. If null or false, return all (usually for admin).
            bool? isActive = (@public == true) ? true : null;
            var distributors = await _distributorRepository.GetAllAsync(isActive);
            return Ok(distributors);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var distributor = await _distributorRepository.GetByIdAsync(id);
            if (distributor == null) return NotFound();
            return Ok(distributor);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(Guid id, [FromBody] DistributorRequest request)
        {
            var distributor = new Distributor
            {
                Id = id,
                Name = request.Name,
                Address = request.Address,
                Phone = request.Phone,
                Country = request.Country,
                Email = request.Email,
                Website = request.Website,
                IsActive = request.IsActive,
                DisplayOrder = request.DisplayOrder
            };

            var success = await _distributorRepository.UpdateAsync(distributor);
            if (!success) return NotFound();
            return Ok(new { message = "Distributor updated successfully" });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _distributorRepository.DeleteAsync(id);
            if (!success) return NotFound();
            return Ok(new { message = "Distributor deleted successfully" });
        }

        [HttpPatch("reorder")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Reorder([FromBody] DistributorReorderRequest request)
        {
            var success = await _distributorRepository.UpdateOrderAsync(request.Order);
            if (!success) return StatusCode(500, "Failed to reorder distributors");
            return Ok(new { message = "Distributors reordered successfully" });
        }
    }
}
