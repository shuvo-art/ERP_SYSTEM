using Microsoft.EntityFrameworkCore;
using ReferenceProjectApi.Core.Entities;
using ReferenceProjectApi.Core.Interfaces;
using ReferenceProjectApi.Infrastructure.Data;

namespace ReferenceProjectApi.Infrastructure.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly ReferenceProjectDbContext _context;

    public CategoryRepository(ReferenceProjectDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ProjectCategory>> GetAllAsync()
    {
        return await _context.ProjectCategories.ToListAsync();
    }

    public async Task<ProjectCategory?> GetByIdAsync(int id)
    {
        return await _context.ProjectCategories.FindAsync(id);
    }

    public async Task<int> CreateAsync(ProjectCategory category)
    {
        _context.ProjectCategories.Add(category);
        await _context.SaveChangesAsync();
        return category.Id;
    }

    public async Task UpdateAsync(ProjectCategory category)
    {
        _context.ProjectCategories.Update(category);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var category = await _context.ProjectCategories.FindAsync(id);
        if (category != null)
        {
            _context.ProjectCategories.Remove(category);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.ProjectCategories.AnyAsync(c => c.Id == id);
    }
}
