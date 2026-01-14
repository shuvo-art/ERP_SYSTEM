using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AboutUsApi.Api.DTOs;
using AboutUsApi.Core.Entities;
using AboutUsApi.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AboutUsApi.Api.Controllers
{
    [ApiController]
    [Route("api/v1/about-us")]
    public class AboutUsController : ControllerBase
    {
        private readonly IAboutUsRepository _repository;
        private readonly ICloudinaryService _cloudinaryService;

        public AboutUsController(IAboutUsRepository repository, ICloudinaryService cloudinaryService)
        {
            _repository = repository;
            _cloudinaryService = cloudinaryService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAboutUs()
        {
            var (sections, items) = await _repository.GetFullAboutUsAsync();
            var response = MapToResponseDto(sections, items);
            return Ok(response);
        }

        [HttpGet("sections/{sectionName}")]
        public async Task<IActionResult> GetSection(string sectionName)
        {
            var section = await _repository.GetSectionAsync(sectionName);
            if (section == null) return NotFound();

            var (allSections, allItems) = await _repository.GetFullAboutUsAsync();
            var sectionItems = allItems.Where(i => i.SectionId == sectionName).ToList();

            var response = MapToResponseDto(new List<AboutUsSection> { section }, sectionItems);
            
            // Return only the specific section part of the DTO
            var property = typeof(AboutUsResponseDto).GetProperties()
                .FirstOrDefault(p => p.Name.Equals(sectionName.Replace("_", ""), StringComparison.OrdinalIgnoreCase));
            
            if (property == null) return Ok(section);

            return Ok(property.GetValue(response));
        }

        [Authorize(Roles = "Admin")]
        [HttpPatch("sections/{sectionName}")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateSection(string sectionName, [FromForm] UpdateSectionRequest request)
        {
            var section = await _repository.GetSectionAsync(sectionName);
            if (section == null) return NotFound();

            var metadata = string.IsNullOrEmpty(section.MetadataJson) 
                ? new Dictionary<string, string>() 
                : JsonSerializer.Deserialize<Dictionary<string, string>>(section.MetadataJson) ?? new Dictionary<string, string>();

            if (request.BannerImage != null)
                metadata["banner_image_url"] = await _cloudinaryService.UploadImageAsync(request.BannerImage, "about-us/banner");
            
            if (request.Thumbnail != null)
                metadata["thumbnail_url"] = await _cloudinaryService.UploadImageAsync(request.Thumbnail, "about-us/video");

            if (!string.IsNullOrEmpty(request.VideoUrl))
                metadata["video_url"] = request.VideoUrl;

            if (request.CompanyProfilePdf != null)
                metadata["company_profile_pdf_url"] = await _cloudinaryService.UploadFileAsync(request.CompanyProfilePdf, "about-us/documents");

            if (request.ProductBrochurePdf != null)
                metadata["product_brochure_pdf_url"] = await _cloudinaryService.UploadFileAsync(request.ProductBrochurePdf, "about-us/documents");

            section.Title = request.Title;
            section.Description = request.Description;
            section.MetadataJson = JsonSerializer.Serialize(metadata);

            await _repository.UpdateSectionAsync(section);
            return Ok(section);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("sections/{sectionName}")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> AddItem(string sectionName, [FromForm] AboutUsItemRequest request)
        {
            var section = await _repository.GetSectionAsync(sectionName);
            if (section == null) return NotFound();

            var item = new AboutUsItem
            {
                SectionId = sectionName,
                Title = request.Title,
                ShortDescription = request.ShortDescription,
                Date = request.Date,
                Designation = request.Designation,
                SocialLinksJson = request.SocialLinksJson,
                OrderIndex = request.OrderIndex
            };

            if (request.Icon != null)
                item.IconUrl = await _cloudinaryService.UploadImageAsync(request.Icon, $"about-us/{sectionName}/icons");
            
            if (request.Image != null)
                item.ImageUrl = await _cloudinaryService.UploadImageAsync(request.Image, $"about-us/{sectionName}/images");

            if (request.Photo != null)
                item.ImageUrl = await _cloudinaryService.UploadImageAsync(request.Photo, $"about-us/team");

            var id = await _repository.AddItemAsync(item);
            item.Id = id;
            return CreatedAtAction(nameof(GetAboutUs), new { id = item.Id }, item);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("sections/{sectionName}/items/{itemId}")]
        public async Task<IActionResult> DeleteItem(string sectionName, Guid itemId)
        {
            var item = await _repository.GetItemByIdAsync(itemId);
            if (item == null || item.SectionId != sectionName) return NotFound();

            await _repository.DeleteItemAsync(itemId);
            return NoContent();
        }

        private AboutUsResponseDto MapToResponseDto(IEnumerable<AboutUsSection> sections, IEnumerable<AboutUsItem> items)
        {
            var sectionsDict = sections.ToDictionary(s => s.Id);
            var itemsBySection = items.GroupBy(i => i.SectionId).ToDictionary(g => g.Key, g => g.ToList());

            var response = new AboutUsResponseDto();

            if (sectionsDict.TryGetValue("about_us", out var s_about))
            {
                var meta = DeserializeMetadata(s_about.MetadataJson);
                response.AboutUs = new AboutUsSectionDto { Title = s_about.Title, Description = s_about.Description, BannerImageUrl = meta.GetValueOrDefault("banner_image_url") };
            }

            if (sectionsDict.TryGetValue("mission", out var s_mission))
            {
                response.Mission = new MissionSectionDto { Title = s_mission.Title, Description = s_mission.Description, MissionList = itemsBySection.GetValueOrDefault("mission")?.Select(i => new MissionItemDto { Id = i.Id, IconUrl = i.IconUrl, ShortDescription = i.ShortDescription }).ToList() ?? new() };
            }

            if (sectionsDict.TryGetValue("vision", out var s_vision))
            {
                response.Vision = new VisionSectionDto { Title = s_vision.Title, Description = s_vision.Description, VisionList = itemsBySection.GetValueOrDefault("vision")?.Select(i => new VisionItemDto { Id = i.Id, IconUrl = i.IconUrl, ShortDescription = i.ShortDescription }).ToList() ?? new() };
            }

            if (sectionsDict.TryGetValue("core_values", out var s_values))
            {
                response.CoreValues = new CoreValuesSectionDto { Title = s_values.Title, Description = s_values.Description, ValuesList = itemsBySection.GetValueOrDefault("core_values")?.Select(i => new CoreValueItemDto { Id = i.Id, Title = i.Title, IconUrl = i.IconUrl, ShortDescription = i.ShortDescription }).ToList() ?? new() };
            }

            if (sectionsDict.TryGetValue("customer_solutions", out var s_solutions))
            {
                response.CustomerSolutions = new CustomerSolutionsSectionDto { Title = s_solutions.Title, Description = s_solutions.Description, SolutionsList = itemsBySection.GetValueOrDefault("customer_solutions")?.Select(i => new CustomerSolutionItemDto { Id = i.Id, IconUrl = i.IconUrl, ShortDescription = i.ShortDescription }).ToList() ?? new() };
            }

            if (sectionsDict.TryGetValue("business_principles", out var s_principles))
            {
                response.BusinessPrinciples = new BusinessPrinciplesSectionDto { Title = s_principles.Title, Description = s_principles.Description, PrinciplesList = itemsBySection.GetValueOrDefault("business_principles")?.Select(i => new BusinessPrincipleItemDto { Id = i.Id, Title = i.Title, IconUrl = i.IconUrl, ShortDescription = i.ShortDescription }).ToList() ?? new() };
            }

            if (sectionsDict.TryGetValue("video", out var s_video))
            {
                var meta = DeserializeMetadata(s_video.MetadataJson);
                response.Video = new VideoSectionDto { Title = s_video.Title, Description = s_video.Description, ThumbnailUrl = meta.GetValueOrDefault("thumbnail_url"), VideoUrl = meta.GetValueOrDefault("video_url") };
            }

            if (sectionsDict.TryGetValue("journey_milestones", out var s_journey))
            {
                response.JourneyMilestones = new JourneyMilestonesSectionDto { Title = s_journey.Title, Description = s_journey.Description, Milestones = itemsBySection.GetValueOrDefault("journey_milestones")?.Select(i => new MilestoneItemDto { Id = i.Id, Date = i.Date, Title = i.Title, Description = i.ShortDescription, ImageUrl = i.ImageUrl }).ToList() ?? new() };
            }

            if (sectionsDict.TryGetValue("team", out var s_team))
            {
                response.Team = new TeamSectionDto { Title = s_team.Title, Description = s_team.Description, Members = itemsBySection.GetValueOrDefault("team")?.Select(i => new TeamMemberDto { Id = i.Id, Name = i.Title, Designation = i.Designation, PhotoUrl = i.ImageUrl, SocialLinks = string.IsNullOrEmpty(i.SocialLinksJson) ? null : JsonSerializer.Deserialize<object>(i.SocialLinksJson) }).ToList() ?? new() };
            }

            if (sectionsDict.TryGetValue("quick_reference", out var s_quick))
            {
                var meta = DeserializeMetadata(s_quick.MetadataJson);
                response.QuickReference = new QuickReferenceSectionDto { Title = s_quick.Title, Description = s_quick.Description, CompanyProfilePdfUrl = meta.GetValueOrDefault("company_profile_pdf_url"), ProductBrochurePdfUrl = meta.GetValueOrDefault("product_brochure_pdf_url") };
            }

            response.UpdatedAt = sections.Max(s => s.UpdatedAt);
            return response;
        }

        private Dictionary<string, string> DeserializeMetadata(string? json)
        {
            if (string.IsNullOrEmpty(json)) return new Dictionary<string, string>();
            try
            {
                return JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new Dictionary<string, string>();
            }
            catch { return new Dictionary<string, string>(); }
        }
    }
}
