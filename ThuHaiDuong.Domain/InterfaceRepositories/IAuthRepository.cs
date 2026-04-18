using ThuHaiDuong.Domain.Entities;

namespace ThuHaiDuong.Domain.InterfaceRepositories;

public interface IAuthRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByUserNameAsync(string userName);
    Task<bool> EmailExistsAsync(string email);
    Task<bool> UserNameExistsAsync(string userName);
 
    // Refresh token
    Task<RefreshToken?> GetActiveRefreshTokenAsync(Guid userId);
    Task<RefreshToken?> GetByTokenValueAsync(string token);
 
    // Revoke tất cả refresh token của user (login mới → xóa cũ)
    Task RevokeAllUserTokensAsync(Guid userId);
}