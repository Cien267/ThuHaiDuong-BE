using Microsoft.EntityFrameworkCore;
using ThuHaiDuong.Application.Payloads.ResultModels.Affiliate;
using ThuHaiDuong.Domain.Entities;
using ThuHaiDuong.Domain.InterfaceRepositories;
using ThuHaiDuong.Infrastructure.DataContext;

namespace ThuHaiDuong.Infrastructure.ImplementRepositories;

public class AffiliateRepository : IAffiliateRepository
{
    private readonly AppDbContext _context;
 
    public AffiliateRepository(AppDbContext context)
    {
        _context = context;
    }
 
    public async Task<bool> TrackingCodeExistsAsync(string code, Guid? excludeId = null)
    {
        var query = _context.AffiliateLinks
            .Where(l => l.TrackingCode == code && !l.DeletedAt.HasValue);
 
        if (excludeId.HasValue)
            query = query.Where(l => l.Id != excludeId.Value);
 
        return await query.AnyAsync();
    }
 
    public async Task<AffiliateLink?> GetByTrackingCodeAsync(string trackingCode)
    {
        // Hot path — chỉ cần IsActive, không cần include navigation
        return await _context.AffiliateLinks
            .FirstOrDefaultAsync(l =>
                l.TrackingCode == trackingCode &&
                l.IsActive &&
                !l.DeletedAt.HasValue &&
                (l.StartDate == null || l.StartDate <= DateTime.UtcNow) &&
                (l.EndDate == null   || l.EndDate   >= DateTime.UtcNow));
    }
 
    public async Task<bool> IsSpamClickAsync(Guid linkId, string ipAddress)
    {
        var threshold = DateTime.UtcNow.AddHours(-1);
 
        return await _context.AffiliateClicks
            .AnyAsync(c =>
                c.AffiliateLinkId == linkId &&
                c.IpAddress == ipAddress &&
                c.ClickedAt >= threshold);
    }
 
    public async Task SyncStoryTargetsAsync(Guid linkId, List<Guid> storyIds)
    {
        var existing = await _context.AffiliateLinkStories
            .Where(s => s.AffiliateLinkId == linkId)
            .ToListAsync();
 
        _context.AffiliateLinkStories.RemoveRange(existing);
 
        if (storyIds.Count > 0)
        {
            var newItems = storyIds.Distinct().Select(sid => new AffiliateLinkStory
            {
                AffiliateLinkId = linkId,
                StoryId         = sid,
            });
            await _context.AffiliateLinkStories.AddRangeAsync(newItems);
        }
 
        await _context.SaveChangesAsync();
    }
 
    public async Task SyncChapterTargetsAsync(Guid linkId, List<Guid> chapterIds)
    {
        var existing = await _context.AffiliateLinkChapters
            .Where(c => c.AffiliateLinkId == linkId)
            .ToListAsync();
 
        _context.AffiliateLinkChapters.RemoveRange(existing);
 
        if (chapterIds.Count > 0)
        {
            var newItems = chapterIds.Distinct().Select(cid => new AffiliateLinkChapter
            {
                AffiliateLinkId = linkId,
                ChapterId       = cid,
            });
            await _context.AffiliateLinkChapters.AddRangeAsync(newItems);
        }
 
        await _context.SaveChangesAsync();
    }
 
    public async Task<List<AffiliateClick>> GetDailyStatsAsync(
        Guid? linkId, DateTime from, DateTime to)
    {
        var query = _context.AffiliateClicks
            .Where(c => c.ClickedAt >= from && c.ClickedAt <= to);
 
        if (linkId.HasValue)
            query = query.Where(c => c.AffiliateLinkId == linkId.Value);
 
        // Group theo ngày
        var raw = await query
            
            .ToListAsync();
        return raw;
    }
}