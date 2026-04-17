using ThuHaiDuong.Application.Payloads.InputModels.Affiliate;
using ThuHaiDuong.Application.Payloads.ResultModels.Affiliate;
using ThuHaiDuong.Shared.Models;

namespace ThuHaiDuong.Application.InterfaceService;

public interface IAffiliateService
{
    // ── CLIENT ────────────────────────────────────────────────────────────────
 
    // Lấy danh sách link hiển thị khi đọc chapter
    // Mỗi Placement trả về 1 link phù hợp nhất
    Task<List<AffiliateDisplayResult>> GetDisplayLinksAsync(Guid storyId, Guid chapterId);
 
    // Hot path: redirect /go/{code}
    // Ghi click (nếu không spam) → trả về TargetUrl để controller redirect
    Task<string> TrackAndGetTargetUrlAsync(
        string trackingCode,
        Guid? userId,
        Guid? chapterId,
        string? ipAddress,
        string? userAgent,
        string? referrer);
 
    // ── ADMIN ─────────────────────────────────────────────────────────────────
 
    Task<PagedResult<AffiliateLinkResult>> GetListAsync(AffiliateLinkQuery query);
    Task<AffiliateLinkResult> GetByIdAsync(Guid id);
    Task<AffiliateLinkResult> CreateAsync(CreateAffiliateLinkInput input);
    Task<AffiliateLinkResult> UpdateAsync(Guid id, UpdateAffiliateLinkInput input);
    Task DeleteAsync(Guid id);
 
    // Báo cáo
    Task<List<AffiliateDailyStatResult>> GetDailyStatsAsync(
        Guid? linkId, DateTime from, DateTime to);
 
    Task<List<AffiliateLinkStatResult>> GetLinkStatsAsync(
        DateTime? from, DateTime? to);
 
    Task<PagedResult<AffiliateClickResult>> GetClicksAsync(
        AffiliateClickReportQuery query);
}