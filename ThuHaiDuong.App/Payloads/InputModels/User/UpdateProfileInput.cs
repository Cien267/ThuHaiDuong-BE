using System.ComponentModel.DataAnnotations;

namespace ThuHaiDuong.Application.Payloads.InputModels.User;

public class UpdateProfileInput
{
    [MaxLength(200)]
    public string? FullName { get; set; }
 
    [MaxLength(20)]
    public string? PhoneNumber { get; set; }
}