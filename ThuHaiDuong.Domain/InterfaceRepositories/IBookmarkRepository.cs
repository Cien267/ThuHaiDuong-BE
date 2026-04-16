using ThuHaiDuong.Domain.Entities;

namespace ThuHaiDuong.Domain.InterfaceRepositories;

public interface IBookmarkRepository
{
    Task<Bookmark?> GetAsync(Guid userId, Guid storyId);
    Task<bool> ExistsAsync(Guid userId, Guid storyId);
    Task<List<Bookmark>> GetUserBookmarksByIdAsync(Guid userId);
    Task<Dictionary<Guid, UserReadingProgress>> GetUserReadingProgressByStoryIdsAsync(Guid userId, List<Guid> storyIds);
}