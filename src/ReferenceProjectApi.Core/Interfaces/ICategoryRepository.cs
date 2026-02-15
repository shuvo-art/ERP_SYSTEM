using ReferenceProjectApi.Core.Entities;

namespace ReferenceProjectApi.Core.Interfaces;

public interface ICategoryRepository
{
    Task<IEnumerable<ProjectCategory>> GetAllAsync();
    Task<ProjectCategory?> GetByIdAsync(int id);
    Task<int> CreateAsync(ProjectCategory category);
    Task UpdateAsync(ProjectCategory category);
    Task DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
}
