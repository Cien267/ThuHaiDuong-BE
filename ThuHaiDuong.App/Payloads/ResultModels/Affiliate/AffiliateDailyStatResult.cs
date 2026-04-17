namespace ThuHaiDuong.Application.Payloads.ResultModels.Affiliate;

public class AffiliateDailyStatResult
{
    public DateOnly Date { get; set; }
    public int TotalClicks { get; set; }
    public int UniqueIps { get; set; }
}