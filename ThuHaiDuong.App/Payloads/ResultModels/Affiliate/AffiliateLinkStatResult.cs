namespace ThuHaiDuong.Application.Payloads.ResultModels.Affiliate;

public class AffiliateLinkStatResult
{
    public Guid AffiliateLinkId { get; set; }
    public string LinkName { get; set; } = null!;
    public string TrackingCode { get; set; } = null!;
    public string Placement { get; set; } = null!;
    public int TotalClicks { get; set; }
    public int UniqueIps { get; set; }
    public DateTime? LastClickedAt { get; set; }
}