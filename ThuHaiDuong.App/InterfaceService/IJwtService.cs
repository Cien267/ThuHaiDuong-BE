using System.Security.Claims;
using ThuHaiDuong.Domain.Entities;

namespace ThuHaiDuong.Application.InterfaceService;

public interface IJwtService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    ClaimsPrincipal? ValidateToken(string token);
}