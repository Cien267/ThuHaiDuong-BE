using System.ComponentModel.DataAnnotations;

namespace ThuHaiDuong.Application.Payloads.InputModels.Admin.Tag;

public class CreateTagInput
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = null!;
 
    [MaxLength(120)]
    public string? Slug { get; set; }
}