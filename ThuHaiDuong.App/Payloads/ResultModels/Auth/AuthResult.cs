namespace ThuHaiDuong.Application.Payloads.ResultModels.Auth;

public class AuthResult
{
    public string AccessToken { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
    public DateTime AccessTokenExpiresAt { get; set; }
    public UserAuthInfo User { get; set; } = null!;
}