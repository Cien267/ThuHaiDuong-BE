using System.Linq.Expressions;
using ThuHaiDuong.Application.Payloads.ResultModels.Common;
using ThuHaiDuong.Application.Payloads.ResultModels.User.Category;
using ThuHaiDuong.Application.Payloads.ResultModels.User.Tag;

namespace ThuHaiDuong.Application.Payloads.ResultModels.Admin.Story;

public class StoryResult : DataResponseBase
{
    public string Title { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string? Description { get; set; }
    public string? CoverImageUrl { get; set; }
    public string? SourceUrl { get; set; }
    public string Status { get; set; } = null!;
    public string StoryType { get; set; } = null!;
    public string? ReleaseSchedule { get; set; }
    public DateTime? NextChapterAt { get; set; }
    public string ContentSource { get; set; } = null!;
    public string? RejectionReason { get; set; }
    public int TotalChapters { get; set; }
    public long TotalViews { get; set; }
    public decimal AverageRating { get; set; }
    public int RatingCount { get; set; }
    public DateTime? LastChapterAt { get; set; }
 
    // Author
    public Guid AuthorId { get; set; }
    public string AuthorName { get; set; } = null!;
 
    // Uploader
    public Guid? UploadedByUserId { get; set; }
    public string? UploadedByUserName { get; set; }
 
    // Taxonomy
    public List<CategorySummaryItem> Categories { get; set; } = [];
    public List<TagSummaryItem> Tags { get; set; } = [];
 
    public static Expression<Func<Domain.Entities.Story, StoryResult>> FromStory =>
        s => new StoryResult
        {
            Id                  = s.Id,
            Title               = s.Title,
            Slug                = s.Slug,
            Description         = s.Description,
            CoverImageUrl       = s.CoverImageUrl,
            SourceUrl           = s.SourceUrl,
            Status              = s.Status,
            StoryType           = s.StoryType,
            ReleaseSchedule     = s.ReleaseSchedule,
            NextChapterAt       = s.NextChapterAt,
            ContentSource       = s.ContentSource,
            RejectionReason     = s.RejectionReason,
            TotalChapters       = s.TotalChapters,
            TotalViews          = s.TotalViews,
            AverageRating       = s.AverageRating,
            RatingCount         = s.RatingCount,
            LastChapterAt       = s.LastChapterAt,
            CreatedAt           = s.CreatedAt,
            UpdatedAt           = s.UpdatedAt,
            AuthorId            = s.AuthorId,
            AuthorName          = s.AuthorName,
            UploadedByUserId    = s.UploadedByUserId,
            UploadedByUserName  = s.UploadedByUser != null ? s.UploadedByUser.UserName : null,
            Categories          = s.StoryCategories
                .Select(sc => new CategorySummaryItem
                {
                    Id   = sc.CategoryId,
                    Name = sc.Category.Name,
                    Slug = sc.Category.Slug,
                }).ToList(),
            Tags                = s.StoryTags
                .Select(st => new TagSummaryItem
                {
                    Id   = st.TagId,
                    Name = st.Tag.Name,
                    Slug = st.Tag.Slug,
                }).ToList(),
        };
}