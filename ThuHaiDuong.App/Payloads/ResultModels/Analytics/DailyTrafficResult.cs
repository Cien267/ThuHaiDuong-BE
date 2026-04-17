namespace ThuHaiDuong.Application.Payloads.ResultModels.Analytics;

public class DailyTrafficResult
{
    public DateOnly Date { get; set; }
    public int ChapterViews { get; set; }
    public int UniqueVisitors { get; set; }   // distinct SessionId + IpAddress
    public int NewUsers { get; set; }
    public int NewComments { get; set; }
    public int NewRatings { get; set; }
}