using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ContactApi.Core.Entities;

namespace ContactApi.Core.Interfaces
{
    public interface IDistributorRepository
    {
        Task<Guid> CreateAsync(Distributor distributor);
        Task<IEnumerable<Distributor>> GetAllAsync(bool? isActive);
        Task<Distributor?> GetByIdAsync(Guid id);
        Task<bool> UpdateAsync(Distributor distributor);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> UpdateOrderAsync(IEnumerable<Guid> order);
    }
}
