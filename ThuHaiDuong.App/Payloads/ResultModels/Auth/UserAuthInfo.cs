namespace ThuHaiDuong.Application.Payloads.ResultModels.Auth;

public class UserAuthInfo
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? FullName { get; set; }
    public string? Avatar { get; set; }
    public string Role { get; set; } = null!;
}