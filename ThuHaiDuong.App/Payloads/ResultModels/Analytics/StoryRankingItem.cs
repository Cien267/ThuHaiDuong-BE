namespace ThuHaiDuong.Application.Payloads.ResultModels.Analytics;

public class StoryRankingItem
{
    public Guid StoryId { get; set; }
    public string Title { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string? CoverImageUrl { get; set; }
    public string AuthorName { get; set; } = null!;
    public int ViewCount { get; set; }        // trong period được chọn
    public long TotalViews { get; set; }      // all-time (denormalized)
    public decimal AverageRating { get; set; }
    public int TotalChapters { get; set; }
    public List<string> CategoryNames { get; set; } = [];
}