namespace ThuHaiDuong.Application.Payloads.ResultModels.User.Chapter;

public class ChapterReadResult
{
    public Guid Id { get; set; }
    public int ChapterNumber { get; set; }
    public string Title { get; set; } = null!;
    public string Content { get; set; } = null!;    // HTML
    public bool IsVip { get; set; }
    public int WordCount { get; set; }
    public DateTime? PublishedAt { get; set; }
 
    public Guid StoryId { get; set; }
    public string StoryTitle { get; set; } = null!;
    public string StorySlug { get; set; } = null!;
 
    public ChapterNavItem? PrevChapter { get; set; }
    public ChapterNavItem? NextChapter { get; set; }
}