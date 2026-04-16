using System.Linq.Expressions;
using ThuHaiDuong.Application.Payloads.ResultModels.Common;
using ThuHaiDuong.Application.Payloads.ResultModels.User.Bookmark;

namespace ThuHaiDuong.Application.Payloads.ResultModels.Admin.Bookmark;

public class BookmarkResult : DataResponseBase
{
    public Guid StoryId { get; set; }
    public string StoryTitle { get; set; } = null!;
    public string StorySlug { get; set; } = null!;
    public string? StoryCoverImageUrl { get; set; }
    public string StoryStatus { get; set; } = null!;
    public int TotalChapters { get; set; }
    public DateTime? LastChapterAt { get; set; }
    public DateTime BookmarkedAt { get; set; }
 
    // Tiếp tục đọc từ chỗ dở — null nếu chưa đọc chapter nào
    public ChapterProgressItem? LastReadChapter { get; set; }
 
    public static Expression<Func<Domain.Entities.Bookmark, BookmarkResult>> FromBookmark =>
        b => new BookmarkResult
        {
            StoryId            = b.StoryId,
            StoryTitle         = b.Story.Title,
            StorySlug          = b.Story.Slug,
            StoryCoverImageUrl = b.Story.CoverImageUrl,
            StoryStatus        = b.Story.Status,
            TotalChapters      = b.Story.TotalChapters,
            LastChapterAt      = b.Story.LastChapterAt,
            BookmarkedAt       = b.CreatedAt,
            LastReadChapter    = null,
        };
}