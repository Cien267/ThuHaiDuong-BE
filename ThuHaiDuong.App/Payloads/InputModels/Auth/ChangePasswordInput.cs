using System.ComponentModel.DataAnnotations;

namespace ThuHaiDuong.Application.Payloads.InputModels.Auth
{
    public class ChangePasswordInput
    {
        [Required]
        public string CurrentPassword { get; set; } = null!;
 
        [Required, MinLength(6), MaxLength(256)]
        public string NewPassword { get; set; } = null!;
    }
}
