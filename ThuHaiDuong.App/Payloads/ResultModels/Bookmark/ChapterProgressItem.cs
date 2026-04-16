namespace ThuHaiDuong.Application.Payloads.ResultModels.User.Bookmark;

public class ChapterProgressItem
{
    public Guid ChapterId { get; set; }
    public int ChapterNumber { get; set; }
    public string ChapterTitle { get; set; } = null!;
    public DateTime LastReadAt { get; set; }
}