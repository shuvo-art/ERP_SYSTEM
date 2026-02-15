using Microsoft.EntityFrameworkCore;
using ReferenceProjectApi.Core.Entities;
using ReferenceProjectApi.Core.Interfaces;
using ReferenceProjectApi.Infrastructure.Data;

namespace ReferenceProjectApi.Infrastructure.Repositories;

public class ReferenceProjectRepository : IReferenceProjectRepository
{
    private readonly ReferenceProjectDbContext _context;

    public ReferenceProjectRepository(ReferenceProjectDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ReferenceProject>> GetProjectsAsync(int page, int limit, string? status, bool? featured, string? search, int? categoryId)
    {
        var query = _context.ReferenceProjects
            .Include(p => p.Category)
            .Include(p => p.GalleryImages)
            .Include(p => p.DetailImages)
            .Include(p => p.ProjectProducts)
                .ThenInclude(pp => pp.Product)
            .AsQueryable();

        if (!string.IsNullOrEmpty(status))
            query = query.Where(p => p.Status == status);

        if (featured.HasValue)
            query = query.Where(p => p.Featured == featured.Value);

        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId.Value);

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(p => 
                p.ProjectName.Contains(search) || 
                p.ShortDescription.Contains(search) || 
                p.Location.Contains(search));
        }

        return await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<int> GetTotalCountAsync(string? status, bool? featured, string? search, int? categoryId)
    {
        var query = _context.ReferenceProjects.AsQueryable();

        if (!string.IsNullOrEmpty(status))
            query = query.Where(p => p.Status == status);

        if (featured.HasValue)
            query = query.Where(p => p.Featured == featured.Value);

        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId.Value);

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(p => 
                p.ProjectName.Contains(search) || 
                p.ShortDescription.Contains(search));
        }

        return await query.CountAsync();
    }

    public async Task<ReferenceProject?> GetByIdAsync(int id)
    {
        return await _context.ReferenceProjects
            .Include(p => p.Category)
            .Include(p => p.GalleryImages)
            .Include(p => p.DetailImages)
            .Include(p => p.ProjectProducts)
                .ThenInclude(pp => pp.Product)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<ReferenceProject?> GetBySlugAsync(string slug)
    {
        return await _context.ReferenceProjects
            .Include(p => p.Category)
            .Include(p => p.GalleryImages)
            .Include(p => p.DetailImages)
            .Include(p => p.ProjectProducts)
                .ThenInclude(pp => pp.Product)
            .FirstOrDefaultAsync(p => p.Slug == slug);
    }

    public async Task<int> CreateAsync(ReferenceProject project)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            _context.ReferenceProjects.Add(project);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return project.Id;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task UpdateAsync(ReferenceProject project)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Update basic project info
            _context.Entry(project).State = EntityState.Modified;
            
            // For complex many-to-many and collections, usually we'd handle them specifically
            // but for simplicity here we assume the passed project object is correctly tracked or we use standard update.
            // In a real API, we often clear and re-add junction records if they change.
            
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task DeleteAsync(int id)
    {
        var project = await _context.ReferenceProjects.FindAsync(id);
        if (project != null)
        {
            _context.ReferenceProjects.Remove(project);
            await _context.SaveChangesAsync();
        }
    }

    // Helper method to check products existence
    public async Task<bool> ProductsExistAsync(List<int> productIds)
    {
        var count = await _context.Products.CountAsync(p => productIds.Contains(p.Id));
        return count == productIds.Distinct().Count();
    }
}
