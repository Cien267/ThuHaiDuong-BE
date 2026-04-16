using ThuHaiDuong.Application.Payloads.InputModels.Comment;
using ThuHaiDuong.Application.Payloads.ResultModels.Comment;
using ThuHaiDuong.Shared.Models;

namespace ThuHaiDuong.Application.InterfaceService;

public interface ICommentService
{
    // Danh sách comment có phân trang + replies (public — không thấy hidden)
    Task<PagedResult<CommentResult>> GetListAsync(CommentQuery query);
 
    // Tạo comment (anonymous hoặc logged-in)
    Task<CommentResult> CreateAsync(CreateCommentInput input, Guid? userId);
 
    // Admin: danh sách tất cả comment kể cả hidden
    Task<PagedResult<CommentResult>> GetListAdminAsync(AdminCommentQuery query);
 
    // Admin: ẩn/hiện comment
    Task ToggleHideAsync(Guid commentId);
 
    // Admin: xóa mềm
    Task DeleteAsync(Guid commentId);
}