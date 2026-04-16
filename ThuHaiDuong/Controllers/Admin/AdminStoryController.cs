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
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
    Roles = "Admin,SuperAdmin")]
public class AdminStoryController : ControllerBase
{
    private readonly IStoryService _storyService;
 
    public AdminStoryController(IStoryService storyService)
    {
        _storyService = storyService;
    }
 
    [HttpGet]
    public async Task<ActionResult<PagedResult<StoryResult>>> GetListAsync(
        [FromQuery] StoryQuery query)
    {
        var result = await _storyService.GetListAdminAsync(query);
        return Ok(result);
    }
 
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<StoryResult>> GetByIdAsync(Guid id)
    {
        var result = await _storyService.GetByIdAdminAsync(id);
        return Ok(result);
    }
 
    [HttpPut("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatusAsync(
        Guid id,
        [FromBody] UpdateStoryStatusInput input)
    {
        await _storyService.UpdateStatusAsync(id, input);
        return NoContent();
    }
}