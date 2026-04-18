using ThuHaiDuong.Application.Payloads.InputModels.Auth;

namespace ThuHaiDuong.Application.InterfaceService;

public interface IGoogleAuthService
{
    // Verify idToken từ Google → trả về thông tin user
    Task<GoogleUserInfo> VerifyIdTokenAsync(string idToken);
}