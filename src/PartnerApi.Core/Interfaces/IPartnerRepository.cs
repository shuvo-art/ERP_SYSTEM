using PartnerApi.Core.Entities;

namespace PartnerApi.Core.Interfaces;

public interface IPartnerRepository
{
    Task<int> CreateAsync(Partner partner);
    Task<bool> UpdateAsync(Partner partner);
    Task<Partner?> GetByIdAsync(int id);
    Task<Partner?> GetBySlugAsync(string slug);
    Task<(IEnumerable<Partner> Data, int Total)> GetAllAsync(string? search, int page, int limit);
    Task<bool> DeleteAsync(int id);
}
