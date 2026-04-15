using Microsoft.AspNetCore.Mvc;
using ThuHaiDuong.Application.InterfaceService;
using ThuHaiDuong.Application.Payloads.ResultModels.User.Tag;

namespace ThuHaiDuong.Controllers.User;

[ApiController]
[Route("api/tags")]
public class TagController : ControllerBase
{
    private readonly ITagService _tagService;
 
    public TagController(ITagService tagService)
    {
        _tagService = tagService;
    }
 
    [HttpGet]
    public async Task<ActionResult<List<TagSummary>>> GetAllAsync(
        [FromQuery] string? search = null)
    {
        var result = await _tagService.GetAllAsync(search);
        return Ok(result);
    }
}