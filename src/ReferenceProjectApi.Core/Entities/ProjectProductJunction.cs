using System;
using System.Collections.Generic;

namespace ReferenceProjectApi.Core.Entities;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    
    // We only need the ID and Name for selection/filtering in this context 
    // unless we want to show more info. Mapping to the actual table in DB.
    public virtual ICollection<ProjectProductJunction> ProjectProducts { get; set; } = new List<ProjectProductJunction>();
}

public class ProjectProductJunction
{
    public int ProjectId { get; set; }
    public virtual ReferenceProject Project { get; set; } = null!;

    public int ProductId { get; set; }
    public virtual Product Product { get; set; } = null!;
}
