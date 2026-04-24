using ThuHaiDuong.Application.InterfaceService;
using ThuHaiDuong.Application.Payloads.InputModels.Analytics;
using ThuHaiDuong.Application.Payloads.ResultModels.Analytics;
using ThuHaiDuong.Shared.Constants;

namespace ThuHaiDuong.Application.ImplementService;

public class CachedAnalyticsService : IAnalyticsService
{
    private readonly IAnalyticsService _inner;
    private readonly ICacheService     _cache;
 
    public CachedAnalyticsService(IAnalyticsService inner, ICacheService cache)
    {
        _inner = inner;
        _cache = cache;
    }
 
    // ── CLIENT — hot path, có cache ────────────────────────────────────────────
 
    public async Task<List<StoryRankingItem>> GetTopStoriesAsync(StoryRankingQuery query)
    {
        var key    = CacheKeys.AnalyticsTopStories(query.Period, query.PageNumber, query.PageSize);
        var cached = await _cache.GetAsync<List<StoryRankingItem>>(key);
        if (cached != null) return cached;
 
        var result = await _inner.GetTopStoriesAsync(query);
        await _cache.SetAsync(key, result, CacheTTL.TopStories);
 
        return result;
    }
 
    // ── CLIENT — track view: không cache, ghi ngay ────────────────────────────
 
    public async Task TrackChapterViewAsync(
        TrackChapterViewInput input, Guid? userId, string? ipAddress)
        => await _inner.TrackChapterViewAsync(input, userId, ipAddress);
 
    // ── ADMIN — không cache ────────────────────────────────────────────────────
 
    public async Task<SiteOverviewResult> GetSiteOverviewAsync(SiteOverviewQuery query)
        => await _inner.GetSiteOverviewAsync(query);
 
    public async Task<List<DailyTrafficResult>> GetDailyTrafficAsync(SiteOverviewQuery query)
        => await _inner.GetDailyTrafficAsync(query);
 
    public async Task<List<ChapterRankingItem>> GetTopChaptersAsync(int limit = 10)
        => await _inner.GetTopChaptersAsync(limit);
 
    public async Task<StoryAnalyticsResult> GetStoryAnalyticsAsync(Guid storyId)
        => await _inner.GetStoryAnalyticsAsync(storyId);
}