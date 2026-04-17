using ThuHaiDuong.Application.Payloads.InputModels.Common;

namespace ThuHaiDuong.Application.Payloads.InputModels.Affiliate;

public class AffiliateClickReportQuery : PaginationParams
{
    public Guid? AffiliateLinkId { get; set; }
    public Guid? StoryId { get; set; }
    public Guid? ChapterId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}