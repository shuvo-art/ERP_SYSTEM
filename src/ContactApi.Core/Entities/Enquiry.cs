using System;

namespace ContactApi.Core.Entities
{
    public class Enquiry
    {
        public Guid Id { get; set; }
        public string Type { get; set; } = string.Empty; // product_enquiry | partnership | dealership | technical_meeting | general
        public string Name { get; set; } = string.Empty;
        public string? Designation { get; set; }
        public string Mobile { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? Country { get; set; }
        public string? CompanyName { get; set; }
        public string? ProductId { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool RequestCallBack { get; set; }
        public bool AgreeDataProtection { get; set; }
        public string Status { get; set; } = "new"; // new | responded | closed
        public string? AdminNotes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
