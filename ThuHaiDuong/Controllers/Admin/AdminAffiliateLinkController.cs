using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ThuHaiDuong.Application.InterfaceService;
using ThuHaiDuong.Application.Payloads.InputModels.Affiliate;
using ThuHaiDuong.Application.Payloads.ResultModels.Affiliate;
using ThuHaiDuong.Shared.Models;

namespace ThuHaiDuong.Controllers;

[ApiController]
[Route("api/admin/affiliate/links")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
    Roles = "Admin,SuperAdmin")]
public class AdminAffiliateLinkController : ControllerBase
{
    private readonly IAffiliateService _affiliateService;
    private readonly IFileStorageService  _fileStorage;
 
    public AdminAffiliateLinkController(IAffiliateService affiliateService, IFileStorageService   fileStorage)
    {
        _affiliateService = affiliateService;
        _fileStorage = fileStorage;
    }
 
    /// <summary>
    /// Danh sách affiliate links — filter theo name, placement, isActive.
    /// Bao gồm TotalClicks, TotalStories, TotalChapters.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PagedResult<AffiliateLinkResult>>> GetListAsync(
        [FromQuery] AffiliateLinkQuery query)
    {
        var result = await _affiliateService.GetListAsync(query);
        return Ok(result);
    }
 
    /// <summary>
    /// Chi tiết 1 link — bao gồm danh sách story/chapter đã gắn.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AffiliateLinkResult>> GetByIdAsync(Guid id)
    {
        var result = await _affiliateService.GetByIdAsync(id);
        return Ok(result);
    }
 
    /// <summary>
    /// Tạo affiliate link mới.
    /// TrackingCode tự generate 8 ký tự nếu không truyền.
    /// Có thể gắn vào story/chapter ngay lúc tạo qua StoryIds/ChapterIds.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<AffiliateLinkResult>> CreateAsync(
        [FromBody] CreateAffiliateLinkInput input)
    {
        var result = await _affiliateService.CreateAsync(input);
        return Ok(result);
    }
 
    /// <summary>
    /// Cập nhật affiliate link.
    /// TrackingCode không thể thay đổi sau khi tạo (URL đã phát tán).
    /// StoryIds/ChapterIds sẽ replace toàn bộ danh sách cũ.
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<AffiliateLinkResult>> UpdateAsync(
        Guid id,
        [FromBody] UpdateAffiliateLinkInput input)
    {
        var result = await _affiliateService.UpdateAsync(id, input);
        return Ok(result);
    }
 
    /// <summary>
    /// Xóa mềm affiliate link.
    /// Click history vẫn giữ nguyên cho báo cáo.
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        await _affiliateService.DeleteAsync(id);
        return NoContent();
    }
    
    /// <summary>
    /// Upload ảnh bìa cho affiliate.
    /// Allowed: jpeg, png, webp. Max 5MB.
    /// </summary>
    [HttpPost("upload-image")]
    [RequestSizeLimit(5 * 1024 * 1024)]
    public async Task<ActionResult<AffiliateImageUploadResult>> UploadCoverAsync(IFormFile file)
    {
        var url = await _fileStorage.UploadAsync(file, "affiliate-images");
        return Ok(new AffiliateImageUploadResult { ImageUrl = url });
    }
}