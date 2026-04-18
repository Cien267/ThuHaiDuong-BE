using System.ComponentModel.DataAnnotations;

namespace ThuHaiDuong.Application.Payloads.InputModels.Auth;

public class RefreshTokenInput
{
    [Required]
    public string RefreshToken { get; set; } = null!;
}