using ThuHaiDuong.Application.InterfaceService;
using ThuHaiDuong.Application.Payloads.InputModels.Category;
using ThuHaiDuong.Application.Payloads.ResultModels.Category;
using ThuHaiDuong.Application.Payloads.ResultModels.User.Category;
using ThuHaiDuong.Shared.Constants;
using ThuHaiDuong.Shared.Models;

namespace ThuHaiDuong.Application.ImplementService;

public class CachedCategoryService : ICategoryService
{
    private readonly ICategoryService _inner;
    private readonly ICacheService    _cache;
 
    public CachedCategoryService(ICategoryService inner, ICacheService cache)
    {
        _inner = inner;
        _cache = cache;
    }
 
    // ── CLIENT — có cache ──────────────────────────────────────────────────────
 
    public async Task<List<CategorySummary>> GetTreeAsync()
    {
        var cached = await _cache.GetAsync<List<CategorySummary>>(CacheKeys.CategoryTree);
        if (cached != null) return cached;
 
        var result = await _inner.GetTreeAsync();
        await _cache.SetAsync(CacheKeys.CategoryTree, result, CacheTTL.CategoryTree);
 
        return result;
    }
 
    public async Task<CategorySummary> GetBySlugAsync(string slug)
    {
        var key    = CacheKeys.CategoryBySlug(slug);
        var cached = await _cache.GetAsync<CategorySummary>(key);
        if (cached != null) return cached;
 
        var result = await _inner.GetBySlugAsync(slug);
        await _cache.SetAsync(key, result, CacheTTL.CategoryDetail);
 
        return result;
    }
 
    // ── ADMIN — không cache ────────────────────────────────────────────────────
 
    public async Task<PagedResult<CategoryResult>> GetListAsync(CategoryQuery query)
        => await _inner.GetListAsync(query);
 
    public async Task<CategoryResult> GetByIdAsync(Guid id)
        => await _inner.GetByIdAsync(id);
 
    // ── WRITE — invalidate toàn bộ category cache ─────────────────────────────
    // Đơn giản hơn: category ít, xóa hết prefix khi có bất kỳ thay đổi nào
 
    public async Task<CategoryResult> CreateAsync(CreateCategoryInput input)
    {
        var result = await _inner.CreateAsync(input);
        await _cache.RemoveByPrefixAsync(CacheKeys.CategoryPrefix);
        return result;
    }
 
    public async Task<CategoryResult> UpdateAsync(Guid id, UpdateCategoryInput input)
    {
        var result = await _inner.UpdateAsync(id, input);
        await _cache.RemoveByPrefixAsync(CacheKeys.CategoryPrefix);
        return result;
    }
 
    public async Task DeleteAsync(Guid id)
    {
        await _inner.DeleteAsync(id);
        await _cache.RemoveByPrefixAsync(CacheKeys.CategoryPrefix);
    }
}