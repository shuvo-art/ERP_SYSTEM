using ContactApi.Api.DTOs;
using ContactApi.Core.Entities;
using ContactApi.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ContactApi.Api.Controllers
{
    [ApiController]
    [Route("api/v1/contact/enquiries")]
    public class EnquiriesController : ControllerBase
    {
        private readonly IEnquiryRepository _enquiryRepository;

        public EnquiriesController(IEnquiryRepository enquiryRepository)
        {
            _enquiryRepository = enquiryRepository;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] EnquiryRequest request)
        {
            var enquiry = new Enquiry
            {
                Type = request.Type,
                Name = request.Name,
                Designation = request.Designation,
                Mobile = request.Mobile,
                Email = request.Email,
                Address = request.Address,
                Country = request.Country,
                CompanyName = request.CompanyName,
                ProductId = request.ProductId,
                Message = request.Message,
                RequestCallBack = request.RequestCallBack,
                AgreeDataProtection = request.AgreeDataProtection
            };

            var id = await _enquiryRepository.CreateAsync(enquiry);

            return CreatedAtAction(nameof(GetById), new { id = id }, new
            {
                message = "Enquiry submitted successfully. We will get back to you soon.",
                enquiry_id = id
            });
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? type,
            [FromQuery] string? status,
            [FromQuery] string? search,
            [FromQuery] DateTime? date_from,
            [FromQuery] DateTime? date_to)
        {
            var enquiries = await _enquiryRepository.GetAllAsync(type, status, search, date_from, date_to);
            return Ok(enquiries);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var enquiry = await _enquiryRepository.GetByIdAsync(id);
            if (enquiry == null) return NotFound();
            return Ok(enquiry);
        }

        [HttpPatch("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] EnquiryUpdateStatusRequest request)
        {
            var success = await _enquiryRepository.UpdateStatusAsync(id, request.Status, request.AdminNotes);
            if (!success) return NotFound();
            return Ok(new { message = "Enquiry status updated successfully" });
        }
    }
}
