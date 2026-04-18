using ThuHaiDuong.Application.Payloads.InputModels.ReadingProgress;
using ThuHaiDuong.Application.Payloads.ResultModels.ReadingProgress;
using ThuHaiDuong.Shared.Models;

namespace ThuHaiDuong.Application.InterfaceService;

public interface IReadingProgressService
{
    // Cập nhật progress + ghi history khi user đọc chapter mới
    Task UpdateAsync(Guid userId, UpdateReadingProgressInput input);
 
    // Lấy tiến độ đọc của user với 1 story
    // Trả về null nếu user chưa đọc chapter nào
    Task<ReadingProgressResult?> GetProgressAsync(Guid userId, Guid storyId);
 
    // Lịch sử đọc có phân trang
    // Filter theo storyId để xem đã đọc những chapter nào trong 1 truyện
    Task<PagedResult<ReadingHistoryItem>> GetHistoryAsync(
        Guid userId, ReadingHistoryQuery query);
 
    // Lấy Set chapterId đã đọc trong 1 story
    // Dùng để đánh dấu chapter "đã đọc" trên danh sách chapter
    Task<HashSet<Guid>> GetReadChapterIdsAsync(Guid userId, Guid storyId);
 
    // Xóa toàn bộ lịch sử đọc của user với 1 story
    Task ClearHistoryAsync(Guid userId, Guid storyId);
}