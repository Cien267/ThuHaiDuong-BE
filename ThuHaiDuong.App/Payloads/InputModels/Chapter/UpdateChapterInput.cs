using System.ComponentModel.DataAnnotations;

namespace ThuHaiDuong.Application.Payloads.InputModels.Chapter;

public class UpdateChapterInput
{
    [Required, MaxLength(300)]
    public string Title { get; set; } = null!;
 
    [Required]
    public string Content { get; set; } = null!;
 
    public bool IsVip { get; set; } = false;
}