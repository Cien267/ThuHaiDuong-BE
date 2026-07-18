using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ThuHaiDuong.Application.InterfaceService;
using ThuHaiDuong.Application.Payloads.InputModels.Utils;
using ThuHaiDuong.Application.Payloads.ResultModels.Utils;

namespace ThuHaiDuong.Controllers.Admin;

[ApiController]
[Route("api/admin/utils")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
    Roles = "Admin,SuperAdmin")]
public class AdminUtilsController : ControllerBase
{
    private readonly ILinkPreviewService _linkPreview;

    public AdminUtilsController(ILinkPreviewService linkPreview)
        => _linkPreview = linkPreview;

    /// <summary>
    /// Fetch OG metadata từ URL — dùng cho affiliate link preview.
    /// </summary>
    [HttpPost("link-preview")]
    public async Task<ActionResult<LinkPreviewResult>> GetLinkPreviewAsync(
        [FromBody] LinkPreviewInput request)
    {
        var result = await _linkPreview.GetPreviewAsync(request.Url);
        return Ok(result);
    }
}