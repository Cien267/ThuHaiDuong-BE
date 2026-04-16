using System.Linq.Expressions;
using ThuHaiDuong.Application.Payloads.ResultModels.Common;

namespace ThuHaiDuong.Application.Payloads.ResultModels.Chapter;

public class ChapterResult : DataResponseBase
{
    public Guid StoryId { get; set; }
    public string StoryTitle { get; set; } = null!;
    public int ChapterNumber { get; set; }
    public string Title { get; set; } = null!;
    public string Content { get; set; } = null!;
    public string Status { get; set; } = null!;
    public bool IsVip { get; set; }
    public int WordCount { get; set; }
    public long ViewCount { get; set; }
    public DateTime? PublishedAt { get; set; }
 
    public static Expression<Func<Domain.Entities.Chapter, ChapterResult>> FromChapter =>
        c => new ChapterResult
        {
            Id            = c.Id,
            StoryId       = c.StoryId,
            StoryTitle    = c.Story.Title,
            ChapterNumber = c.ChapterNumber,
            Title         = c.Title,
            Content       = c.Content,
            Status        = c.Status,
            IsVip         = c.IsVip,
            WordCount     = c.WordCount,
            ViewCount     = c.ViewCount,
            PublishedAt   = c.PublishedAt,
            CreatedAt     = c.CreatedAt,
            UpdatedAt     = c.UpdatedAt,
        };
}