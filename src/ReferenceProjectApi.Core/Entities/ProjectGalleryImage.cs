namespace ReferenceProjectApi.Core.Entities;

public class ProjectGalleryImage
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    
    // Navigation property
    public virtual ReferenceProject Project { get; set; } = null!;
}
