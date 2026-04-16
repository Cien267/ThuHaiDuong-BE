using System.Linq.Expressions;
using ThuHaiDuong.Application.Payloads.ResultModels.Common;

namespace ThuHaiDuong.Application.Payloads.ResultModels.Admin.Comment;

public class CommentResult : DataResponseBase
{
    public Guid StoryId { get; set; }
    public Guid? ChapterId { get; set; }
    public Guid? ParentCommentId { get; set; }
    public string Content { get; set; } = null!;
    public int LikeCount { get; set; }
 
    // Author info — một trong hai sẽ có giá trị
    public CommentAuthorInfo Author { get; set; } = null!;
 
    // Replies (chỉ load ở level 1, không đệ quy)
    public List<CommentResult> Replies { get; set; } = [];
 
    public static Expression<Func<Domain.Entities.Comment, CommentResult>> FromComment =>
        c => new CommentResult
        {
            Id              = c.Id,
            StoryId         = c.StoryId,
            ChapterId       = c.ChapterId,
            ParentCommentId = c.ParentCommentId,
            Content         = c.Content,
            LikeCount       = c.LikeCount,
            CreatedAt       = c.CreatedAt,
            Author          = new CommentAuthorInfo
            {
                UserId    = c.UserId,
                UserName  = c.UserId != null ? c.User!.UserName : null,
                Avatar    = c.UserId != null ? c.User!.Avatar : null,
                GuestName = c.GuestName,
                IsGuest   = c.UserId == null,
            },
            Replies = new List<CommentResult>()
        };
}