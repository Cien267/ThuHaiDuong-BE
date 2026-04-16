using ThuHaiDuong.Application.Payloads.InputModels.Common;

namespace ThuHaiDuong.Application.Payloads.InputModels.Comment;

public class AdminCommentQuery : CommentQuery
{
    public bool? IsHidden { get; set; }
    public bool? IsGuest { get; set; }
}