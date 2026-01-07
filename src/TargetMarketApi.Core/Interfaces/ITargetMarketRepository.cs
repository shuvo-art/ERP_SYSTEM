using TargetMarketApi.Core.Entities;

namespace TargetMarketApi.Core.Interfaces;

public interface ITargetMarketRepository
{
    Task<int> CreateAsync(TargetMarket market);
    Task<bool> UpdateAsync(TargetMarket market);
    Task<TargetMarket?> GetByIdAsync(int id);
    Task<(IEnumerable<TargetMarket> Data, int Total)> GetAllAsync(string? search, int page, int limit);
    Task<bool> DeleteAsync(int id);
}
