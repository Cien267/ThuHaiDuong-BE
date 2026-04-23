using Microsoft.AspNetCore.Http;
using ThuHaiDuong.Application.Payloads.InputModels.User;
using ThuHaiDuong.Application.Payloads.ResultModels.User;

namespace ThuHaiDuong.Application.InterfaceService;

public interface IUserProfileService
{
    // Lấy profile đầy đủ của user hiện tại
    Task<UserProfileResult> GetMyProfileAsync(Guid userId);
 
    // Cập nhật thông tin cơ bản (FullName, PhoneNumber)
    Task<UserProfileResult> UpdateProfileAsync(Guid userId, UpdateProfileInput input);
 
    // Đổi username — kiểm tra unique
    Task<UserProfileResult> UpdateUsernameAsync(Guid userId, UpdateUsernameInput input);
 
    // Upload avatar mới — xóa avatar cũ sau khi upload thành công
    Task<AvatarUploadResult> UploadAvatarAsync(Guid userId, IFormFile file);
 
    // Xóa avatar — về trạng thái không có ảnh
    Task RemoveAvatarAsync(Guid userId);
}