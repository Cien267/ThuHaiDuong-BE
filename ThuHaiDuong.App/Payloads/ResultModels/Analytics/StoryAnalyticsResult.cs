namespace ThuHaiDuong.Application.Payloads.ResultModels.Analytics;

public class StoryAnalyticsResult
{
    public Guid StoryId { get; set; }
    public string Title { get; set; } = null!;
    public long TotalViews { get; set; }
    public decimal AverageRating { get; set; }
    public int RatingCount { get; set; }
    public int BookmarkCount { get; set; }
    public int CommentCount { get; set; }
 
    // View theo ngày (30 ngày gần nhất)
    public List<DailyStoryViewResult> DailyViews { get; set; } = [];
 
    // Chapter hot nhất của truyện này
    public List<ChapterRankingItem> TopChapters { get; set; } = [];
}