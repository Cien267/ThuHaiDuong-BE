namespace ThuHaiDuong.Application.InterfaceService;

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T value, TimeSpan? expiry = null);
    Task RemoveAsync(string key);
 
    // Xóa nhiều key theo pattern prefix — dùng khi invalidate group
    // VD: RemoveByPrefixAsync("story:") → xóa tất cả key bắt đầu bằng "story:"
    Task RemoveByPrefixAsync(string prefix);
}