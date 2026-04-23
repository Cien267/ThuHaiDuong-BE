using System.ComponentModel.DataAnnotations;

namespace ThuHaiDuong.Application.Payloads.InputModels.User;

public class UpdateUsernameInput
{
    [Required, MaxLength(100)]
    public string UserName { get; set; } = null!;
}