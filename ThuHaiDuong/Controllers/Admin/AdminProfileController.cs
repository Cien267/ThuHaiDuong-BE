using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ThuHaiDuong.Application.InterfaceService;
using ThuHaiDuong.Application.Payloads.InputModels.User;
using ThuHaiDuong.Application.Payloads.ResultModels.User;

namespace ThuHaiDuong.Controllers;

[ApiController]
[Route("api/admin/profile")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
    Roles = "Contributor,Admin,SuperAdmin")]
public class AdminProfileController : ControllerBase
{
    private readonly IUserProfileService _profileService;
 
    public AdminProfileController(IUserProfileService profileService)
    {
        _profileService = profileService;
    }
 
    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
 
    /// <summary>
    /// Profile của staff đang đăng nhập.
    /// Trả về: id, userName, email, fullName, phoneNumber, avatar, role, lastLoginAt.
    /// Không có bookmark/comment/rating stats (những thứ chỉ dành cho Reader).
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<StaffProfileResult>> GetMyProfileAsync()
    {
        var result = await _profileService.GetMyStaffProfileAsync(CurrentUserId);
        return Ok(result);
    }
 
    /// <summary>
    /// Cập nhật FullName và PhoneNumber.
    /// </summary>
    [HttpPut]
    public async Task<ActionResult<StaffProfileResult>> UpdateProfileAsync(
        [FromBody] UpdateProfileInput input)
    {
        var result = await _profileService.UpdateStaffProfileAsync(CurrentUserId, input);
        return Ok(result);
    }
 
    /// <summary>
    /// Đổi username — kiểm tra unique, 409 nếu đã tồn tại.
    /// </summary>
    [HttpPatch("username")]
    public async Task<ActionResult<StaffProfileResult>> UpdateUsernameAsync(
        [FromBody] UpdateUsernameInput input)
    {
        var result = await _profileService.UpdateStaffUsernameAsync(CurrentUserId, input);
        return Ok(result);
    }
 
    /// <summary>
    /// Upload avatar mới (multipart/form-data, field = "file").
    /// Allowed: jpeg, png, webp, gif. Max 5MB.
    /// Avatar cũ bị xóa sau khi DB update thành công.
    /// </summary>
    [HttpPost("avatar")]
    [RequestSizeLimit(5 * 1024 * 1024)]
    public async Task<ActionResult<AvatarUploadResult>> UploadAvatarAsync(IFormFile file)
    {
        var result = await _profileService.UploadAvatarAsync(CurrentUserId, file);
        return Ok(result);
    }
 
    /// <summary>
    /// Xóa avatar — set về null, file vật lý bị xóa khỏi server.
    /// </summary>
    [HttpDelete("avatar")]
    public async Task<IActionResult> RemoveAvatarAsync()
    {
        await _profileService.RemoveAvatarAsync(CurrentUserId);
        return NoContent();
    }
}