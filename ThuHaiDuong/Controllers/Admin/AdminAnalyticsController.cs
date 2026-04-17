using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ThuHaiDuong.Application.InterfaceService;
using ThuHaiDuong.Application.Payloads.InputModels.Analytics;
using ThuHaiDuong.Application.Payloads.ResultModels.Analytics;
using ThuHaiDuong.Domain.InterfaceRepositories;

namespace ThuHaiDuong.Controllers;

[ApiController]
[Route("api/admin/analytics")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
    Roles = "Admin,SuperAdmin")]
public class AdminAnalyticsController : ControllerBase
{
    private readonly IAnalyticsService   _analyticsService;
    private readonly IAnalyticsRepository _analyticsRepo;
 
    public AdminAnalyticsController(
        IAnalyticsService    analyticsService,
        IAnalyticsRepository analyticsRepo)
    {
        _analyticsService = analyticsService;
        _analyticsRepo    = analyticsRepo;
    }
 
    /// <summary>
    /// Widget cards cho dashboard:
    /// - Trong khoảng thời gian: views, unique visitors, new users,
    ///   new comments, new ratings, new bookmarks
    /// - All-time: total stories, chapters, users
    /// Mặc định: 30 ngày gần nhất.
    /// </summary>
    [HttpGet("overview")]
    public async Task<ActionResult<SiteOverviewResult>> GetOverviewAsync(
        [FromQuery] SiteOverviewQuery query)
    {
        var result = await _analyticsService.GetSiteOverviewAsync(query);
        return Ok(result);
    }
 
    /// <summary>
    /// Chart traffic toàn site theo ngày.
    /// Mỗi điểm dữ liệu: date, chapterViews, uniqueVisitors,
    ///                    newUsers, newComments, newRatings.
    /// Mặc định: 30 ngày gần nhất.
    /// </summary>
    [HttpGet("traffic")]
    public async Task<ActionResult<List<DailyTrafficResult>>> GetDailyTrafficAsync(
        [FromQuery] SiteOverviewQuery query)
    {
        var result = await _analyticsService.GetDailyTrafficAsync(query);
        return Ok(result);
    }
 
    /// <summary>
    /// Bảng xếp hạng truyện hot — admin version (có thể xem tất cả period).
    /// Period: "today" | "week" | "month" | "all"
    /// </summary>
    [HttpGet("stories/top")]
    public async Task<ActionResult<List<StoryRankingItem>>> GetTopStoriesAsync(
        [FromQuery] StoryRankingQuery query)
    {
        var result = await _analyticsService.GetTopStoriesAsync(query);
        return Ok(result);
    }
 
    /// <summary>
    /// Bảng xếp hạng chapter hot all-time.
    /// Dùng ViewCount denormalized trên Chapters — không cần aggregate.
    /// </summary>
    [HttpGet("chapters/top")]
    public async Task<ActionResult<List<ChapterRankingItem>>> GetTopChaptersAsync(
        [FromQuery] int limit = 10)
    {
        var result = await _analyticsService.GetTopChaptersAsync(limit);
        return Ok(result);
    }
 
    /// <summary>
    /// Analytics chi tiết của 1 story:
    /// - Tổng views, rating, bookmark, comment
    /// - View theo ngày (30 ngày gần nhất từ DailyStoryStats)
    /// - Top 5 chapter hot nhất của story
    /// </summary>
    [HttpGet("stories/{storyId:guid}")]
    public async Task<ActionResult<StoryAnalyticsResult>> GetStoryAnalyticsAsync(
        Guid storyId)
    {
        var result = await _analyticsService.GetStoryAnalyticsAsync(storyId);
        return Ok(result);
    }
 
    /// <summary>
    /// Trigger aggregate thủ công cho 1 ngày cụ thể.
    /// Dùng khi: job chạy tự động bị lỗi, hoặc cần re-aggregate ngày cũ.
    /// Chỉ SuperAdmin được phép.
    /// </summary>
    [HttpPost("aggregate")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> TriggerAggregateAsync(
        [FromQuery] DateOnly? date = null)
    {
        var targetDate = date ?? DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1));
        await _analyticsRepo.AggregateDailyStatsAsync(targetDate);
        return NoContent();
    }
}