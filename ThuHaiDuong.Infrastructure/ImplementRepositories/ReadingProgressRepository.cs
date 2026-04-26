using Microsoft.EntityFrameworkCore;
using ThuHaiDuong.Application.Payloads.InputModels.ReadingProgress;
using ThuHaiDuong.Application.Payloads.ResultModels.ReadingProgress;
using ThuHaiDuong.Domain.Entities;
using ThuHaiDuong.Domain.InterfaceRepositories;
using ThuHaiDuong.Infrastructure.DataContext;

namespace ThuHaiDuong.Infrastructure.ImplementRepositories;

public class ReadingProgressRepository : IReadingProgressRepository
{
    private readonly AppDbContext _context;
 
    public ReadingProgressRepository(AppDbContext context)
    {
        _context = context;
    }
 
    public async Task<UserReadingProgress?> GetProgressAsync(Guid userId, Guid storyId)
    {
        return await _context.UserReadingProgresses
            .FirstOrDefaultAsync(p =>
                p.UserId == userId &&
                p.StoryId == storyId &&
                !p.IsDeleted);
    }
 
    public async Task<ReadingProgressResult?> GetProgressDetailAsync(
        Guid userId, Guid storyId)
    {
        var progress = await _context.UserReadingProgresses
            .Include(p => p.Story)
            .Include(p => p.LastChapter)
            .FirstOrDefaultAsync(p =>
                p.UserId == userId &&
                p.StoryId == storyId &&
                !p.IsDeleted);
 
        if (progress == null) return null;
 
        // Đếm số chapter đã đọc trong story này
        var readCount = await _context.ReadingHistories
            .CountAsync(h =>
                h.UserId == userId &&
                h.StoryId == storyId &&
                !h.IsDeleted);
 
        // Tổng chapter published (denormalized — đọc trực tiếp, không COUNT)
        var totalPublished = progress.Story.TotalChapters;
 
        return new ReadingProgressResult
        {
            StoryId                = storyId,
            StoryTitle             = progress.Story.Title,
            StorySlug              = progress.Story.Slug,
            TotalPublishedChapters = totalPublished,
            LastChapterId          = progress.LastChapterId,
            LastChapterNumber      = progress.LastChapterNumber,
            LastChapterTitle       = progress.LastChapter.Title,
            LastReadAt             = progress.LastReadAt,
            ReadChapterCount       = readCount,
        };
    }
 
    public async Task UpsertProgressAsync(
        Guid userId, Guid storyId, Guid chapterId, int chapterNumber)
    {
        var existing = await GetProgressAsync(userId, storyId);
 
        if (existing != null)
        {
            // Chỉ update nếu chapter mới hơn chapter hiện tại
            // Tránh trường hợp user quay lại đọc chương cũ làm mất progress
            if (chapterNumber > existing.LastChapterNumber)
            {
                existing.LastChapterId     = chapterId;
                existing.LastChapterNumber = chapterNumber;
                existing.LastReadAt        = DateTime.UtcNow;
                _context.UserReadingProgresses.Update(existing);
                await _context.SaveChangesAsync();
            }
        }
        else
        {
            var progress = new UserReadingProgress
            {
                UserId            = userId,
                StoryId           = storyId,
                LastChapterId     = chapterId,
                LastChapterNumber = chapterNumber,
                LastReadAt        = DateTime.UtcNow,
            };
            await _context.UserReadingProgresses.AddAsync(progress);
            await _context.SaveChangesAsync();
        }
    }
 
    public async Task UpsertHistoryAsync(Guid userId, Guid storyId, Guid chapterId)
    {
        var existing = await _context.ReadingHistories
            .FirstOrDefaultAsync(h =>
                h.UserId == userId &&
                h.ChapterId == chapterId &&
                !h.IsDeleted);
 
        if (existing != null)
        {
            // Đọc lại chapter cũ → chỉ update ReadAt
            existing.ReadAt = DateTime.UtcNow;
            _context.ReadingHistories.Update(existing);
        }
        else
        {
            await _context.ReadingHistories.AddAsync(new ReadingHistory
            {
                UserId    = userId,
                ChapterId = chapterId,
                StoryId   = storyId,
                ReadAt    = DateTime.UtcNow,
            });
        }
 
        await _context.SaveChangesAsync();
    }
 
 
    public async Task<HashSet<Guid>> GetReadChapterIdsAsync(Guid userId, Guid storyId)
    {
        var ids = await _context.ReadingHistories
            .Where(h =>
                h.UserId == userId &&
                h.StoryId == storyId &&
                !h.IsDeleted)
            .Select(h => h.ChapterId)
            .ToListAsync();
 
        return [.. ids];
    }
}