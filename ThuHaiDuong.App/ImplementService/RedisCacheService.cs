using System.Text.Json;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using ThuHaiDuong.Application.InterfaceService;
using IDatabase = Microsoft.EntityFrameworkCore.Storage.IDatabase;

namespace ThuHaiDuong.Application.ImplementService;

public class RedisCacheService : ICacheService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly StackExchange.Redis.IDatabase _db;
    private readonly ILogger<RedisCacheService> _logger;
 
    // TTL mặc định nếu không truyền expiry
    private static readonly TimeSpan DefaultExpiry = TimeSpan.FromMinutes(10);
 
    public RedisCacheService(
        IConnectionMultiplexer redis,
        ILogger<RedisCacheService> logger)
    {
        _redis  = redis;
        _db     = redis.GetDatabase();
        _logger = logger;
    }
 
    public async Task<T?> GetAsync<T>(string key)
    {
        try
        {
            var value = await _db.StringGetAsync(key);
            if (!value.HasValue) return default;
 
            return JsonSerializer.Deserialize<T>(value!);
        }
        catch (Exception ex)
        {
            // Cache miss không nên crash app — log và trả về default
            _logger.LogWarning(ex, "Cache GET failed for key: {Key}", key);
            return default;
        }
    }
 
    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        try
        {
            var json = JsonSerializer.Serialize(value);
            await _db.StringSetAsync(key, json, expiry ?? DefaultExpiry);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cache SET failed for key: {Key}", key);
        }
    }
 
    public async Task RemoveAsync(string key)
    {
        try
        {
            await _db.KeyDeleteAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cache REMOVE failed for key: {Key}", key);
        }
    }
 
    public async Task RemoveByPrefixAsync(string prefix)
    {
        try
        {
            // SCAN thay vì KEYS — không block Redis server
            var server  = _redis.GetServers().First();
            var keys    = server.KeysAsync(pattern: $"{prefix}*");
 
            await foreach (var key in keys)
                await _db.KeyDeleteAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cache REMOVE_BY_PREFIX failed for prefix: {Prefix}", prefix);
        }
    }
}