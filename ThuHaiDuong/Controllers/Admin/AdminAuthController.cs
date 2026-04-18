using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ThuHaiDuong.Application.InterfaceService;
using ThuHaiDuong.Application.Payloads.InputModels.Auth;
using ThuHaiDuong.Application.Payloads.Responses;
using ThuHaiDuong.Application.Payloads.ResultModels.Auth;
using ThuHaiDuong.Domain.InterfaceRepositories;

namespace ThuHaiDuong.Controllers;

[ApiController]
[Route("api/admin/auth")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
    Roles = "SuperAdmin")]
public class AdminAuthController : ControllerBase
{
    private readonly IAuthService          _authService;
    private readonly IBaseRepository<Domain.Entities.User> _userRepo;
 
    public AdminAuthController(
        IAuthService          authService,
        IBaseRepository<Domain.Entities.User> userRepo)
    {
        _authService = authService;
        _userRepo    = userRepo;
    }
 
    private string? ClientIp =>
        HttpContext.Connection.RemoteIpAddress?.ToString();
 
    private string? ClientUserAgent =>
        Request.Headers.UserAgent.ToString();
 
    /// <summary>
    /// Đăng nhập admin portal — chỉ dành cho Staff (Contributor/Admin/SuperAdmin).
    /// Reader cố login qua đây → 403.
    /// Không hỗ trợ Google OAuth — Staff phải dùng email/password.
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResult>> LoginAsync(
        [FromBody] LoginInput input)
    {
        var result = await _authService.AdminLoginAsync(input, ClientIp, ClientUserAgent);
        return Ok(result);
    }
 
    /// <summary>
    /// Tạo tài khoản Contributor hoặc Admin mới.
    /// Chỉ SuperAdmin được phép.
    /// Không thể tạo SuperAdmin qua API.
    /// </summary>
    [HttpPost("staff")]
    public async Task<ActionResult<UserAuthInfo>> CreateStaffAsync(
        [FromBody] CreateStaffInput input)
    {
        var result = await _authService.CreateStaffAsync(input);
        return CreatedAtAction(nameof(GetStaffListAsync), result);
    }
 
    /// <summary>
    /// Danh sách staff (Contributor, Admin) — không bao gồm Reader.
    /// </summary>
    [HttpGet("staff")]
    public async Task<ActionResult<List<UserAuthInfo>>> GetStaffListAsync()
    {
        var staffRoles = new[] { "Contributor", "Admin", "SuperAdmin" };
 
        var query = _userRepo.BuildQueryable(
            [],
            u => staffRoles.Contains(u.Role) && !u.DeletedAt.HasValue
        );
 
        var staff = await query
            .OrderBy(u => u.Role)
            .ThenBy(u => u.UserName)
            .Select(u => new UserAuthInfo
            {
                Id       = u.Id,
                UserName = u.UserName,
                Email    = u.Email,
                FullName = u.FullName,
                Avatar   = u.Avatar,
                Role     = u.Role,
            })
            .ToListAsync();
 
        return Ok(staff);
    }
 
    /// <summary>
    /// Bật/tắt tài khoản staff.
    /// Tắt → user không thể login, refresh token bị revoke luôn.
    /// </summary>
    [HttpPatch("staff/{id:guid}/toggle-active")]
    public async Task<IActionResult> ToggleActiveAsync(Guid id)
    {
        var user = await _userRepo.GetByIdAsync(id)
            ?? throw new ResponseErrorObject(
                "User not found.", StatusCodes.Status404NotFound);
 
        // Không cho tắt SuperAdmin
        if (user.Role == "SuperAdmin")
            throw new ResponseErrorObject(
                "Cannot deactivate a SuperAdmin account.",
                StatusCodes.Status403Forbidden);
 
        user.IsActive = !user.IsActive;
        await _userRepo.UpdateAsync(user);
 
        return NoContent();
    }
}