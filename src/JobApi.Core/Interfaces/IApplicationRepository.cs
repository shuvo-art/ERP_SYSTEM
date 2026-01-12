using JobApi.Core.Entities;

namespace JobApi.Core.Interfaces;

public interface IApplicationRepository
{
    Task<IEnumerable<JobApplication>> GetAllAsync(Guid? jobId, string? status, string? search);
    Task<int> GetTotalCountAsync(Guid? jobId, string? status, string? search);
    Task<JobApplication?> GetByIdAsync(Guid id);
    Task<Guid> CreateAsync(JobApplication application);
    Task UpdateStatusAsync(Guid id, string status, string? notes);
    Task DeleteAsync(Guid id);
}
