using System.ComponentModel.DataAnnotations;

namespace ThuHaiDuong.Application.Payloads.InputModels.User
{
    public class UpdateUserInput
    {
        public Guid Id { get; set; }
        public string UserName { get; set; }
        [Required(ErrorMessage = "Full name is required")] 
        public string FullName { get; set; }
        [Required(ErrorMessage = "Email is required")]
        public string Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? CurrentPassword { get; set; }
        public string? NewPassword { get; set; }
        public string? ConfirmPassword { get; set; }
    }
}
