using Microsoft.AspNetCore.Mvc;
using ThuHaiDuong.Application.InterfaceService;
using ThuHaiDuong.Application.Payloads.ResultModels.User.Chapter;
using ThuHaiDuong.Shared.Models;

namespace ThuHaiDuong.Controllers.User;

[ApiController]
public class ChapterController : ControllerBase
{
    private readonly IChapterService _chapterService;
 
    public ChapterController(IChapterService chapterService)
    {
        _chapterService = chapterService;
    }
 
    [HttpGet("api/stories/{storyId:guid}/chapters")]
    public async Task<ActionResult<PagedResult<ChapterListItem>>> GetListAsync(Guid storyId)
    {
        var result = await _chapterService.GetListAsync(storyId);
        return Ok(result);
    }
 
    [HttpGet("api/chapters/{chapterId:guid}")]
    public async Task<ActionResult<ChapterReadResult>> GetByIdAsync(Guid chapterId)
    {
        var result = await _chapterService.GetForReadingAsync(chapterId);
        return Ok(result);
    }
 
    [HttpGet("api/stories/{storyId:guid}/chapters/{number:int}")]
    public async Task<ActionResult<ChapterReadResult>> GetByNumberAsync(
        Guid storyId, int number)
    {
        var result = await _chapterService.GetForReadingByNumberAsync(storyId, number);
        return Ok(result);
    }
}