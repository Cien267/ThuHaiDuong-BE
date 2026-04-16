using ThuHaiDuong.Application.Payloads.ResultModels.Common;
using ThuHaiDuong.Application.Payloads.ResultModels.User.Category;
using ThuHaiDuong.Application.Payloads.ResultModels.User.Chapter;
using ThuHaiDuong.Application.Payloads.ResultModels.User.Tag;

namespace ThuHaiDuong.Application.Payloads.ResultModels.User.Story;

public class StoryDetail : DataResponseBase
{
    public string Title { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string? Description { get; set; }
    public string? CoverImageUrl { get; set; }
    public string Status { get; set; } = null!;
    public string StoryType { get; set; } = null!;
    public string? ReleaseSchedule { get; set; }
    public DateTime? NextChapterAt { get; set; }
    public int TotalChapters { get; set; }
    public long TotalViews { get; set; }
    public decimal AverageRating { get; set; }
    public int RatingCount { get; set; }
    public DateTime? LastChapterAt { get; set; }
 
    // Author
    public Guid AuthorId { get; set; }
    public string AuthorName { get; set; } = null!;
    public string? AuthorSlug { get; set; }
    public string? AuthorAvatarUrl { get; set; }
 
    // Taxonomy
    public List<CategorySummaryItem> Categories { get; set; } = [];
    public List<TagSummaryItem> Tags { get; set; } = [];
 
    // Danh sách chương (chỉ số + tên, không có content)
    public List<ChapterSummaryItem> Chapters { get; set; } = [];
}