using ThuHaiDuong.Application.Payloads.Responses;

namespace ThuHaiDuong.Middlewares;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _env;
 
    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IHostEnvironment env)
    {
        _next   = next;
        _logger = logger;
        _env    = env;
    }
 
    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ResponseErrorObject ex)
        {
            _logger.LogWarning(
                "Business error {StatusCode}: {Message} | Path: {Path}",
                ex.StatusCode, ex.Message, context.Request.Path);
 
            await WriteResponseAsync(context, ex.StatusCode, ex.Message, ex.Errors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unhandled exception | Path: {Path} | Method: {Method}",
                context.Request.Path, context.Request.Method);
 
            var message = _env.IsProduction()
                ? "An unexpected error occurred. Please try again later."
                : ex.Message;
 
            var errors = _env.IsProduction()
                ? null
                : new Dictionary<string, string[]>
                {
                    ["type"]           = [ex.GetType().Name],
                    ["details"]        = [ex.Message],
                    ["stackTrace"]     = [ex.StackTrace ?? ""],
                    ["innerException"] = [ex.InnerException?.Message ?? ""],
                };
 
            await WriteResponseAsync(context, 500, message, errors);
        }
    }
 
    private static async Task WriteResponseAsync(
        HttpContext context,
        int statusCode,
        string message,
        object? errors = null)
    {
        context.Response.StatusCode  = statusCode;
        context.Response.ContentType = "application/json";
 
        await context.Response.WriteAsJsonAsync(new
        {
            message,
            statusCode,
            errors,
        });
    }
}