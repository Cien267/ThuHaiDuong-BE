namespace ThuHaiDuong.Application.Payloads.ResultModels.User.Tag;

public class TagSummaryItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Slug { get; set; } = null!;
}