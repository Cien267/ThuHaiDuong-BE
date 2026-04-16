using ThuHaiDuong.Domain.Entities;

namespace ThuHaiDuong.Domain.InterfaceRepositories;

public interface IRatingRepository
{
    Task<Rating?> GetUserRatingAsync(Guid userId, Guid storyId);
    Task<bool> UserHasRatedAsync(Guid userId, Guid storyId);
 
    // Lấy phân bố điểm để hiển thị RatingSummary
    Task<Dictionary<int, int>> GetScoreDistributionAsync(Guid storyId);
 
    // Cập nhật AverageRating + RatingCount trên Story sau khi thêm rating
    Task SyncStoryRatingAsync(Guid storyId);
}