using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ThuHaiDuong.Application.InterfaceService;
using ThuHaiDuong.Application.Payloads.ResultModels.Admin.Bookmark;
using ThuHaiDuong.Application.Payloads.ResultModels.User.Bookmark;

namespace ThuHaiDuong.Controllers.User;

[ApiController]
[Route("api/bookmarks")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class BookmarkController : ControllerBase
{
    private readonly IBookmarkService _bookmarkService;
 
    public BookmarkController(IBookmarkService bookmarkService)
    {
        _bookmarkService = bookmarkService;
    }
 
    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
 
    [HttpGet]
    public async Task<ActionResult<List<BookmarkResult>>> GetMyBookmarksAsync()
    {
        var result = await _bookmarkService.GetUserBookmarksAsync(CurrentUserId);
        return Ok(result);
    }
 
    [HttpGet("{storyId:guid}")]
    public async Task<ActionResult<bool>> IsBookmarkedAsync(Guid storyId)
    {
        var result = await _bookmarkService.IsBookmarkedAsync(CurrentUserId, storyId);
        return Ok(result);
    }
 
    [HttpPost("{storyId:guid}/toggle")]
    public async Task<ActionResult<BookmarkToggleResult>> ToggleAsync(Guid storyId)
    {
        var result = await _bookmarkService.ToggleAsync(CurrentUserId, storyId);
        return Ok(result);
    }
}