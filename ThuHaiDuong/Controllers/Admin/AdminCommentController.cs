using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ThuHaiDuong.Application.InterfaceService;
using ThuHaiDuong.Application.Payloads.InputModels.Comment;
using ThuHaiDuong.Application.Payloads.ResultModels.Comment;
using ThuHaiDuong.Shared.Models;

namespace ThuHaiDuong.Controllers;

[ApiController]
[Route("api/admin/comments")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
    Roles = "Admin,SuperAdmin")]
public class AdminCommentController : ControllerBase
{
    private readonly ICommentService _commentService;
 
    public AdminCommentController(ICommentService commentService)
    {
        _commentService = commentService;
    }
 
    [HttpGet]
    public async Task<ActionResult<PagedResult<CommentResult>>> GetListAsync(
        [FromQuery] AdminCommentQuery query)
    {
        var result = await _commentService.GetListAdminAsync(query);
        return Ok(result);
    }
 
    [HttpPatch("{id:guid}/toggle-hide")]
    public async Task<IActionResult> ToggleHideAsync(Guid id)
    {
        await _commentService.ToggleHideAsync(id);
        return NoContent();
    }
 
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        await _commentService.DeleteAsync(id);
        return NoContent();
    }
}