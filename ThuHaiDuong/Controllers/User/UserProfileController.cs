using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ThuHaiDuong.Application.InterfaceService;
using ThuHaiDuong.Application.Payloads.InputModels.User;
using ThuHaiDuong.Application.Payloads.ResultModels.User;

namespace ThuHaiDuong.Controllers.User;

[ApiController]
[Route("api/profile")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class UserProfileController : ControllerBase
{
    private readonly IUserProfileService _profileService;
 
    public UserProfileController(IUserProfileService profileService)
    {
        _profileService = profileService;
    }
 
    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
 
    /// <summary>
    /// Thông tin profile đầy đủ của user đang login.
    /// Bao gồm: thông tin cá nhân + BookmarkCount, CommentCount, RatingCount.
    /// Dùng cho: trang profile, header avatar.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<UserProfileResult>> GetMyProfileAsync()
    {
        var result = await _profileService.GetMyProfileAsync(CurrentUserId);
        return Ok(result);
    }
 
    /// <summary>
    /// Cập nhật thông tin cơ bản: FullName, PhoneNumber.
    /// Email và Role không cho sửa qua endpoint này.
    /// </summary>
    [HttpPut]
    public async Task<ActionResult<UserProfileResult>> UpdateProfileAsync(
        [FromBody] UpdateProfileInput input)
    {
        var result = await _profileService.UpdateProfileAsync(CurrentUserId, input);
        return Ok(result);
    }
 
    /// <summary>
    /// Đổi username.
    /// Tách riêng khỏi UpdateProfile vì username cần check unique — thao tác nhạy cảm hơn.
    /// 409 nếu username đã tồn tại.
    /// Dùng PATCH vì chỉ thay đổi 1 field.
    /// </summary>
    [HttpPatch("username")]
    public async Task<ActionResult<UserProfileResult>> UpdateUsernameAsync(
        [FromBody] UpdateUsernameInput input)
    {
        var result = await _profileService.UpdateUsernameAsync(CurrentUserId, input);
        return Ok(result);
    }
 
    /// <summary>
    /// Upload avatar mới.
    /// Request: multipart/form-data, field name = "file".
    /// Allowed: image/jpeg, image/png, image/webp, image/gif. Max 5MB.
    ///
    /// Flow:
    ///   1. Upload file mới → lưu vào /uploads/avatars/
    ///   2. Cập nhật Avatar URL trong DB
    ///   3. Xóa file cũ (nếu có)
    ///
    /// Avatar cũ chỉ bị xóa SAU KHI DB đã update thành công.
    /// Response: { avatarUrl: "..." }
    /// </summary>
    [HttpPost("avatar")]
    [RequestSizeLimit(5 * 1024 * 1024)]   // 5MB limit ở middleware level
    public async Task<ActionResult<AvatarUploadResult>> UploadAvatarAsync(
        IFormFile file)
    {
        var result = await _profileService.UploadAvatarAsync(CurrentUserId, file);
        return Ok(result);
    }
 
    /// <summary>
    /// Xóa avatar — về trạng thái không có ảnh (null).
    /// File vật lý cũng bị xóa khỏi server.
    /// Không làm gì nếu user chưa có avatar.
    /// </summary>
    [HttpDelete("avatar")]
    public async Task<IActionResult> RemoveAvatarAsync()
    {
        await _profileService.RemoveAvatarAsync(CurrentUserId);
        return NoContent();
    }
}