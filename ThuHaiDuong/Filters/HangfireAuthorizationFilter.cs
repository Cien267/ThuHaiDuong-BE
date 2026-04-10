using Hangfire.Dashboard;

namespace ThuHaiDuong.Filters;

public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    private readonly string[] _requiredRoles;
    private readonly string[] _allowedIPs;

    public HangfireAuthorizationFilter(
        string[]? requiredRoles = null,
        string[]? allowedIPs = null)
    {
        _requiredRoles = requiredRoles ?? new[] { "Administrator" };
        _allowedIPs    = allowedIPs   ?? Array.Empty<string>();
    }

    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        var remoteIp    = httpContext.Connection.RemoteIpAddress?.ToString();

        if (_allowedIPs.Length > 0 && _allowedIPs.Contains(remoteIp))
            return true;

        if (httpContext.User?.Identity?.IsAuthenticated != true)
        {
            httpContext.Response.Redirect("/login?returnUrl=/hangfire");
            return false;
        }

        return _requiredRoles.Any(role => httpContext.User.IsInRole(role));
    }
}