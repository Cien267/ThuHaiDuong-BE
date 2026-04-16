using ThuHaiDuong.Application.Payloads.InputModels.Admin.Auth;
using ThuHaiDuong.Domain.Entities;
using ThuHaiDuong.Application.Payloads.Responses;
using ThuHaiDuong.Application.Payloads.ResultModels.User;
using ThuHaiDuong.Shared.Models;

namespace ThuHaiDuong.Application.InterfaceService
{
    public interface IAuthService
    {
        Task<UserResult> RegisterAsync(RegisterInput request);
        Task<ResponseObject<LoginResult>> GetJwtTokenAsync(User user);
        Task<LoginResult> LoginAsync(LoginInput request);

        Task<ResponseObject<UserResult>> UpdateProfileAsync(Guid userId, UpdateProfileInput request);

        Task<UserResult> GetUserInfoAsync();
        Task<ResponseObject<UserResult>> ChangePasswordAsync(Guid userId, ChangePasswordInput request);
        Task LogoutAsync();
    }
}
