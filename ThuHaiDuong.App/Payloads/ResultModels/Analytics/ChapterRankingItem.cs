namespace ThuHaiDuong.Application.Payloads.ResultModels.Analytics;

public class ChapterRankingItem
{
    public Guid ChapterId { get; set; }
    public int ChapterNumber { get; set; }
    public string ChapterTitle { get; set; } = null!;
    public Guid StoryId { get; set; }
    public string StoryTitle { get; set; } = null!;
    public string StorySlug { get; set; } = null!;
    public long ViewCount { get; set; }
}