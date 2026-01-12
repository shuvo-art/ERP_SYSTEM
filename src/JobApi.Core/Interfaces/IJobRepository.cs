using JobApi.Core.Entities;

namespace JobApi.Core.Interfaces;

public interface IJobRepository
{
    Task<IEnumerable<JobPosting>> GetAllAsync(string? status, string? department, string? location);
    Task<JobPosting?> GetByIdAsync(Guid id);
    Task<JobPosting?> GetBySlugAsync(string slug);
    Task<Guid> CreateAsync(JobPosting job);
    Task UpdateAsync(JobPosting job);
    Task DeleteAsync(Guid id);
}
