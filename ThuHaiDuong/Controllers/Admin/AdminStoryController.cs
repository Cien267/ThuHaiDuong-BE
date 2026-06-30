using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ThuHaiDuong.Application.InterfaceService;
using ThuHaiDuong.Application.Payloads.InputModels.Story;
using ThuHaiDuong.Application.Payloads.ResultModels.Story;
using ThuHaiDuong.Shared.Models;

namespace ThuHaiDuong.Controllers;

[ApiController]
[Route("api/admin/stories")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class AdminStoryController : ControllerBase
{
    private readonly IStoryService _storyService;
 
    public AdminStoryController(IStoryService storyService)
    {
        _storyService = storyService;
    }
    
    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
 
    private string CurrentUserRole =>
        User.FindFirstValue(ClaimTypes.Role)!;
 
    [HttpGet]
    [Authorize(Roles = "Contributor,Admin,SuperAdmin")]
    public async Task<ActionResult<PagedResult<StoryResult>>> GetListAsync(
        [FromQuery] StoryQuery query)
    {
        if (CurrentUserRole == "Contributor")
            query.UploadedByUserId = CurrentUserId;
        
        var result = await _storyService.GetListAdminAsync(query);
        return Ok(result);
    }
 
    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Contributor,Admin,SuperAdmin")]
    public async Task<ActionResult<StoryResult>> GetByIdAsync(Guid id)
    {
        var result = await _storyService.GetByIdAdminAsync(id);
        return Ok(result);
    }
 
    [HttpPut("{id:guid}/status")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> UpdateStatusAsync(
        Guid id,
        [FromBody] UpdateStoryStatusInput input)
    {
        await _storyService.UpdateStatusAsync(id, input);
        return NoContent();
    }
}