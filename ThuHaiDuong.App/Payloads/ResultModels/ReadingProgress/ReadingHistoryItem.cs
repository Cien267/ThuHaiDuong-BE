using System.Linq.Expressions;
using ThuHaiDuong.Domain.Entities;

namespace ThuHaiDuong.Application.Payloads.ResultModels.ReadingProgress;

public class ReadingHistoryItem
{
    public Guid ChapterId { get; set; }
    public int ChapterNumber { get; set; }
    public string ChapterTitle { get; set; } = null!;
    public bool IsVip { get; set; }
 
    public Guid StoryId { get; set; }
    public string StoryTitle { get; set; } = null!;
    public string StorySlug { get; set; } = null!;
    public string? StoryCoverImageUrl { get; set; }
 
    public DateTime ReadAt { get; set; }
 
    public static Expression<Func<ReadingHistory, ReadingHistoryItem>> FromHistory =>
        h => new ReadingHistoryItem
        {
            ChapterId       = h.ChapterId,
            ChapterNumber   = h.Chapter.ChapterNumber,
            ChapterTitle    = h.Chapter.Title,
            IsVip           = h.Chapter.IsVip,
            StoryId         = h.StoryId,
            StoryTitle      = h.Story.Title,
            StorySlug       = h.Story.Slug,
            StoryCoverImageUrl = h.Story.CoverImageUrl,
            ReadAt          = h.ReadAt,
        };
}