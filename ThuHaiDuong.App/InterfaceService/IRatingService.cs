using ThuHaiDuong.Application.Payloads.InputModels.User.Rating;
using ThuHaiDuong.Application.Payloads.ResultModels.User.Rating;
using ThuHaiDuong.Shared.Models;

namespace ThuHaiDuong.Application.InterfaceService;

public interface IRatingService
{
    // Tạo rating (chỉ 1 lần per user per story — không cho update)
    Task<RatingResult> CreateAsync(Guid userId, CreateRatingInput input);
 
    // Tổng quan rating của 1 story + rating của user hiện tại nếu đã login
    Task<RatingSummary> GetSummaryAsync(Guid storyId, Guid? currentUserId = null);
 
    // Danh sách rating của 1 story (admin)
    Task<PagedResult<RatingResult>> GetListAdminAsync(Guid storyId, int page, int pageSize);
 
    // Admin xóa rating vi phạm
    Task DeleteAsync(Guid ratingId);
}