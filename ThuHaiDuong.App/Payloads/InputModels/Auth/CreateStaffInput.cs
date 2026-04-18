using System.ComponentModel.DataAnnotations;

namespace ThuHaiDuong.Application.Payloads.InputModels.Auth;

public class CreateStaffInput
{
    [Required, MaxLength(100)]
    public string UserName { get; set; } = null!;
 
    [Required, MaxLength(256), EmailAddress]
    public string Email { get; set; } = null!;
 
    [Required, MinLength(6), MaxLength(256)]
    public string Password { get; set; } = null!;
 
    [MaxLength(200)]
    public string? FullName { get; set; }
 
    // "Contributor" | "Admin"
    [Required]
    public string Role { get; set; } = "Contributor";
}