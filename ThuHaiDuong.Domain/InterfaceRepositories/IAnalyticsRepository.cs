using ThuHaiDuong.Domain.Entities;

namespace ThuHaiDuong.Domain.InterfaceRepositories;

public interface IAnalyticsRepository
{
    // Ghi view — không dedup, ghi tất cả (Q3: C)
    Task RecordChapterViewAsync(ChapterView view);
 
    // Tăng ViewCount trực tiếp trên Chapter và TotalViews trên Story
    // Dùng ExecuteUpdateAsync — không load entity
    Task IncrementViewCountersAsync(Guid chapterId, Guid storyId);
 
    // ── BACKGROUND JOB ────────────────────────────────────────────────────────
 
    // Được gọi bởi background job lúc 00:00 UTC
    // Tính aggregate cho ngày hôm qua và upsert vào DailyStoryStats
    Task AggregateDailyStatsAsync(DateOnly date);
}