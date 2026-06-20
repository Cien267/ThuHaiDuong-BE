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
    private readonly IFileStorageService  _fileStorage;
 
    public StoryPortalController(IStoryService storyService, IFileStorageService   fileStorage)
    {
        _storyService = storyService;
        _fileStorage = fileStorage;
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
        return Ok(result);
    }
 
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<StoryResult>> UpdateAsync(
        Guid id,
        [FromBody] UpdateStoryInput input)
    {
        var result = await _storyService.UpdateAsync(id, input, CurrentUserId, CurrentUserRole);
        return Ok(result);
    }
    
    /// <summary>
    /// Upload ảnh bìa cho truyện.
    /// Allowed: jpeg, png, webp. Max 5MB.
    /// </summary>
    [HttpPost("upload-cover")]
    [RequestSizeLimit(5 * 1024 * 1024)]
    public async Task<ActionResult<CoverUploadResult>> UploadCoverAsync(IFormFile file)
    {
        var url = await _fileStorage.UploadAsync(file, "covers");
        return Ok(new CoverUploadResult { CoverImageUrl = url });
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