using System.ComponentModel.DataAnnotations;

namespace ThuHaiDuong.Application.Payloads.InputModels.Admin.Story;

public class CreateStoryInput
{
    [Required, MaxLength(300)]
    public string Title { get; set; } = null!;
 
    [MaxLength(350)]
    public string? Slug { get; set; }
 
    [Required]
    public Guid AuthorId { get; set; }
 
    [MaxLength(1000)]
    public string? SourceUrl { get; set; }
 
    public string? Description { get; set; }
 
    [MaxLength(500)]
    public string? CoverImageUrl { get; set; }
 
    // "Serial" | "Completed" — default Completed
    public string StoryType { get; set; } = ThuHaiDuong.Shared.Constants.StoryType.Completed;
 
    // Chỉ có ý nghĩa khi StoryType = Serial
    public string? ReleaseSchedule { get; set; }
    public DateTime? NextChapterAt { get; set; }
 
    // "Manual" | "Crawled" | "UGC"
    public string ContentSource { get; set; } = ThuHaiDuong.Shared.Constants.ContentSource.Manual;
 
    // Gắn category và tag ngay khi tạo
    public List<Guid> CategoryIds { get; set; } = [];
    public List<Guid> TagIds { get; set; } = [];
}