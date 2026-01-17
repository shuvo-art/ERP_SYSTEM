using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace ContactApi.Core.DTOs
{
    public class DistributorRequest
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        public string Address { get; set; } = string.Empty;
        
        [Required]
        public string Phone { get; set; } = string.Empty;
        
        [Required]
        public string Country { get; set; } = string.Empty;
        
        [EmailAddress]
        public string? Email { get; set; }
        
        public string? Website { get; set; }
        public bool IsActive { get; set; } = true;
        public int DisplayOrder { get; set; }
    }

    public class DistributorReorderRequest
    {
        [Required]
        public List<Guid> Order { get; set; } = new();
    }
}
