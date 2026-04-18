namespace ThuHaiDuong.Application.Payloads.ResultModels.ReadingProgress;

public class ReadingProgressResult
{
    public Guid StoryId { get; set; }
    public string StoryTitle { get; set; } = null!;
    public string StorySlug { get; set; } = null!;
    public int TotalPublishedChapters { get; set; }
 
    // Chapter đang đọc dở
    public Guid LastChapterId { get; set; }
    public int LastChapterNumber { get; set; }
    public string LastChapterTitle { get; set; } = null!;
    public DateTime LastReadAt { get; set; }
 
    // Số chapter đã đọc (từ ReadingHistory)
    public int ReadChapterCount { get; set; }
 
    // % tiến độ: ReadChapterCount / TotalPublishedChapters
    public decimal ProgressPercent => TotalPublishedChapters > 0
        ? Math.Round((decimal)ReadChapterCount / TotalPublishedChapters * 100, 1)
        : 0;
}