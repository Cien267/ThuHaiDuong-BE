using ThuHaiDuong.Domain.Entities;

namespace ThuHaiDuong.Domain.InterfaceRepositories;

public interface IAffiliateRepository
{
    Task<bool> TrackingCodeExistsAsync(string code, Guid? excludeId = null);
 
    // Hot path: /go/{code} — lookup nhanh nhất có thể
    Task<AffiliateLink?> GetByTrackingCodeAsync(string trackingCode);
 
    // Chống spam: kiểm tra IP đã click link này trong vòng 1 giờ chưa
    Task<bool> IsSpamClickAsync(Guid linkId, string ipAddress);
 
    // Sync story/chapter targets
    Task SyncStoryTargetsAsync(Guid linkId, List<Guid> storyIds);
    Task SyncChapterTargetsAsync(Guid linkId, List<Guid> chapterIds);
 
    // Báo cáo click theo ngày
    Task<List<AffiliateClick>> GetDailyStatsAsync(
        Guid? linkId, DateTime from, DateTime to);
}