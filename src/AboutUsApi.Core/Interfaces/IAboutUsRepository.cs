using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AboutUsApi.Core.Entities;

namespace AboutUsApi.Core.Interfaces
{
    public interface IAboutUsRepository
    {
        Task<(IEnumerable<AboutUsSection> Sections, IEnumerable<AboutUsItem> Items)> GetFullAboutUsAsync();
        Task<AboutUsSection?> GetSectionAsync(string sectionId);
        Task<bool> UpdateSectionAsync(AboutUsSection section);
        Task<Guid> AddItemAsync(AboutUsItem item);
        Task<bool> UpdateItemAsync(AboutUsItem item);
        Task<bool> DeleteItemAsync(Guid itemId);
        Task<AboutUsItem?> GetItemByIdAsync(Guid itemId);
    }
}
