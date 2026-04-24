using ThuHaiDuong.Application.InterfaceService;
using ThuHaiDuong.Application.Payloads.InputModels.Story;
using ThuHaiDuong.Application.Payloads.ResultModels.Story;
using ThuHaiDuong.Application.Payloads.ResultModels.User.Story;
using ThuHaiDuong.Shared.Constants;
using ThuHaiDuong.Shared.Models;

namespace ThuHaiDuong.Application.ImplementService;

public class CachedStoryService : IStoryService
{
    private readonly IStoryService  _inner;
    private readonly ICacheService  _cache;
 
    public CachedStoryService(IStoryService inner, ICacheService cache)
    {
        _inner = inner;
        _cache = cache;
    }
 
    // ── CLIENT — có cache ──────────────────────────────────────────────────────
 
    public async Task<PagedResult<StorySummary>> GetListAsync(StoryQuery query)
    {
        // Danh sách truyện có quá nhiều biến thể filter → không cache
        // Chỉ cache GetBySlug và Top stories
        return await _inner.GetListAsync(query);
    }
 
    public async Task<StoryDetail> GetBySlugAsync(string slug)
    {
        var key    = CacheKeys.StoryDetail(slug);
        var cached = await _cache.GetAsync<StoryDetail>(key);
 
        if (cached != null) return cached;
 
        var result = await _inner.GetBySlugAsync(slug);
        await _cache.SetAsync(key, result, CacheTTL.StoryDetail);
 
        return result;
    }
 
    // ── ADMIN — không cache (admin cần data fresh) ─────────────────────────────
 
    public async Task<PagedResult<StoryResult>> GetListAdminAsync(StoryQuery query)
        => await _inner.GetListAdminAsync(query);
 
    public async Task<StoryResult> GetByIdAdminAsync(Guid id)
        => await _inner.GetByIdAdminAsync(id);
 
    // ── WRITE OPERATIONS — gọi inner rồi invalidate ───────────────────────────
 
    public async Task<StoryResult> CreateAsync(
        CreateStoryInput input, Guid uploadedByUserId)
    {
        var result = await _inner.CreateAsync(input, uploadedByUserId);
 
        // Story mới → invalidate top stories (có thể ảnh hưởng ranking)
        await InvalidateTopStoriesAsync();
 
        return result;
    }
 
    public async Task<StoryResult> UpdateAsync(
        Guid id, UpdateStoryInput input, Guid requestUserId, string requestUserRole)
    {
        var result = await _inner.UpdateAsync(id, input, requestUserId, requestUserRole);
 
        // Lấy slug để invalidate đúng key
        await InvalidateStoryAsync(result.Slug);
 
        return result;
    }
 
    public async Task SubmitForReviewAsync(
        Guid id, Guid requestUserId, string requestUserRole)
    {
        await _inner.SubmitForReviewAsync(id, requestUserId, requestUserRole);
        // Submit review không thay đổi nội dung hiển thị client → không invalidate
    }
 
    public async Task ReviewAsync(Guid id, ReviewStoryInput input)
    {
        await _inner.ReviewAsync(id, input);
        // Approve/Reject không thay đổi nội dung client → không invalidate
    }
 
    public async Task UpdateStatusAsync(Guid id, UpdateStoryStatusInput input)
    {
        await _inner.UpdateStatusAsync(id, input);
 
        // Status thay đổi (Publishing/Paused/Completed) ảnh hưởng
        // những gì client thấy → phải invalidate
        var story = await _inner.GetByIdAdminAsync(id);
        await InvalidateStoryAsync(story.Slug);
        await InvalidateTopStoriesAsync();
    }
 
    public async Task DeleteAsync(
        Guid id, Guid requestUserId, string requestUserRole)
    {
        var story = await _inner.GetByIdAdminAsync(id);
        await _inner.DeleteAsync(id, requestUserId, requestUserRole);
 
        await InvalidateStoryAsync(story.Slug);
        await InvalidateTopStoriesAsync();
    }
 
    // ── INVALIDATION HELPERS ──────────────────────────────────────────────────
 
    private async Task InvalidateStoryAsync(string slug)
    {
        await _cache.RemoveAsync(CacheKeys.StoryDetail(slug));
    }
 
    private async Task InvalidateTopStoriesAsync()
    {
        // Xóa tất cả variant của top stories (mọi period, mọi page)
        await _cache.RemoveByPrefixAsync("analytics:top:");
        await _cache.RemoveByPrefixAsync("story:top:");
    }
}