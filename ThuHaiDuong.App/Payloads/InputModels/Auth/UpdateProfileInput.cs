using System.ComponentModel.DataAnnotations;

namespace ThuHaiDuong.Application.Payloads.InputModels.Auth
{
    public class UpdateProfileInput
    {

        [Required(ErrorMessage = "Full name is required")]
        public string FullName { get; set; }
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "Email is required")]
        public string Email { get; set; }

        public string? PhoneNumber { get; set; }
    }
}
