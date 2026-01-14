using System;

namespace AboutUsApi.Core.Entities
{
    public class AboutUsSection
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? MetadataJson { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
