using Microsoft.AspNetCore.Mvc;
using ThuHaiDuong.Application.InterfaceService;
using ThuHaiDuong.Application.Payloads.InputModels.Story;
using ThuHaiDuong.Application.Payloads.ResultModels.User.Story;
using ThuHaiDuong.Shared.Models;

namespace ThuHaiDuong.Controllers.User;

[ApiController]
[Route("api/stories")]
public class StoryController : ControllerBase
{
    private readonly IStoryService _storyService;
 
    public StoryController(IStoryService storyService)
    {
        _storyService = storyService;
    }
 
    [HttpGet]
    public async Task<ActionResult<PagedResult<StorySummary>>> GetListAsync(
        [FromQuery] StoryQuery query)
    {
        var result = await _storyService.GetListAsync(query);
        return Ok(result);
    }
 
    [HttpGet("{slug}")]
    public async Task<ActionResult<StoryDetail>> GetBySlugAsync(string slug)
    {
        var result = await _storyService.GetBySlugAsync(slug);
        return Ok(result);
    }
}