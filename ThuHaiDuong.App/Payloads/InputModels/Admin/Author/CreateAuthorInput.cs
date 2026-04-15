using System.ComponentModel.DataAnnotations;

namespace ThuHaiDuong.Application.Payloads.InputModels.Admin.Author;

public class CreateAuthorInput
{
    [Required, MaxLength(200)]
    public string Name { get; set; } = null!;
 
    [MaxLength(250)]
    public string? Slug { get; set; }
 
    [MaxLength(200)]
    public string? PenName { get; set; }
 
    [MaxLength(10)]
    public string? Country { get; set; }        // "CN" | "VN" | "KR" | "JP" ...
 
    [MaxLength(2000)]
    public string? Description { get; set; }
 
    [MaxLength(500)]
    public string? AvatarUrl { get; set; }
}