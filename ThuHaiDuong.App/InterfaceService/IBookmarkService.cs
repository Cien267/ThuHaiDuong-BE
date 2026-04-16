using ThuHaiDuong.Application.Payloads.ResultModels.Admin.Bookmark;
using ThuHaiDuong.Application.Payloads.ResultModels.User.Bookmark;

namespace ThuHaiDuong.Application.InterfaceService;

public interface IBookmarkService
{
    // Toggle: đã bookmark → xóa; chưa bookmark → thêm
    Task<BookmarkToggleResult> ToggleAsync(Guid userId, Guid storyId);
 
    // Kiểm tra user đã bookmark chưa (dùng khi render trang truyện)
    Task<bool> IsBookmarkedAsync(Guid userId, Guid storyId);
 
    // Danh sách bookmark của user kèm reading progress
    Task<List<BookmarkResult>> GetUserBookmarksAsync(Guid userId);
}