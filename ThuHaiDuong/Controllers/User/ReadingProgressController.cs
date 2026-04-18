using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ThuHaiDuong.Application.InterfaceService;
using ThuHaiDuong.Application.Payloads.InputModels.ReadingProgress;
using ThuHaiDuong.Application.Payloads.ResultModels.ReadingProgress;
using ThuHaiDuong.Shared.Models;

namespace ThuHaiDuong.Controllers.User;

[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class ReadingProgressController : ControllerBase
{
    private readonly IReadingProgressService _progressService;
 
    public ReadingProgressController(IReadingProgressService progressService)
    {
        _progressService = progressService;
    }
 
    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
 
    /// <summary>
    /// Cập nhật tiến độ đọc khi user chuyển sang chapter mới.
    ///
    /// Frontend gọi endpoint này ngay khi user:
    ///   - Click vào chapter từ danh sách
    ///   - Bấm nút "Chương tiếp theo" / "Chương trước"
    ///
    /// Logic:
    ///   - UserReadingProgress: chỉ update nếu ChapterNumber MỚI HƠN chapter hiện tại
    ///     → tránh mất progress khi user quay lại đọc chương cũ
    ///   - ReadingHistory: UPSERT (tạo mới hoặc update ReadAt nếu đọc lại)
    ///
    /// Response: 204 No Content.
    /// </summary>
    [HttpPut("api/reading-progress")]
    public async Task<IActionResult> UpdateProgressAsync(
        [FromBody] UpdateReadingProgressInput input)
    {
        await _progressService.UpdateAsync(CurrentUserId, input);
        return NoContent();
    }
 
    /// <summary>
    /// Tiến độ đọc của user với 1 story cụ thể.
    ///
    /// Response bao gồm:
    ///   - LastChapter: chapter đang đọc dở (dùng cho nút "Đọc tiếp")
    ///   - ReadChapterCount / TotalPublishedChapters: tiến độ số chapter
    ///   - ProgressPercent: % hoàn thành (tính từ hai giá trị trên)
    ///
    /// Trả về 404 nếu user chưa đọc chapter nào của story này.
    /// </summary>
    [HttpGet("api/reading-progress/{storyId:guid}")]
    public async Task<ActionResult<ReadingProgressResult>> GetProgressAsync(Guid storyId)
    {
        var result = await _progressService.GetProgressAsync(CurrentUserId, storyId);
 
        if (result == null)
            return NotFound(new { message = "No reading progress found for this story." });
 
        return Ok(result);
    }
 
    /// <summary>
    /// Lịch sử đọc của user — tất cả chapter đã đọc, sort theo ReadAt DESC.
    ///
    /// Filter: storyId (optional) — xem lịch sử trong 1 truyện cụ thể.
    ///
    /// Dùng cho:
    ///   - Trang "Lịch sử đọc" (không filter storyId)
    ///   - Trang chi tiết truyện (filter storyId → biết user đã đọc đến đâu)
    /// </summary>
    [HttpGet("api/reading-history")]
    public async Task<ActionResult<PagedResult<ReadingHistoryItem>>> GetHistoryAsync(
        [FromQuery] ReadingHistoryQuery query)
    {
        var result = await _progressService.GetHistoryAsync(CurrentUserId, query);
        return Ok(result);
    }
 
    /// <summary>
    /// Danh sách Id của các chapter đã đọc trong 1 story.
    ///
    /// Dùng để đánh dấu "✓ đã đọc" trên danh sách chapter của trang truyện.
    /// Frontend nhận Set<Guid> → kiểm tra O(1) cho từng chapter.
    ///
    /// Ví dụ response: ["uuid1", "uuid2", "uuid3"]
    /// </summary>
    [HttpGet("api/reading-history/{storyId:guid}/chapters")]
    public async Task<ActionResult<HashSet<Guid>>> GetReadChapterIdsAsync(Guid storyId)
    {
        var result = await _progressService.GetReadChapterIdsAsync(CurrentUserId, storyId);
        return Ok(result);
    }
 
    /// <summary>
    /// Xóa toàn bộ lịch sử đọc của user với 1 story.
    /// Đồng thời reset UserReadingProgress → nút "Đọc tiếp" sẽ về chương đầu.
    ///
    /// Dùng cho: tính năng "Đọc lại từ đầu".
    /// </summary>
    [HttpDelete("api/reading-history/{storyId:guid}")]
    public async Task<IActionResult> ClearHistoryAsync(Guid storyId)
    {
        await _progressService.ClearHistoryAsync(CurrentUserId, storyId);
        return NoContent();
    }
}