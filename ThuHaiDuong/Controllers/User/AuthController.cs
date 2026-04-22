using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ThuHaiDuong.Application.InterfaceService;
using ThuHaiDuong.Application.Payloads.InputModels.Auth;
using ThuHaiDuong.Application.Payloads.ResultModels.Auth;

namespace ThuHaiDuong.Controllers.User;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
 
    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }
 
    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
 
    /// <summary>
    /// Đăng ký tài khoản Reader mới bằng email và password.
    /// Response: access token + refresh token + thông tin user.
    /// </summary>
    [HttpPost("register")]
    public async Task<ActionResult<AuthResult>> RegisterAsync(
        [FromBody] RegisterInput input)
    {
        var result = await _authService.RegisterAsync(input);
        return Ok(result);
    }
 
    /// <summary>
    /// Đăng nhập Reader bằng email/password.
    /// Staff (Contributor/Admin/SuperAdmin) cố login qua đây → 403.
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<AuthResult>> LoginAsync(
        [FromBody] LoginInput input)
    {
        var result = await _authService.ClientLoginAsync(input);
        return Ok(result);
    }
 
    /// <summary>
    /// Đăng nhập bằng Google OAuth — chỉ dành cho Reader.
    /// Frontend dùng Google Sign-In SDK → nhận idToken → gửi lên đây.
    ///
    /// Flow:
    ///   1. Frontend: Google Sign-In → nhận idToken
    ///   2. Frontend: POST /api/auth/google { idToken }
    ///   3. Backend: verify idToken với Google server
    ///   4. Backend: tìm hoặc tạo user → trả về JWT
    ///
    /// Nếu email đã tồn tại với Role khác Reader → 403.
    /// </summary>
    [HttpPost("google")]
    public async Task<ActionResult<AuthResult>> GoogleLoginAsync(
        [FromBody] GoogleLoginInput input)
    {
        var result = await _authService.GoogleLoginAsync(input);
        return Ok(result);
    }
 
    /// <summary>
    /// Làm mới access token bằng refresh token.
    /// Access token cũ không cần gửi lên.
    ///
    /// Flow frontend:
    ///   1. Gọi API → nhận 401
    ///   2. POST /api/auth/refresh { refreshToken }
    ///   3. Nhận access token mới → retry request cũ
    ///   4. Nếu refresh cũng 401 → redirect về trang login
    /// </summary>
    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResult>> RefreshAsync(
        [FromBody] RefreshTokenInput input)
    {
        var result = await _authService.RefreshTokenAsync(input);
        return Ok(result);
    }
 
    /// <summary>
    /// Đăng xuất — revoke refresh token hiện tại.
    /// Access token vẫn còn hiệu lực đến khi hết hạn (stateless JWT).
    /// Frontend cần tự xóa token khỏi storage.
    /// </summary>
    [HttpPost("logout")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> LogoutAsync()
    {
        await _authService.LogoutAsync(CurrentUserId);
        return NoContent();
    }
 
    /// <summary>
    /// Đổi mật khẩu.
    /// Sau khi đổi → tự động revoke tất cả refresh token
    /// → user phải login lại trên tất cả thiết bị.
    /// </summary>
    [HttpPut("change-password")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> ChangePasswordAsync(
        [FromBody] ChangePasswordInput input)
    {
        await _authService.ChangePasswordAsync(CurrentUserId, input);
        return NoContent();
    }
 
    /// <summary>
    /// Thông tin user đang đăng nhập.
    /// Dùng khi app khởi động để restore session từ stored token.
    /// </summary>
    [HttpGet("me")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public IActionResult MeAsync()
    {
        var userInfo = new UserAuthInfo
        {
            Id       = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!),
            UserName = User.FindFirstValue(ClaimTypes.Name)!,
            Email    = User.FindFirstValue(ClaimTypes.Email)!,
            Role     = User.FindFirstValue(ClaimTypes.Role)!,
        };
 
        // Đọc trực tiếp từ JWT claims — không query DB
        return Ok(userInfo);
    }
}