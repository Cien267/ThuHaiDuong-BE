using Microsoft.EntityFrameworkCore;
using ThuHaiDuong.Domain.InterfaceRepositories;
using ThuHaiDuong.Infrastructure.DataContext;

namespace ThuHaiDuong.Infrastructure.ImplementRepositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly AppDbContext _context;
 
    public CategoryRepository(AppDbContext context)
    {
        _context = context;
    }
 
    public async Task<bool> SlugExistsAsync(string slug, Guid? excludeId = null)
    {
        var query = _context.Categories
            .Where(c => c.Slug == slug && !c.DeletedAt.HasValue);
 
        if (excludeId.HasValue)
            query = query.Where(c => c.Id != excludeId.Value);
 
        return await query.AnyAsync();
    }
 
    public async Task<bool> HasStoriesAsync(Guid categoryId)
    {
        return await _context.StoryCategories
            .AnyAsync(sc => sc.CategoryId == categoryId && !sc.Story.DeletedAt.HasValue);
    }
 
    public async Task<bool> HasChildrenAsync(Guid categoryId)
    {
        return await _context.Categories
            .AnyAsync(c => c.ParentId == categoryId && !c.DeletedAt.HasValue);
    }
}