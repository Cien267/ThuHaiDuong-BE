using ThuHaiDuong.Application.Payloads.InputModels.Auth;
using ThuHaiDuong.Application.Payloads.ResultModels.Auth;

namespace ThuHaiDuong.Application.InterfaceService
{
    public interface IAuthService
    {
        // ── CLIENT — Reader ───────────────────────────────────────────────────────
        Task<AuthResult> RegisterAsync(RegisterInput input);
 
        // Đăng nhập client — chỉ cho phép Role = Reader
        // Staff cố login qua đây → 403
        Task<AuthResult> ClientLoginAsync(LoginInput input);
        Task<AuthResult> GoogleLoginAsync(GoogleLoginInput input);
 
        // ── ADMIN PORTAL — Staff (Contributor/Admin/SuperAdmin) ───────────────────
        // Đăng nhập admin — chỉ cho phép Role != Reader
        // Reader cố login qua đây → 403
        Task<AuthResult> AdminLoginAsync(LoginInput input);
 
        // ── SUPER ADMIN ONLY ──────────────────────────────────────────────────────
        Task<UserAuthInfo> CreateStaffAsync(CreateStaffInput input);
 
        // ── ALL AUTHENTICATED ─────────────────────────────────────────────────────
        Task<AuthResult> RefreshTokenAsync(RefreshTokenInput input);
        Task LogoutAsync(Guid userId);
        Task ChangePasswordAsync(Guid userId, ChangePasswordInput input);
    }
}
