namespace ThuHaiDuong.Application.Payloads.ResultModels.Analytics;

public class DailyStoryViewResult
{
    public DateOnly Date { get; set; }
    public int ViewCount { get; set; }
    public int UniqueVisitors { get; set; }
}