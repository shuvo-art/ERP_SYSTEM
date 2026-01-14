using System;

namespace AboutUsApi.Core.Entities
{
    public class AboutUsItem
    {
        public Guid Id { get; set; }
        public string SectionId { get; set; } = string.Empty;
        public string? Title { get; set; }
        public string? ShortDescription { get; set; }
        public string? IconUrl { get; set; }
        public string? ImageUrl { get; set; }
        public string? Date { get; set; }
        public string? Designation { get; set; }
        public string? SocialLinksJson { get; set; }
        public int OrderIndex { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
