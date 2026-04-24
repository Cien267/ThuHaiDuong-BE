namespace ThuHaiDuong.Shared.Constants;

public static class CacheTTL
{
    // Story detail: user đọc, ít thay đổi trong ngắn hạn
    public static readonly TimeSpan StoryDetail = TimeSpan.FromMinutes(10);
 
    // Top stories: thay đổi thường xuyên hơn, TTL ngắn hơn
    public static readonly TimeSpan TopStories = TimeSpan.FromMinutes(5);
 
    // Admin views: admin cần thấy data fresh hơn
    public static readonly TimeSpan AdminView = TimeSpan.FromMinutes(5);
 
    // Category tree: rất ít thay đổi
    public static readonly TimeSpan CategoryTree = TimeSpan.FromMinutes(30);
 
    // Category detail
    public static readonly TimeSpan CategoryDetail = TimeSpan.FromMinutes(30);
}