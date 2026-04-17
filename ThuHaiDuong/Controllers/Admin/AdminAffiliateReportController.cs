using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ThuHaiDuong.Application.InterfaceService;
using ThuHaiDuong.Application.Payloads.InputModels.Affiliate;
using ThuHaiDuong.Application.Payloads.ResultModels.Affiliate;
using ThuHaiDuong.Shared.Models;

namespace ThuHaiDuong.Controllers;

[ApiController]
[Route("api/admin/affiliate/reports")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
    Roles = "Admin,SuperAdmin")]
public class AdminAffiliateReportController : ControllerBase
{
    private readonly IAffiliateService _affiliateService;
 
    public AdminAffiliateReportController(IAffiliateService affiliateService)
    {
        _affiliateService = affiliateService;
    }
 
    /// <summary>
    /// Click theo ngày — dùng để vẽ chart.
    /// Filter: linkId (optional, bỏ trống = tất cả link).
    /// Mặc định: 30 ngày gần nhất.
    /// </summary>
    [HttpGet("daily")]
    public async Task<ActionResult<List<AffiliateDailyStatResult>>> GetDailyStatsAsync(
        [FromQuery] Guid? linkId = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null)
    {
        var dateFrom = from ?? DateTime.UtcNow.AddDays(-30);
        var dateTo   = to   ?? DateTime.UtcNow;
 
        var result = await _affiliateService.GetDailyStatsAsync(linkId, dateFrom, dateTo);
        return Ok(result);
    }
 
    /// <summary>
    /// Tổng hợp click theo từng link — bảng xếp hạng link hiệu quả nhất.
    /// Sort theo TotalClicks DESC.
    /// </summary>
    [HttpGet("links")]
    public async Task<ActionResult<List<AffiliateLinkStatResult>>> GetLinkStatsAsync(
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null)
    {
        var result = await _affiliateService.GetLinkStatsAsync(from, to);
        return Ok(result);
    }
 
    /// <summary>
    /// Raw click log — xem chi tiết từng click.
    /// Filter: linkId, storyId, chapterId, fromDate, toDate.
    /// Dùng để debug hoặc export báo cáo.
    /// </summary>
    [HttpGet("clicks")]
    public async Task<ActionResult<PagedResult<AffiliateClickResult>>> GetClicksAsync(
        [FromQuery] AffiliateClickReportQuery query)
    {
        var result = await _affiliateService.GetClicksAsync(query);
        return Ok(result);
    }
}