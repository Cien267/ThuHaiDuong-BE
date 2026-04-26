using Microsoft.EntityFrameworkCore;
using ThuHaiDuong.Application.Payloads.ResultModels.User.Chapter;
using ThuHaiDuong.Domain.Entities;
using ThuHaiDuong.Domain.InterfaceRepositories;
using ThuHaiDuong.Infrastructure.DataContext;
using ThuHaiDuong.Shared.Constants;

namespace ThuHaiDuong.Infrastructure.ImplementRepositories;

public class ChapterRepository : IChapterRepository
{
    private readonly AppDbContext _context;
 
    public ChapterRepository(AppDbContext context)
    {
        _context = context;
    }
 
    public async Task<int> GetMaxChapterNumberAsync(Guid storyId)
    {
        var max = await _context.Chapters
            .Where(c => c.StoryId == storyId && !c.IsDeleted)
            .MaxAsync(c => (int?)c.ChapterNumber);
 
        return max ?? 0;
    }
 
    public async Task<bool> ChapterNumberExistsAsync(
        Guid storyId, int chapterNumber, Guid? excludeId = null)
    {
        var query = _context.Chapters
            .Where(c => c.StoryId == storyId
                        && c.ChapterNumber == chapterNumber
                        && !c.IsDeleted);
 
        if (excludeId.HasValue)
            query = query.Where(c => c.Id != excludeId.Value);
 
        return await query.AnyAsync();
    }
 
    public async Task<Chapter?> GetChapterForReadingAsync(Guid chapterId)
    {
        var chapter = await _context.Chapters
            .Include(c => c.Story)
            .FirstOrDefaultAsync(c =>
                c.Id == chapterId &&
                c.Status == ChapterStatus.Published &&
                !c.IsDeleted &&
                !c.Story.IsDeleted &&
                (c.Story.Status == StoryStatus.Publishing ||
                 c.Story.Status == StoryStatus.Completed));
 
        if (chapter == null) return null;
        return chapter;
    }
 
    public async Task<Chapter?> GetChapterForReadingByNumberAsync(
        Guid storyId, int chapterNumber)
    {
        var chapter = await _context.Chapters
            .Include(c => c.Story)
            .FirstOrDefaultAsync(c =>
                c.StoryId == storyId &&
                c.ChapterNumber == chapterNumber &&
                c.Status == ChapterStatus.Published &&
                !c.IsDeleted &&
                !c.Story.IsDeleted);
 
        if (chapter == null) return null;
        return chapter;
    }
 
    // Cập nhật TotalChapters và LastChapterAt trực tiếp bằng ExecuteUpdateAsync
    // Không load entity vào memory → hiệu quả hơn
    public async Task SyncStoryCountersAsync(Guid storyId)
    {
        var publishedChapters = await _context.Chapters
            .Where(c => c.StoryId == storyId
                        && c.Status == ChapterStatus.Published
                        && !c.IsDeleted)
            .ToListAsync();
 
        var totalChapters  = publishedChapters.Count;
        var lastChapterAt  = publishedChapters.Count > 0
            ? publishedChapters.Max(c => c.PublishedAt)
            : (DateTime?)null;
 
        await _context.Stories
            .Where(s => s.Id == storyId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(x => x.TotalChapters, totalChapters)
                .SetProperty(x => x.LastChapterAt, lastChapterAt)
                .SetProperty(x => x.UpdatedAt, DateTime.UtcNow));
    }
 
    
    public async Task<Chapter?> GetPrevChapterAsync(Chapter chapter)
    {
        return await _context.Chapters
            .Where(c => c.StoryId == chapter.StoryId
                        && c.ChapterNumber < chapter.ChapterNumber
                        && c.Status == ChapterStatus.Published
                        && !c.IsDeleted)
            .OrderByDescending(c => c.ChapterNumber)
            .FirstOrDefaultAsync();
    }
    
    public async Task<Chapter?> GetNextChapterAsync(Chapter chapter)
    {
        return await _context.Chapters
            .Where(c => c.StoryId == chapter.StoryId
                        && c.ChapterNumber > chapter.ChapterNumber
                        && c.Status == ChapterStatus.Published
                        && !c.IsDeleted)
            .OrderBy(c => c.ChapterNumber)
            .FirstOrDefaultAsync();
    }
}