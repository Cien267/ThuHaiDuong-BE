using System.Linq.Expressions;
using ThuHaiDuong.Application.Payloads.ResultModels.Common;
using ThuHaiDuong.Domain.Entities;

namespace ThuHaiDuong.Application.Payloads.ResultModels.Affiliate;

public class AffiliateLinkResult : DataResponseBase
{
    public string Name { get; set; } = null!;
    public string TargetUrl { get; set; } = null!;
    public string TrackingCode { get; set; } = null!;
 
    // URL redirect hoàn chỉnh để copy: /go/{TrackingCode}
    public string RedirectUrl { get; set; } = null!;
 
    public string Placement { get; set; } = null!;
    public int Priority { get; set; }
    public bool IsActive { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
 
    // Thống kê tổng
    public int TotalClicks { get; set; }
    public int TotalStories { get; set; }
    public int TotalChapters { get; set; }
 
 
    // Danh sách story/chapter đã gắn
    public List<AffiliateLinkTargetItem> Stories { get; set; } = [];
    public List<AffiliateLinkTargetItem> Chapters { get; set; } = [];
 
    public static Expression<Func<AffiliateLink, AffiliateLinkResult>> FromLink =>
        l => new AffiliateLinkResult
        {
            Id            = l.Id,
            Name          = l.Name,
            TargetUrl     = l.TargetUrl,
            TrackingCode  = l.TrackingCode,
            RedirectUrl   = $"/go/{l.TrackingCode}",
            Placement     = l.Placement,
            Priority      = l.Priority,
            IsActive      = l.IsActive,
            StartDate     = l.StartDate,
            EndDate       = l.EndDate,
            TotalClicks   = l.AffiliateClicks.Count,
            TotalStories  = l.AffiliateLinkStories.Count,
            TotalChapters = l.AffiliateLinkChapters.Count,
            CreatedAt     = l.CreatedAt,
            UpdatedAt     = l.UpdatedAt,
            Stories       = l.AffiliateLinkStories
                .Select(s => new AffiliateLinkTargetItem
                {
                    Id    = s.StoryId,
                    Title = s.Story.Title,
                    Slug  = s.Story.Slug,
                }).ToList(),
            Chapters      = l.AffiliateLinkChapters
                .Select(c => new AffiliateLinkTargetItem
                {
                    Id    = c.ChapterId,
                    Title = $"Chương {c.Chapter.ChapterNumber}: {c.Chapter.Title}",
                    Slug  = c.Chapter.StoryId.ToString(),
                }).ToList(),
        };
}