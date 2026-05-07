using Microsoft.EntityFrameworkCore;
using ThuHaiDuong.Domain.Entities;
using ThuHaiDuong.Domain.InterfaceRepositories;
using ThuHaiDuong.Infrastructure.DataContext;

namespace ThuHaiDuong.Infrastructure.ImplementRepositories;

public class TagRepository : ITagRepository
{
    private readonly AppDbContext _context;
 
    public TagRepository(AppDbContext context)
    {
        _context = context;
    }
 
    public async Task<bool> SlugExistsAsync(string slug, Guid? excludeId = null)
    {
        var query = _context.Tags
            .Where(t => t.Slug == slug && !t.DeletedAt.HasValue);
 
        if (excludeId.HasValue)
            query = query.Where(t => t.Id != excludeId.Value);
 
        return await query.AnyAsync();
    }
 
    public async Task<List<Tag>> GetByIdsAsync(IEnumerable<Guid> ids)
    {
        return await _context.Tags
            .Where(t => ids.Contains(t.Id) && !t.DeletedAt.HasValue)
            .ToListAsync();
    }
 
    public async Task<bool> HasStoriesAsync(Guid tagId)
    {
        return await _context.StoryTags
            .AnyAsync(st => st.TagId == tagId && !st.Story.DeletedAt.HasValue);
    }
}