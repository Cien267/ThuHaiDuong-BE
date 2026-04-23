using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using ThuHaiDuong.Application.InterfaceService;
using ThuHaiDuong.Application.Payloads.Responses;

namespace ThuHaiDuong.Application.ImplementService;

public class LocalFileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _env;
    private readonly IConfiguration     _config;
 
    // Các MIME type được phép upload avatar
    private static readonly string[] AllowedMimeTypes =
        ["image/jpeg", "image/png", "image/webp", "image/gif"];
 
    // Giới hạn 5MB
    private const long MaxFileSizeBytes = 5 * 1024 * 1024;
 
    public LocalFileStorageService(IWebHostEnvironment env, IConfiguration config)
    {
        _env    = env;
        _config = config;
    }
 
    public async Task<string> UploadAsync(IFormFile file, string folder)
    {
        // Validate MIME type
        if (!AllowedMimeTypes.Contains(file.ContentType.ToLower()))
            throw new ResponseErrorObject(
                $"File type not allowed. Allowed: {string.Join(", ", AllowedMimeTypes)}",
                StatusCodes.Status400BadRequest);
 
        // Validate file size
        if (file.Length > MaxFileSizeBytes)
            throw new ResponseErrorObject(
                "File size exceeds 5MB limit.",
                StatusCodes.Status400BadRequest);
 
        // Tạo thư mục nếu chưa có
        var uploadDir = Path.Combine(_env.WebRootPath, "uploads", folder);
        Directory.CreateDirectory(uploadDir);
 
        // Generate tên file unique — tránh trùng và tránh path traversal
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var fileName  = $"{Guid.NewGuid():N}{extension}";
        var filePath  = Path.Combine(uploadDir, fileName);
 
        await using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);
 
        // Trả về URL relative — frontend ghép domain
        // Hoặc có thể trả về absolute URL nếu config BaseUrl
        var baseUrl = _config["App:BaseUrl"]?.TrimEnd('/') ?? "";
        return $"{baseUrl}/uploads/{folder}/{fileName}";
    }
 
    public Task DeleteAsync(string? fileUrl)
    {
        if (string.IsNullOrWhiteSpace(fileUrl)) return Task.CompletedTask;
 
        try
        {
            // Chuyển URL về đường dẫn file vật lý
            // VD: "https://domain.com/uploads/avatars/abc.jpg"
            //   → "uploads/avatars/abc.jpg"
            //   → "{wwwroot}/uploads/avatars/abc.jpg"
            var uri          = new Uri(fileUrl, UriKind.RelativeOrAbsolute);
            var relativePath = uri.IsAbsoluteUri ? uri.AbsolutePath : fileUrl;
 
            // Strip leading slash
            relativePath = relativePath.TrimStart('/');
 
            // Bảo vệ: chỉ xóa file trong thư mục uploads
            if (!relativePath.StartsWith("uploads/", StringComparison.OrdinalIgnoreCase))
                return Task.CompletedTask;
 
            var filePath = Path.Combine(_env.WebRootPath, relativePath);
 
            if (File.Exists(filePath))
                File.Delete(filePath);
        }
        catch
        {
            // Không throw — file không tồn tại hoặc lỗi IO đều bỏ qua
        }
 
        return Task.CompletedTask;
    }
}