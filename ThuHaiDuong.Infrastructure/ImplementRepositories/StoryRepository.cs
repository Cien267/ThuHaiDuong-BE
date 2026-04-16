using Microsoft.EntityFrameworkCore;
using ThuHaiDuong.Application.Payloads.ResultModels.User.Category;
using ThuHaiDuong.Application.Payloads.ResultModels.User.Chapter;
using ThuHaiDuong.Application.Payloads.ResultModels.User.Story;
using ThuHaiDuong.Application.Payloads.ResultModels.User.Tag;
using ThuHaiDuong.Domain.Entities;
using ThuHaiDuong.Domain.InterfaceRepositories;
using ThuHaiDuong.Infrastructure.DataContext;
using ThuHaiDuong.Shared.Constants;

namespace ThuHaiDuong.Infrastructure.ImplementRepositories;

public class StoryRepository : IStoryRepository
{
    private readonly AppDbContext _context;
 
    public StoryRepository(AppDbContext context)
    {
        _context = context;
    }
 
    public async Task<bool> SlugExistsAsync(string slug, Guid? excludeId = null)
    {
        var query = _context.Stories
            .Where(s => s.Slug == slug && !s.DeletedAt.HasValue);
 
        if (excludeId.HasValue)
            query = query.Where(s => s.Id != excludeId.Value);
 
        return await query.AnyAsync();
    }
 
    public async Task<Story?> GetDetailBySlugAsync(string slug)
    {
        var story = await _context.Stories
            .Include(s => s.Author)
            .Include(s => s.StoryCategories)
                .ThenInclude(sc => sc.Category)
            .Include(s => s.StoryTags)
                .ThenInclude(st => st.Tag)
            .Include(s => s.Chapters
                .Where(c => c.Status == "Published" && !c.DeletedAt.HasValue)
                .OrderBy(c => c.ChapterNumber))
            .FirstOrDefaultAsync(s =>
                s.Slug == slug &&
                !s.DeletedAt.HasValue &&
                (s.Status == StoryStatus.Publishing || s.Status == StoryStatus.Completed));
 
        if (story == null) return null;

        return story;
    }
 
    public async Task SyncCategoriesAsync(Guid storyId, List<Guid> categoryIds)
    {
        // Xóa toàn bộ mapping cũ
        var existing = await _context.StoryCategories
            .Where(sc => sc.StoryId == storyId)
            .ToListAsync();
 
        _context.StoryCategories.RemoveRange(existing);
 
        // Thêm mapping mới
        if (categoryIds.Count > 0)
        {
            var newMappings = categoryIds
                .Distinct()
                .Select(cid => new StoryCategory
                {
                    StoryId    = storyId,
                    CategoryId = cid,
                });
 
            await _context.StoryCategories.AddRangeAsync(newMappings);
        }
 
        await _context.SaveChangesAsync();
    }
 
    public async Task SyncTagsAsync(Guid storyId, List<Guid> tagIds)
    {
        var existing = await _context.StoryTags
            .Where(st => st.StoryId == storyId)
            .ToListAsync();
 
        _context.StoryTags.RemoveRange(existing);
 
        if (tagIds.Count > 0)
        {
            var newMappings = tagIds
                .Distinct()
                .Select(tid => new StoryTag
                {
                    StoryId = storyId,
                    TagId   = tid,
                });
 
            await _context.StoryTags.AddRangeAsync(newMappings);
        }
 
        await _context.SaveChangesAsync();
    }
 
    public async Task UpdateTotalChaptersAsync(Guid storyId)
    {
        var count = await _context.Chapters
            .CountAsync(c =>
                c.StoryId == storyId &&
                c.Status == "Published" &&
                !c.DeletedAt.HasValue);
 
        await _context.Stories
            .Where(s => s.Id == storyId)
            .ExecuteUpdateAsync(s =>
                s.SetProperty(x => x.TotalChapters, count));
    }
}