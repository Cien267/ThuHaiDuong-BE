using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using ThuHaiDuong.Application.InterfaceService;
using ThuHaiDuong.Application.Payloads.InputModels.Comment;
using ThuHaiDuong.Application.Payloads.ResultModels.Comment;
using ThuHaiDuong.Shared.Models;

namespace ThuHaiDuong.Controllers.User;

[ApiController]
[Route("api/comments")]
public class CommentController : ControllerBase
{
    private readonly ICommentService _commentService;
 
    public CommentController(ICommentService commentService)
    {
        _commentService = commentService;
    }
 
    private Guid? CurrentUserId => User.Identity?.IsAuthenticated == true
        ? Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!)
        : null;
 
    [HttpGet]
    public async Task<ActionResult<PagedResult<CommentResult>>> GetListAsync(
        [FromQuery] CommentQuery query)
    {
        var result = await _commentService.GetListAsync(query);
        return Ok(result);
    }
 
    [HttpPost]
    public async Task<ActionResult<CommentResult>> CreateAsync(
        [FromBody] CreateCommentInput input)
    {
        var result = await _commentService.CreateAsync(input, CurrentUserId);
        return CreatedAtAction(nameof(GetListAsync),
            new { storyId = result.StoryId }, result);
    }
}