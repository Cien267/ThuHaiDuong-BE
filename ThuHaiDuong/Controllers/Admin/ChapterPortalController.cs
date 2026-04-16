using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ThuHaiDuong.Application.InterfaceService;
using ThuHaiDuong.Application.Payloads.InputModels.Admin.Chapter;
using ThuHaiDuong.Application.Payloads.ResultModels.Admin.Chapter;

namespace ThuHaiDuong.Controllers;

[ApiController]
[Route("api/portal/chapters")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class ChapterPortalController : ControllerBase
{
    private readonly IChapterService _chapterService;
 
    public ChapterPortalController(IChapterService chapterService)
    {
        _chapterService = chapterService;
    }
 
    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
 
    private string CurrentUserRole =>
        User.FindFirstValue(ClaimTypes.Role)!;
 
    [HttpPost]
    public async Task<ActionResult<ChapterResult>> CreateAsync(
        [FromBody] CreateChapterInput input)
    {
        var result = await _chapterService.CreateAsync(
            input, CurrentUserId, CurrentUserRole);
 
        return CreatedAtAction(
            nameof(AdminChapterController.GetByIdAsync),
            "AdminChapter",
            new { id = result.Id },
            result);
    }
 
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ChapterResult>> UpdateAsync(
        Guid id,
        [FromBody] UpdateChapterInput input)
    {
        var result = await _chapterService.UpdateAsync(
            id, input, CurrentUserId, CurrentUserRole);
 
        return Ok(result);
    }
 
    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatusAsync(
        Guid id,
        [FromBody] UpdateChapterStatusInput input)
    {
        await _chapterService.UpdateStatusAsync(
            id, input, CurrentUserId, CurrentUserRole);
 
        return NoContent();
    }
 
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        await _chapterService.DeleteAsync(id, CurrentUserId, CurrentUserRole);
        return NoContent();
    }
}