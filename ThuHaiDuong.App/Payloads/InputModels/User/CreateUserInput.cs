using System.ComponentModel.DataAnnotations;

namespace ThuHaiDuong.Application.Payloads.InputModels.User
{
    public class CreateUserInput
    {
        public string UserName { get; set; }
        [Required(ErrorMessage = "Password is required")] 
        public string Password { get; set; }
        [Required(ErrorMessage = "Full name is required")] 
        public string FullName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        public string Email { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? PhoneNumber { get; set; }
        public Guid? BrokerageId {get; set;}
    }
}
