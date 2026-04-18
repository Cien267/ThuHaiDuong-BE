using System.ComponentModel.DataAnnotations;

namespace ThuHaiDuong.Application.Payloads.InputModels.Auth
{
    public class LoginInput
    {
        [Required, MaxLength(256)]
        public string Email { get; set; } = null!;
 
        [Required, MaxLength(256)]
        public string Password { get; set; } = null!;
    }
}
