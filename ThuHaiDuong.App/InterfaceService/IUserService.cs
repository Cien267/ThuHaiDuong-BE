using ThuHaiDuong.Application.Payloads.InputModels.User;
using ThuHaiDuong.Application.Payloads.ResultModels.User;
using ThuHaiDuong.Shared.Models;

namespace ThuHaiDuong.Application.InterfaceService
{
    public interface IUserService
    {
        Task<List<UserResult>> GetAllUserAsync();
        Task<PagedResult<UserResult>> GetListUsersAsync(UserQuery query);
        Task<UserResult> GetUserByIdAsync(Guid id);
        Task<UserResult> CreateUserAsync(CreateUserInput request);
        Task<UserResult> UpdateUserAsync(Guid userId, UpdateUserInput request);
        Task DeleteUserAsync(Guid id);
    }
}
