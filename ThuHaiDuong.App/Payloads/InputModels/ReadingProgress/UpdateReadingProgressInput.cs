using System.ComponentModel.DataAnnotations;

namespace ThuHaiDuong.Application.Payloads.InputModels.ReadingProgress;

public class UpdateReadingProgressInput
{
    [Required]
    public Guid StoryId { get; set; }
 
    [Required]
    public Guid ChapterId { get; set; }
 
    [Required]
    public int ChapterNumber { get; set; }
}