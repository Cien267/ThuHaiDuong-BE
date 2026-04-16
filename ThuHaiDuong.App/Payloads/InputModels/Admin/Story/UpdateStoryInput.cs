using System.ComponentModel.DataAnnotations;

namespace ThuHaiDuong.Application.Payloads.InputModels.Admin.Story;

public class UpdateStoryInput
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
 
    public string StoryType { get; set; } = ThuHaiDuong.Shared.Constants.StoryType.Completed;
    public string? ReleaseSchedule { get; set; }
    public DateTime? NextChapterAt { get; set; }
 
    public List<Guid> CategoryIds { get; set; } = [];
    public List<Guid> TagIds { get; set; } = [];
}