namespace ThuHaiDuong.Shared.Constants;

public static class CacheKeys
{
    // ── STORY ─────────────────────────────────────────────────────────────────
 
    // Prefix — dùng để invalidate tất cả cache liên quan story
    public const string StoryPrefix = "story:";
 
    // Chi tiết truyện theo slug (trang đọc truyện — hot nhất)
    // TTL: 10 phút
    public static string StoryDetail(string slug)
        => $"story:detail:{slug}";
 
    // Top stories theo period — trang chủ
    // TTL: 5 phút (thay đổi thường xuyên hơn)
    public static string TopStories(string period, int page, int pageSize)
        => $"story:top:{period}:p{page}:s{pageSize}";
 
    // Story admin result — dùng khi admin xem chi tiết
    // TTL: 5 phút
    public static string StoryAdmin(Guid storyId)
        => $"story:admin:{storyId}";
 
    // ── CATEGORY ──────────────────────────────────────────────────────────────
 
    public const string CategoryPrefix = "category:";
 
    // Cây danh mục — ít thay đổi, TTL dài hơn
    // TTL: 30 phút
    public const string CategoryTree = "category:tree";
 
    // Chi tiết 1 category theo slug
    // TTL: 30 phút
    public static string CategoryBySlug(string slug)
        => $"category:slug:{slug}";
 
    // ── TOP STORIES (Analytics) ────────────────────────────────────────────────
 
    public const string AnalyticsPrefix = "analytics:";
 
    // Top stories từ analytics (giống StoryTop nhưng nguồn khác)
    public static string AnalyticsTopStories(string period, int page, int pageSize)
        => $"analytics:top:{period}:p{page}:s{pageSize}";
}