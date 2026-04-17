using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using ThuHaiDuong.Application.InterfaceService;
using ThuHaiDuong.Application.Payloads.InputModels.Analytics;
using ThuHaiDuong.Application.Payloads.ResultModels.Analytics;

namespace ThuHaiDuong.Controllers.User;

[ApiController]
[Route("api/analytics")]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;
 
    public AnalyticsController(IAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }
 
    private Guid? CurrentUserId => User.Identity?.IsAuthenticated == true
        ? Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!)
        : null;
 
    private string? ClientIp =>
        HttpContext.Connection.RemoteIpAddress?.ToString();
 
    /// <summary>
    /// Track lượt đọc chapter.
    /// Frontend gọi ngay khi user mở trang đọc truyện.
    ///
    /// SessionId: client tự generate UUID và lưu localStorage,
    /// gửi lên để track anonymous user xuyên suốt session.
    ///
    /// Response: 204 No Content — fire-and-forget, không block UX.
    /// </summary>
    [HttpPost("views")]
    public async Task<IActionResult> TrackViewAsync(
        [FromBody] TrackChapterViewInput input)
    {
        await _analyticsService.TrackChapterViewAsync(
            input, CurrentUserId, ClientIp);
 
        return NoContent();
    }
 
    /// <summary>
    /// Top truyện hot — dùng cho trang chủ, section "Đang hot".
    /// Period: "today" | "week" | "month" | "all"
    /// - today/week/month: đọc từ DailyStoryStats (pre-aggregated, nhanh)
    /// - all: đọc TotalViews denormalized trên Stories (nhanh nhất)
    /// </summary>
    [HttpGet("stories/top")]
    public async Task<ActionResult<List<StoryRankingItem>>> GetTopStoriesAsync(
        [FromQuery] StoryRankingQuery query)
    {
        var result = await _analyticsService.GetTopStoriesAsync(query);
        return Ok(result);
    }
}