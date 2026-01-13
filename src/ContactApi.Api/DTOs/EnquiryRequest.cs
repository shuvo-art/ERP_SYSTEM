using System.ComponentModel.DataAnnotations;

namespace ContactApi.Api.DTOs
{
    public class EnquiryRequest
    {
        [Required]
        public string Type { get; set; } = string.Empty;
        
        [Required]
        public string Name { get; set; } = string.Empty;
        
        public string? Designation { get; set; }
        
        [Required]
        public string Mobile { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        public string? Address { get; set; }
        public string? Country { get; set; }
        public string? CompanyName { get; set; }
        public string? ProductId { get; set; }
        
        [Required]
        public string Message { get; set; } = string.Empty;
        
        public bool RequestCallBack { get; set; }
        public bool AgreeDataProtection { get; set; }
    }

    public class EnquiryUpdateStatusRequest
    {
        [Required]
        public string Status { get; set; } = string.Empty;
        public string? AdminNotes { get; set; }
    }
}
