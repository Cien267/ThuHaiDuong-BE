using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using ThuHaiDuong.Application.InterfaceService;
using ThuHaiDuong.Application.Payloads.InputModels.User;
using ThuHaiDuong.Application.Payloads.Responses;
using ThuHaiDuong.Application.Payloads.ResultModels.User;
using ThuHaiDuong.Domain.Entities;
using ThuHaiDuong.Domain.InterfaceRepositories;

namespace ThuHaiDuong.Application.ImplementService;

public class UserProfileService : IUserProfileService
{
    private readonly IBaseRepository<User> _userRepo;
    private readonly IAuthRepository      _authRepo;
    private readonly IFileStorageService  _fileStorage;
 
    public UserProfileService(
        IBaseRepository<User> userRepo,
        IAuthRepository       authRepo,
        IFileStorageService   fileStorage)
    {
        _userRepo    = userRepo;
        _authRepo    = authRepo;
        _fileStorage = fileStorage;
    }
 
    public async Task<UserProfileResult> GetMyProfileAsync(Guid userId)
    {
        var query = _userRepo.BuildQueryable(
            ["Bookmarks", "Comments", "Ratings"],
            u => u.Id == userId && !u.IsDeleted
        );
 
        return await query
            .Select(UserProfileResult.FromUser)
            .FirstOrDefaultAsync()
            ?? throw new ResponseErrorObject(
                "User not found.", StatusCodes.Status404NotFound);
    }
 
    public async Task<UserProfileResult> UpdateProfileAsync(
        Guid userId, UpdateProfileInput input)
    {
        var user = await GetUserOrThrowAsync(userId);
 
        user.FullName    = input.FullName?.Trim();
        user.PhoneNumber = input.PhoneNumber?.Trim();
 
        await _userRepo.UpdateAsync(user);
 
        return await GetMyProfileAsync(userId);
    }
 
    public async Task<UserProfileResult> UpdateUsernameAsync(
        Guid userId, UpdateUsernameInput input)
    {
        var user     = await GetUserOrThrowAsync(userId);
        var newName  = input.UserName.Trim();
 
        // Không làm gì nếu không thay đổi
        if (user.UserName == newName)
            return await GetMyProfileAsync(userId);
 
        if (await _authRepo.UserNameExistsAsync(newName))
            throw new ResponseErrorObject(
                "Username is already taken.", StatusCodes.Status409Conflict);
 
        user.UserName = newName;
        await _userRepo.UpdateAsync(user);
 
        return await GetMyProfileAsync(userId);
    }
 
    public async Task<AvatarUploadResult> UploadAvatarAsync(Guid userId, IFormFile file)
    {
        if (file == null || file.Length == 0)
            throw new ResponseErrorObject(
                "No file provided.", StatusCodes.Status400BadRequest);
 
        var user = await GetUserOrThrowAsync(userId);
 
        // Upload file mới trước
        var newAvatarUrl = await _fileStorage.UploadAsync(file, "avatars");
 
        // Xóa avatar cũ sau khi upload thành công
        // Thứ tự này quan trọng: nếu upload fail → không mất avatar cũ
        var oldAvatarUrl = user.Avatar;
        user.Avatar      = newAvatarUrl;
        await _userRepo.UpdateAsync(user);
 
        // Xóa file cũ sau khi DB đã update — không throw nếu xóa fail
        await _fileStorage.DeleteAsync(oldAvatarUrl);
 
        return new AvatarUploadResult { AvatarUrl = newAvatarUrl };
    }
 
    public async Task RemoveAvatarAsync(Guid userId)
    {
        var user = await GetUserOrThrowAsync(userId);
 
        if (string.IsNullOrWhiteSpace(user.Avatar))
            return; // Không có avatar → không làm gì
 
        var oldAvatarUrl = user.Avatar;
        user.Avatar      = null;
        await _userRepo.UpdateAsync(user);
 
        await _fileStorage.DeleteAsync(oldAvatarUrl);
    }
 
    // ── PRIVATE ───────────────────────────────────────────────────────────────
 
    private async Task<User> GetUserOrThrowAsync(Guid userId)
    {
        return await _userRepo.GetByIdAsync(userId)
            ?? throw new ResponseErrorObject(
                "User not found.", StatusCodes.Status404NotFound);
    }
}