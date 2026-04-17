using System.ComponentModel.DataAnnotations;

namespace ThuHaiDuong.Application.Payloads.InputModels.Analytics;

public class TrackChapterViewInput
{
    [Required]
    public Guid ChapterId { get; set; }
 
    [Required]
    public Guid StoryId { get; set; }
 
    // SessionId từ cookie anonymous (frontend tự generate và lưu localStorage)
    [MaxLength(100)]
    public string? SessionId { get; set; }
}