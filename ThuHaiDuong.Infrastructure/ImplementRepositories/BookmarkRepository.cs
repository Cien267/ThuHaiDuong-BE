using Microsoft.EntityFrameworkCore;
using ThuHaiDuong.Application.Payloads.ResultModels.Bookmark;
using ThuHaiDuong.Application.Payloads.ResultModels.User.Bookmark;
using ThuHaiDuong.Domain.Entities;
using ThuHaiDuong.Domain.InterfaceRepositories;
using ThuHaiDuong.Infrastructure.DataContext;

namespace ThuHaiDuong.Infrastructure.ImplementRepositories;

public class BookmarkRepository : IBookmarkRepository
{
    private readonly AppDbContext _context;
 
    public BookmarkRepository(AppDbContext context)
    {
        _context = context;
    }
 
    public async Task<Bookmark?> GetAsync(Guid userId, Guid storyId)
    {
        return await _context.Bookmarks
            .FirstOrDefaultAsync(b =>
                b.UserId == userId &&
                b.StoryId == storyId &&
                !b.DeletedAt.HasValue);
    }
 
    public async Task<bool> ExistsAsync(Guid userId, Guid storyId)
    {
        return await _context.Bookmarks
            .AnyAsync(b =>
                b.UserId == userId &&
                b.StoryId == storyId &&
                !b.DeletedAt.HasValue);
    }

    public async Task<List<Bookmark>> GetUserBookmarksByIdAsync(Guid userId)
    {
        return await _context.Bookmarks
            .Where(b => b.UserId == userId && !b.DeletedAt.HasValue && !b.Story.DeletedAt.HasValue)
            .Include(b => b.Story)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();
        
    }
    
    public async Task<Dictionary<Guid, UserReadingProgress>> GetUserReadingProgressByStoryIdsAsync(Guid userId, List<Guid> storyIds)
    {
        return await _context.UserReadingProgresses
            .Where(p => p.UserId == userId && storyIds.Contains(p.StoryId))
            .Include(p => p.LastChapter)
            .ToDictionaryAsync(p => p.StoryId);
        
    }
}