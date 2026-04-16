using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ThuHaiDuong.Application.InterfaceService;
using ThuHaiDuong.Application.Payloads.InputModels.Story;
using ThuHaiDuong.Application.Payloads.ResultModels.Story;

namespace ThuHaiDuong.Controllers;

[ApiController]
[Route("api/portal/stories")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class StoryPortalController : ControllerBase
{
    private readonly IStoryService _storyService;
 
    public StoryPortalController(IStoryService storyService)
    {
        _storyService = storyService;
    }
 
    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
 
    private string CurrentUserRole =>
        User.FindFirstValue(ClaimTypes.Role)!;
 
    [HttpPost]
    public async Task<ActionResult<StoryResult>> CreateAsync(
        [FromBody] CreateStoryInput input)
    {
        var result = await _storyService.CreateAsync(input, CurrentUserId);
        return CreatedAtAction(
            nameof(AdminStoryController.GetByIdAsync),
            "AdminStory",
            new { id = result.Id },
            result);
    }
 
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<StoryResult>> UpdateAsync(
        Guid id,
        [FromBody] UpdateStoryInput input)
    {
        var result = await _storyService.UpdateAsync(id, input, CurrentUserId, CurrentUserRole);
        return Ok(result);
    }
 
    [HttpPost("{id:guid}/submit")]
    public async Task<IActionResult> SubmitForReviewAsync(Guid id)
    {
        await _storyService.SubmitForReviewAsync(id, CurrentUserId, CurrentUserRole);
        return NoContent();
    }
 
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        await _storyService.DeleteAsync(id, CurrentUserId, CurrentUserRole);
        return NoContent();
    }
}