using ThuHaiDuong.Application.Payloads.Responses;

namespace ThuHaiDuong.Middlewares;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ResponseErrorObject ex)
        {
            context.Response.StatusCode = ex.StatusCode;
            context.Response.ContentType = "application/json";

            var response = new
            {
                message = ex.Message,
                statusCode = ex.StatusCode,
                errors = ex.Errors
            };

            await context.Response.WriteAsJsonAsync(response);
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";

            var response = new
            {
                message = ex.Message,
                statusCode = 500,
                errors = new
                {
                    type = ex.GetType().Name,
                    details = ex.Message,
                    stackTrace = ex.StackTrace,
                    innerException = ex.InnerException?.Message
                }
            };

            await context.Response.WriteAsJsonAsync(response);
        }
    }
}