namespace ThuHaiDuong.Application.Payloads.ResultModels.Affiliate;

public class AffiliateDisplayResult
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Placement { get; set; } = null!;
 
    // URL để gọi redirect: /go/{TrackingCode}
    public string RedirectUrl { get; set; } = null!;
}