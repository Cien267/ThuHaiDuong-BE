namespace ThuHaiDuong.Application.Payloads.ResultModels.Affiliate;

public class AffiliateLinkTargetItem
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string Slug { get; set; } = null!;
}