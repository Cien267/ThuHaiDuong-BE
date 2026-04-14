using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using ThuHaiDuong.Application.InterfaceService;

namespace ThuHaiDuong.Application.ImplementService;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _contextAccessor;

    public CurrentUserService(IHttpContextAccessor contextAccessor)
    {
        _contextAccessor = contextAccessor;
    }

    public Guid? GetUserId()
    {
        var userIdClaim = _contextAccessor.HttpContext?.User?.FindFirst("UserId")?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }

    public string GetUserName()
    {
        return _contextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value;
    }

    public string GetEmail()
    {
        return _contextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value;
    }

    public IEnumerable<string> GetRoles()
    {
        return _contextAccessor.HttpContext?.User?
            .FindAll(ClaimTypes.Role)
            .Select(c => c.Value) ?? Enumerable.Empty<string>();
    }

    public IEnumerable<string> GetPermissions()
    {
        return _contextAccessor.HttpContext?.User?
            .FindAll("Permission")
            .Select(c => c.Value) ?? Enumerable.Empty<string>();
    }
}