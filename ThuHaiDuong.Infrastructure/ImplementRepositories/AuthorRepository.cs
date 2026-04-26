using Microsoft.EntityFrameworkCore;
using ThuHaiDuong.Domain.InterfaceRepositories;
using ThuHaiDuong.Infrastructure.DataContext;

namespace ThuHaiDuong.Infrastructure.ImplementRepositories;

public class AuthorRepository : IAuthorRepository
{
    private readonly AppDbContext _context;
 
    public AuthorRepository(AppDbContext context)
    {
        _context = context;
    }
 
    public async Task<bool> SlugExistsAsync(string slug, Guid? excludeId = null)
    {
        var query = _context.Authors
            .Where(a => a.Slug == slug && !a.IsDeleted);
 
        if (excludeId.HasValue)
            query = query.Where(a => a.Id != excludeId.Value);
 
        return await query.AnyAsync();
    }
 
    public async Task<bool> HasStoriesAsync(Guid authorId)
    {
        return await _context.Stories
            .AnyAsync(s => s.AuthorId == authorId && !s.IsDeleted);
    }
}