using System.Linq.Expressions;
using ThuHaiDuong.Application.Payloads.ResultModels.Common;

namespace ThuHaiDuong.Application.Payloads.ResultModels.User.Chapter;

public class ChapterListItem : DataResponseBase
{
    public int ChapterNumber { get; set; }
    public string Title { get; set; } = null!;
    public string Status { get; set; } = null!;
    public bool IsVip { get; set; }
    public int WordCount { get; set; }
    public long ViewCount { get; set; }
    public DateTime? PublishedAt { get; set; }
 
    public static Expression<Func<Domain.Entities.Chapter, ChapterListItem>> FromChapter =>
        c => new ChapterListItem
        {
            Id            = c.Id,
            ChapterNumber = c.ChapterNumber,
            Title         = c.Title,
            Status        = c.Status,
            IsVip         = c.IsVip,
            WordCount     = c.WordCount,
            ViewCount     = c.ViewCount,
            PublishedAt   = c.PublishedAt,
            CreatedAt     = c.CreatedAt,
            UpdatedAt     = c.UpdatedAt,
        };
}