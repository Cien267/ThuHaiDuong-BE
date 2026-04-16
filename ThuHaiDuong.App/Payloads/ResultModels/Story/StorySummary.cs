using System.Linq.Expressions;
using ThuHaiDuong.Application.Payloads.ResultModels.Common;

namespace ThuHaiDuong.Application.Payloads.ResultModels.User.Story;

public class StorySummary : DataResponseBase
{
    public string Title { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string AuthorName { get; set; } = null!;
    public string? AuthorSlug { get; set; }
    public string? CoverImageUrl { get; set; }
    public string Status { get; set; } = null!;
    public string StoryType { get; set; } = null!;
    public string? ReleaseSchedule { get; set; }
    public int TotalChapters { get; set; }
    public long TotalViews { get; set; }
    public decimal AverageRating { get; set; }
    public int RatingCount { get; set; }
    public DateTime? LastChapterAt { get; set; }
 
    public List<string> CategoryNames { get; set; } = [];
 
    public static Expression<Func<Domain.Entities.Story, StorySummary>> FromStory =>
        s => new StorySummary
        {
            Id              = s.Id,
            Title           = s.Title,
            Slug            = s.Slug,
            AuthorName      = s.AuthorName,
            AuthorSlug      = s.Author.Slug,
            CoverImageUrl   = s.CoverImageUrl,
            Status          = s.Status,
            StoryType       = s.StoryType,
            ReleaseSchedule = s.ReleaseSchedule,
            TotalChapters   = s.TotalChapters,
            TotalViews      = s.TotalViews,
            AverageRating   = s.AverageRating,
            RatingCount     = s.RatingCount,
            LastChapterAt   = s.LastChapterAt,
            CreatedAt       = s.CreatedAt,
            CategoryNames   = s.StoryCategories
                .Where(sc => sc.Category.IsActive)
                .Select(sc => sc.Category.Name)
                .ToList(),
        };
}