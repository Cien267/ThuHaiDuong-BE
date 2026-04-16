using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ThuHaiDuong.Application.InterfaceService;
using ThuHaiDuong.Application.Payloads.InputModels.Admin.Chapter;
using ThuHaiDuong.Application.Payloads.ResultModels.User.Chapter;
using ThuHaiDuong.Shared.Models;

namespace ThuHaiDuong.Controllers;

[ApiController]
[Route("api/admin/chapters")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
    Roles = "Admin,SuperAdmin")]
public class AdminChapterController : ControllerBase
{
    private readonly IChapterService _chapterService;
 
    public AdminChapterController(IChapterService chapterService)
    {
        _chapterService = chapterService;
    }
 
    [HttpGet]
    public async Task<ActionResult<PagedResult<ChapterListItem>>> GetListAsync(
        [FromQuery] ChapterQuery query)
    {
        var result = await _chapterService.GetListAdminAsync(query);
        return Ok(result);
    }
 
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ChapterReadResult>> GetByIdAsync(Guid id)
    {
        var result = await _chapterService.GetForReadingAsync(id);
        return Ok(result);
    }
}