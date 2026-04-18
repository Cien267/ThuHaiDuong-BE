using System.ComponentModel.DataAnnotations;

namespace ThuHaiDuong.Application.Payloads.InputModels.Auth;

public class GoogleLoginInput
{
    [Required]
    public string IdToken { get; set; } = null!;
}