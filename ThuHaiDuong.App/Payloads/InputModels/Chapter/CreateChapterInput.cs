using System.ComponentModel.DataAnnotations;
using ThuHaiDuong.Shared.Constants;

namespace ThuHaiDuong.Application.Payloads.InputModels.Chapter;

public class CreateChapterInput
{
    public Guid StoryId { get; set; }
 
    public int? ChapterNumber { get; set; }
 
    [Required, MaxLength(300)]
    public string Title { get; set; } = null!;
 
    [Required]
    public string Content { get; set; } = null!;
 
    public bool IsVip { get; set; } = false;
 
    public string Status { get; set; } = ChapterStatus.Draft;
}