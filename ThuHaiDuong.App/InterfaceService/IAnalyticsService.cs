using ThuHaiDuong.Application.Payloads.InputModels.Analytics;
using ThuHaiDuong.Application.Payloads.ResultModels.Analytics;

namespace ThuHaiDuong.Application.InterfaceService;

public interface IAnalyticsService
{
    // ── CLIENT ────────────────────────────────────────────────────────────────
 
    // Frontend gọi khi user mở trang đọc chapter
    Task TrackChapterViewAsync(TrackChapterViewInput input, Guid? userId, string? ipAddress);
 
    // Trang chủ: truyện hot theo period
    Task<List<StoryRankingItem>> GetTopStoriesAsync(StoryRankingQuery query);
 
    // ── ADMIN DASHBOARD ───────────────────────────────────────────────────────
 
    // Widget cards: tổng quan nhanh
    Task<SiteOverviewResult> GetSiteOverviewAsync(SiteOverviewQuery query);
 
    // Chart: traffic toàn site theo ngày
    Task<List<DailyTrafficResult>> GetDailyTrafficAsync(SiteOverviewQuery query);
 
    // Bảng: top chapter hot all-time
    Task<List<ChapterRankingItem>> GetTopChaptersAsync(int limit = 10);
 
    // Chi tiết analytics của 1 story cụ thể
    Task<StoryAnalyticsResult> GetStoryAnalyticsAsync(Guid storyId);
}