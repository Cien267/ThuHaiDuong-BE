namespace ThuHaiDuong.Application.Payloads.ResultModels.User.Category;

public class CategorySummaryItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Slug { get; set; } = null!;
}