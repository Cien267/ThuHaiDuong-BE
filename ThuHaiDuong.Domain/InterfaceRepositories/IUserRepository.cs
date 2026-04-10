using ThuHaiDuong.Domain.Entities;

namespace ThuHaiDuong.Domain.InterfaceRepositories
{
    public interface IUserRepository
    {
        Task<User> GetUserByEmail(string email);
        Task<User> GetUserByUsername(string username);
        Task<bool> AddRoleToUserAsync(User user, List<string> Roles);
        Task<IEnumerable<string>> GetRolesOfUserAsync(User user);

        Task<User> UpdateAsync(User user);
        Task RevokeRefreshTokensAsync(Guid userId);

    }
}
