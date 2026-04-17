using System.Linq.Expressions;
using ThuHaiDuong.Application.Payloads.ResultModels.Common;
using ThuHaiDuong.Domain.Entities;

namespace ThuHaiDuong.Application.Payloads.ResultModels.Affiliate;

public class AffiliateClickResult : DataResponseBase
{
    public Guid AffiliateLinkId { get; set; }
    public string LinkName { get; set; } = null!;
    public string TrackingCode { get; set; } = null!;
    public Guid? UserId { get; set; }
    public string? UserName { get; set; }
    public Guid? ChapterId { get; set; }
    public string? ChapterTitle { get; set; }
    public Guid? StoryId { get; set; }
    public string? StoryTitle { get; set; }
    public string? IpAddress { get; set; }
    public string? Referrer { get; set; }
    public DateTime ClickedAt { get; set; }
 
    public static Expression<Func<AffiliateClick, AffiliateClickResult>> FromClick =>
        c => new AffiliateClickResult
        {
            Id              = c.Id,
            AffiliateLinkId = c.AffiliateLinkId,
            LinkName        = c.AffiliateLink.Name,
            TrackingCode    = c.AffiliateLink.TrackingCode,
            UserId          = c.UserId,
            UserName        = c.User != null ? c.User.UserName : null,
            ChapterId       = c.ChapterId,
            ChapterTitle    = c.Chapter != null
                ? $"Chương {c.Chapter.ChapterNumber}: {c.Chapter.Title}"
                : null,
            StoryId         = c.Chapter != null ? c.Chapter.StoryId : null,
            StoryTitle      = c.Chapter != null ? c.Chapter.Story.Title : null,
            IpAddress       = c.IpAddress,
            Referrer        = c.Referrer,
            ClickedAt       = c.ClickedAt,
        };
}