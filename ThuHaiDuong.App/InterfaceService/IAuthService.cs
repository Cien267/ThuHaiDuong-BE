using ThuHaiDuong.Application.Payloads.InputModels.Auth;
using ThuHaiDuong.Application.Payloads.ResultModels.Auth;

namespace ThuHaiDuong.Application.InterfaceService
{
    public interface IAuthService
    {
        // ── CLIENT — Reader ───────────────────────────────────────────────────────
        Task<AuthResult> RegisterAsync(RegisterInput input, string? ipAddress, string? userAgent);
 
        // Đăng nhập client — chỉ cho phép Role = Reader
        // Staff cố login qua đây → 403
        Task<AuthResult> ClientLoginAsync(LoginInput input, string? ipAddress, string? userAgent);
        Task<AuthResult> GoogleLoginAsync(GoogleLoginInput input, string? ipAddress, string? userAgent);
 
        // ── ADMIN PORTAL — Staff (Contributor/Admin/SuperAdmin) ───────────────────
        // Đăng nhập admin — chỉ cho phép Role != Reader
        // Reader cố login qua đây → 403
        Task<AuthResult> AdminLoginAsync(LoginInput input, string? ipAddress, string? userAgent);
 
        // ── SUPER ADMIN ONLY ──────────────────────────────────────────────────────
        Task<UserAuthInfo> CreateStaffAsync(CreateStaffInput input);
 
        // ── ALL AUTHENTICATED ─────────────────────────────────────────────────────
        Task<AuthResult> RefreshTokenAsync(RefreshTokenInput input, string? ipAddress, string? userAgent);
        Task LogoutAsync(Guid userId);
        Task ChangePasswordAsync(Guid userId, ChangePasswordInput input);
    }
}
