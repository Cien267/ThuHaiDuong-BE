using StackExchange.Redis;

namespace ThuHaiDuong.Middlewares;

/// <summary>
/// Rate limiting middleware dùng Redis sliding window counter.
/// Áp dụng cho các endpoint được cấu hình trong appsettings.json.
///
/// Sliding window: đếm số request trong X giây gần nhất (không phải reset theo phút cố định).
/// VD: window 60s, limit 5 → nếu request thứ 5 lúc 00:45
///     thì request tiếp theo chỉ được phép lúc 01:45 (không phải 01:00).
///
/// Key pattern: ratelimit:{endpoint_key}:{ip}
/// </summary>
public class RateLimitMiddleware
{
    private readonly RequestDelegate                _next;
    private readonly IConnectionMultiplexer         _redis;
    private readonly ILogger<RateLimitMiddleware>   _logger;
    private readonly List<RateLimitRule>            _rules;
 
    public RateLimitMiddleware(
        RequestDelegate              next,
        IConnectionMultiplexer       redis,
        ILogger<RateLimitMiddleware> logger,
        IConfiguration               config)
    {
        _next   = next;
        _redis  = redis;
        _logger = logger;
        _rules  = config
            .GetSection("RateLimit:Rules")
            .Get<List<RateLimitRule>>() ?? [];
    }
 
    public async Task Invoke(HttpContext context)
    {
        var path   = context.Request.Path.Value?.ToLower() ?? "";
        var method = context.Request.Method.ToUpper();
 
        // Tìm rule phù hợp với request hiện tại
        var rule = _rules.FirstOrDefault(r =>
            path.StartsWith(r.Path.ToLower()) &&
            (r.Method == "*" || r.Method.ToUpper() == method));
 
        if (rule == null)
        {
            await _next(context);
            return;
        }
 
        var ip  = GetClientIp(context);
        var key = $"ratelimit:{rule.Key}:{ip}";
 
        var (allowed, remaining, retryAfter) =
            await CheckRateLimitAsync(key, rule.Limit, rule.WindowSeconds);
 
        // Thêm headers thông tin rate limit vào response
        context.Response.Headers["X-RateLimit-Limit"]     = rule.Limit.ToString();
        context.Response.Headers["X-RateLimit-Remaining"] = remaining.ToString();
        context.Response.Headers["X-RateLimit-Window"]    = $"{rule.WindowSeconds}s";
 
        if (!allowed)
        {
            _logger.LogWarning(
                "Rate limit exceeded | Rule: {Rule} | IP: {Ip} | Path: {Path}",
                rule.Key, ip, path);
 
            context.Response.StatusCode  = StatusCodes.Status429TooManyRequests;
            context.Response.ContentType = "application/json";
 
            if (retryAfter > 0)
                context.Response.Headers["Retry-After"] = retryAfter.ToString();
 
            await context.Response.WriteAsJsonAsync(new
            {
                message    = rule.Message ?? "Too many requests. Please try again later.",
                statusCode = 429,
                retryAfterSeconds = retryAfter,
            });
 
            return;
        }
 
        await _next(context);
    }
 
    /// <summary>
    /// Sliding window counter dùng Redis INCR + EXPIRE.
    /// Mỗi request = 1 key với TTL = windowSeconds.
    /// Đếm tổng key còn sống trong window.
    ///
    /// Returns: (allowed, remaining, retryAfterSeconds)
    /// </summary>
    private async Task<(bool Allowed, int Remaining, int RetryAfter)> CheckRateLimitAsync(
        string key, int limit, int windowSeconds)
    {
        var db = _redis.GetDatabase();
 
        // Lua script để đảm bảo atomic: INCR + EXPIRE trong 1 operation
        const string luaScript = @"
            local current = redis.call('INCR', KEYS[1])
            if current == 1 then
                redis.call('EXPIRE', KEYS[1], ARGV[1])
            end
            local ttl = redis.call('TTL', KEYS[1])
            return {current, ttl}
        ";
 
        var result = (RedisValue[])await db.ScriptEvaluateAsync(
            luaScript,
            keys:   [new RedisKey(key)],
            values: [(RedisValue)(long)windowSeconds]);
 
        var current    = (int)result[0];
        var ttl        = (int)result[1];
        var remaining  = Math.Max(0, limit - current);
        var allowed    = current <= limit;
        var retryAfter = allowed ? 0 : ttl;
 
        return (allowed, remaining, retryAfter);
    }
 
    private static string GetClientIp(HttpContext context)
    {
        // Hỗ trợ X-Forwarded-For khi đứng sau proxy/load balancer
        var forwarded = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(forwarded))
            return forwarded.Split(',')[0].Trim();
 
        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}
 
/// <summary>
/// Cấu hình 1 rule rate limit.
/// </summary>
public class RateLimitRule
{
    // Tên duy nhất — dùng trong Redis key
    public string Key { get; set; } = null!;
 
    // URL prefix cần apply — VD: "/api/admin/auth/login"
    public string Path { get; set; } = null!;
 
    // HTTP method — "*" = tất cả
    public string Method { get; set; } = "*";
 
    // Số request tối đa trong window
    public int Limit { get; set; }
 
    // Kích thước sliding window (giây)
    public int WindowSeconds { get; set; }
 
    // Message trả về khi bị block
    public string? Message { get; set; }
}