using ThuHaiDuong.Domain.Entities;

namespace ThuHaiDuong.Domain.InterfaceRepositories;

public interface IReadingProgressRepository
{
    // Lấy progress của user với 1 story
    Task<UserReadingProgress?> GetProgressAsync(Guid userId, Guid storyId);
 
 
    // Upsert progress (tạo mới hoặc cập nhật)
    Task UpsertProgressAsync(Guid userId, Guid storyId, Guid chapterId, int chapterNumber);
 
    // Upsert history (tạo mới hoặc update ReadAt nếu đã tồn tại)
    Task UpsertHistoryAsync(Guid userId, Guid storyId, Guid chapterId);
 
 
    // Kiểm tra user đã đọc chapter cụ thể chưa (dùng để đánh dấu trên UI)
    Task<HashSet<Guid>> GetReadChapterIdsAsync(Guid userId, Guid storyId);
}