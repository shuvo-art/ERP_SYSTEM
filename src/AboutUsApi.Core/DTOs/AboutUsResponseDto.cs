using System;
using System.Collections.Generic;

namespace AboutUsApi.Core.DTOs
{
    public class AboutUsResponseDto
    {
        public string Id { get; set; } = "about-us-singleton";
        public DateTime UpdatedAt { get; set; }
        public AboutUsSectionDto AboutUs { get; set; } = new();
        public MissionSectionDto Mission { get; set; } = new();
        public VisionSectionDto Vision { get; set; } = new();
        public CoreValuesSectionDto CoreValues { get; set; } = new();
        public CustomerSolutionsSectionDto CustomerSolutions { get; set; } = new();
        public BusinessPrinciplesSectionDto BusinessPrinciples { get; set; } = new();
        public VideoSectionDto Video { get; set; } = new();
        public JourneyMilestonesSectionDto JourneyMilestones { get; set; } = new();
        public TeamSectionDto Team { get; set; } = new();
        public QuickReferenceSectionDto QuickReference { get; set; } = new();
    }

    public class AboutUsSectionDto
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? BannerImageUrl { get; set; }
    }

    public class MissionSectionDto
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public List<MissionItemDto> MissionList { get; set; } = new();
    }

    public class MissionItemDto
    {
        public Guid Id { get; set; }
        public string? IconUrl { get; set; }
        public string? ShortDescription { get; set; }
    }

    public class VisionSectionDto
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public List<VisionItemDto> VisionList { get; set; } = new();
    }

    public class VisionItemDto
    {
        public Guid Id { get; set; }
        public string? IconUrl { get; set; }
        public string? ShortDescription { get; set; }
    }

    public class CoreValuesSectionDto
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public List<CoreValueItemDto> ValuesList { get; set; } = new();
    }

    public class CoreValueItemDto
    {
        public Guid Id { get; set; }
        public string? Title { get; set; }
        public string? IconUrl { get; set; }
        public string? ShortDescription { get; set; }
    }

    public class CustomerSolutionsSectionDto
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public List<CustomerSolutionItemDto> SolutionsList { get; set; } = new();
    }

    public class CustomerSolutionItemDto
    {
        public Guid Id { get; set; }
        public string? IconUrl { get; set; }
        public string? ShortDescription { get; set; }
    }

    public class BusinessPrinciplesSectionDto
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public List<BusinessPrincipleItemDto> PrinciplesList { get; set; } = new();
    }

    public class BusinessPrincipleItemDto
    {
        public Guid Id { get; set; }
        public string? Title { get; set; }
        public string? IconUrl { get; set; }
        public string? ShortDescription { get; set; }
    }

    public class VideoSectionDto
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ThumbnailUrl { get; set; }
        public string? VideoUrl { get; set; }
    }

    public class JourneyMilestonesSectionDto
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public List<MilestoneItemDto> Milestones { get; set; } = new();
    }

    public class MilestoneItemDto
    {
        public Guid Id { get; set; }
        public string? Date { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
    }

    public class TeamSectionDto
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public List<TeamMemberDto> Members { get; set; } = new();
    }

    public class TeamMemberDto
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Designation { get; set; }
        public string? PhotoUrl { get; set; }
        public object? SocialLinks { get; set; }
    }

    public class QuickReferenceSectionDto
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? CompanyProfilePdfUrl { get; set; }
        public string? ProductBrochurePdfUrl { get; set; }
    }
}
