namespace ThuHaiDuong.Application.Payloads.InputModels.Auth;

public class GoogleUserInfo
{
    public string GoogleId { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? Name { get; set; }
    public string? Picture { get; set; }
}