using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ThuHaiDuong.Application.InterfaceService;
using ThuHaiDuong.Application.Payloads.ResultModels.Notification;
using ThuHaiDuong.Shared.Models;

namespace ThuHaiDuong.Controllers.Admin;

[ApiController]
[Route("api/notifications")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _service;
    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public NotificationController(INotificationService service)
        => _service = service;

    [HttpGet]
    public async Task<ActionResult<PagedResult<NotificationResult>>> GetListAsync(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _service.GetListAsync(CurrentUserId, page, pageSize);
        return Ok(result);
    }

    [HttpGet("unread-count")]
    public async Task<ActionResult<UnreadCountResult>> GetUnreadCountAsync()
    {
        var count = await _service.GetUnreadCountAsync(CurrentUserId);
        return Ok(new UnreadCountResult { Count = count });
    }

    [HttpPatch("{id:guid}/read")]
    public async Task<IActionResult> MarkReadAsync(Guid id)
    {
        await _service.MarkReadAsync(id, CurrentUserId);
        return NoContent();
    }

    [HttpPatch("read-all")]
    public async Task<IActionResult> MarkAllReadAsync()
    {
        await _service.MarkAllReadAsync(CurrentUserId);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        await _service.DeleteAsync(id, CurrentUserId);
        return NoContent();
    }
}