using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ContactApi.Core.Entities;

namespace ContactApi.Core.Interfaces
{
    public interface IEnquiryRepository
    {
        Task<Guid> CreateAsync(Enquiry enquiry);
        Task<IEnumerable<Enquiry>> GetAllAsync(string? type, string? status, string? search, DateTime? dateFrom, DateTime? dateTo);
        Task<Enquiry?> GetByIdAsync(Guid id);
        Task<bool> UpdateStatusAsync(Guid id, string status, string? adminNotes);
    }
}
