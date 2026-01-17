using Microsoft.AspNetCore.Http;

namespace AboutUsApi.Core.DTOs
{
    public class AboutUsItemRequest
    {
        public string? Title { get; set; }
        public string? ShortDescription { get; set; }
        public string? Date { get; set; }
        public string? Designation { get; set; }
        public string? SocialLinksJson { get; set; }
        public int OrderIndex { get; set; }
        public IFormFile? Icon { get; set; }
        public IFormFile? Image { get; set; }
        public IFormFile? Photo { get; set; }
    }

    public class UpdateSectionRequest
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public IFormFile? BannerImage { get; set; }
        public IFormFile? Thumbnail { get; set; }
        public string? VideoUrl { get; set; }
        public IFormFile? CompanyProfilePdf { get; set; }
        public IFormFile? ProductBrochurePdf { get; set; }
    }
}
