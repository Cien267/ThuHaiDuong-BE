using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using ThuHaiDuong.Application.InterfaceService;
using ThuHaiDuong.Application.Payloads.ResultModels.Affiliate;

namespace ThuHaiDuong.Controllers.User;

[ApiController]
public class AffiliateController : ControllerBase
{
    private readonly IAffiliateService _affiliateService;
 
    public AffiliateController(IAffiliateService affiliateService)
    {
        _affiliateService = affiliateService;
    }
 
    private Guid? CurrentUserId => User.Identity?.IsAuthenticated == true
        ? Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!)
        : null;
 
    [HttpGet("/go/{code}")]
    public async Task<IActionResult> RedirectAsync(
        string code,
        [FromQuery] Guid? chapterId = null)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = Request.Headers.UserAgent.ToString();
        var referrer  = Request.Headers.Referer.ToString();
 
        var targetUrl = await _affiliateService.TrackAndGetTargetUrlAsync(
            trackingCode: code,
            userId:       CurrentUserId,
            chapterId:    chapterId,
            ipAddress:    ipAddress,
            userAgent:    userAgent,
            referrer:     string.IsNullOrWhiteSpace(referrer) ? null : referrer);
 
        // 302 Temporary Redirect — không cache để tracking luôn chạy
        return Redirect(targetUrl);
    }
 
    [HttpGet("/api/affiliate/display")]
    public async Task<ActionResult<List<AffiliateDisplayResult>>> GetDisplayLinksAsync(
        [FromQuery] Guid storyId,
        [FromQuery] Guid chapterId)
    {
        var result = await _affiliateService.GetDisplayLinksAsync(storyId, chapterId);
        return Ok(result);
    }
}