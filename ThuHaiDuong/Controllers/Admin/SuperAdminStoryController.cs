using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ThuHaiDuong.Application.InterfaceService;
using ThuHaiDuong.Application.Payloads.InputModels.Admin.Story;
using ThuHaiDuong.Application.Payloads.ResultModels.Admin.Story;
using ThuHaiDuong.Shared.Constants;
using ThuHaiDuong.Shared.Models;

namespace ThuHaiDuong.Controllers;

[ApiController]
[Route("api/superadmin/stories")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
    Roles = "SuperAdmin")]
public class SuperAdminStoryController : ControllerBase
{
    private readonly IStoryService _storyService;
 
    public SuperAdminStoryController(IStoryService storyService)
    {
        _storyService = storyService;
    }
 
    [HttpGet("pending")]
    public async Task<ActionResult<PagedResult<StoryResult>>> GetPendingAsync(
        [FromQuery] StoryQuery query)
    {
        query.Status = StoryStatus.PendingReview;
        var result = await _storyService.GetListAdminAsync(query);
        return Ok(result);
    }
 
    [HttpPut("{id:guid}/review")]
    public async Task<IActionResult> ReviewAsync(
        Guid id,
        [FromBody] ReviewStoryInput input)
    {
        await _storyService.ReviewAsync(id, input);
        return NoContent();
    }
}