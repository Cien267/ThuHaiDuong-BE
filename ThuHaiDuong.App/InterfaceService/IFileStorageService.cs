using Microsoft.AspNetCore.Http;

namespace ThuHaiDuong.Application.InterfaceService;

public interface IFileStorageService
{
    /// <summary>
    /// Upload file và trả về URL public để lưu vào DB.
    /// </summary>
    Task<string> UploadAsync(IFormFile file, string folder);
 
    /// <summary>
    /// Xóa file theo URL đã lưu trong DB.
    /// Không throw nếu file không tồn tại.
    /// </summary>
    Task DeleteAsync(string? fileUrl);
}