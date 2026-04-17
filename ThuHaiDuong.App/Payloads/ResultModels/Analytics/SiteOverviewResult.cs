namespace ThuHaiDuong.Application.Payloads.ResultModels.Analytics;

public class SiteOverviewResult
{
    // Tính trong khoảng thời gian query
    public int TotalChapterViews { get; set; }
    public int UniqueVisitors { get; set; }
    public int NewUsers { get; set; }
    public int NewComments { get; set; }
    public int NewRatings { get; set; }
    public int NewBookmarks { get; set; }
 
    // All-time counters (luôn trả về bất kể filter ngày)
    public int TotalStories { get; set; }
    public int TotalChapters { get; set; }
    public int TotalUsers { get; set; }
}