using Microsoft.EntityFrameworkCore;
using ThuHaiDuong.Application.Payloads.InputModels.Analytics;
using ThuHaiDuong.Application.Payloads.ResultModels.Analytics;
using ThuHaiDuong.Domain.Entities;
using ThuHaiDuong.Domain.InterfaceRepositories;
using ThuHaiDuong.Infrastructure.DataContext;
using ThuHaiDuong.Shared.Constants;

namespace ThuHaiDuong.Infrastructure.ImplementRepositories;

public class AnalyticsRepository : IAnalyticsRepository
{
    private readonly AppDbContext _context;
 
    public AnalyticsRepository(AppDbContext context)
    {
        _context = context;
    }
 
    // ── TRACK VIEW ────────────────────────────────────────────────────────────
 
    public async Task RecordChapterViewAsync(ChapterView view)
    {
        await _context.ChapterViews.AddAsync(view);
        await _context.SaveChangesAsync();
    }
 
    public async Task IncrementViewCountersAsync(Guid chapterId, Guid storyId)
    {
        // Tăng ViewCount trên Chapter
        await _context.Chapters
            .Where(c => c.Id == chapterId)
            .ExecuteUpdateAsync(c =>
                c.SetProperty(x => x.ViewCount, x => x.ViewCount + 1));
 
        // Tăng TotalViews trên Story
        await _context.Stories
            .Where(s => s.Id == storyId)
            .ExecuteUpdateAsync(s =>
                s.SetProperty(x => x.TotalViews, x => x.TotalViews + 1));
    }
 
    // ── BACKGROUND JOB ────────────────────────────────────────────────────────
 
    public async Task AggregateDailyStatsAsync(DateOnly date)
    {
        var dayStart = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var dayEnd   = date.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);
 
        // Group chapter views theo storyId trong ngày
        var viewStats = await _context.ChapterViews
            .Where(v => v.ViewedAt >= dayStart && v.ViewedAt <= dayEnd)
            .GroupBy(v => v.StoryId)
            .Select(g => new
            {
                StoryId        = g.Key,
                ViewCount      = g.Count(),
                UniqueVisitors = g
                    .Select(v => v.SessionId != null ? v.SessionId : v.IpAddress)
                    .Distinct()
                    .Count(),
            })
            .ToListAsync();
 
        // New bookmarks trong ngày theo story
        var bookmarkStats = await _context.Bookmarks
            .Where(b => b.CreatedAt >= dayStart && b.CreatedAt <= dayEnd && !b.IsDeleted)
            .GroupBy(b => b.StoryId)
            .Select(g => new { StoryId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.StoryId, x => x.Count);
 
        // New ratings trong ngày theo story
        var ratingStats = await _context.Ratings
            .Where(r => r.CreatedAt >= dayStart && r.CreatedAt <= dayEnd && !r.IsDeleted)
            .GroupBy(r => r.StoryId)
            .Select(g => new { StoryId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.StoryId, x => x.Count);
 
        // Upsert từng story vào DailyStoryStats
        foreach (var stat in viewStats)
        {
            var existing = await _context.DailyStoryStats
                .FirstOrDefaultAsync(d =>
                    d.StoryId == stat.StoryId && d.StatDate == date);
 
            if (existing != null)
            {
                existing.ViewCount      = stat.ViewCount;
                existing.UniqueVisitors = stat.UniqueVisitors;
                existing.NewBookmarks   = bookmarkStats.GetValueOrDefault(stat.StoryId, 0);
                existing.NewRatings     = ratingStats.GetValueOrDefault(stat.StoryId, 0);
                _context.DailyStoryStats.Update(existing);
            }
            else
            {
                await _context.DailyStoryStats.AddAsync(new DailyStoryStat
                {
                    StoryId        = stat.StoryId,
                    StatDate       = date,
                    ViewCount      = stat.ViewCount,
                    UniqueVisitors = stat.UniqueVisitors,
                    NewBookmarks   = bookmarkStats.GetValueOrDefault(stat.StoryId, 0),
                    NewRatings     = ratingStats.GetValueOrDefault(stat.StoryId, 0),
                });
            }
        }
 
        await _context.SaveChangesAsync();
    }
}