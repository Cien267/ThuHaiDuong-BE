namespace ThuHaiDuong.Application.Payloads.ResultModels.User.Chapter;

public class ChapterNavItem
{
    public Guid Id { get; set; }
    public int ChapterNumber { get; set; }
    public string Title { get; set; } = null!;
    public bool IsVip { get; set; }
}