using ReferenceProjectApi.Core.Entities;

namespace ReferenceProjectApi.Core.Interfaces;

public interface IReferenceProjectRepository
{
    Task<IEnumerable<ReferenceProject>> GetProjectsAsync(int page, int limit, string? status, bool? featured, string? search, int? categoryId);
    Task<int> GetTotalCountAsync(string? status, bool? featured, string? search, int? categoryId);
    Task<ReferenceProject?> GetByIdAsync(int id);
    Task<ReferenceProject?> GetBySlugAsync(string slug);
    Task<int> CreateAsync(ReferenceProject project);
    Task UpdateAsync(ReferenceProject project);
    Task DeleteAsync(int id);
    Task<bool> ProductsExistAsync(List<int> productIds);
}
