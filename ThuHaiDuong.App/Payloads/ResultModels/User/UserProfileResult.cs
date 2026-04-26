using System.Linq.Expressions;
using ThuHaiDuong.Application.Payloads.ResultModels.Common;

namespace ThuHaiDuong.Application.Payloads.ResultModels.User;

public class UserProfileResult : DataResponseBase
{
    public string UserName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? FullName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Avatar { get; set; }
    public string Role { get; set; } = null!;
    public DateTime? LastLoginAt { get; set; }
 
    // Thống kê nhỏ hiển thị trên trang profile
    public int BookmarkCount { get; set; }
    public int CommentCount { get; set; }
    public int RatingCount { get; set; }
 
    public static Expression<Func<Domain.Entities.User, UserProfileResult>> FromUser =>
        u => new UserProfileResult
        {
            Id          = u.Id,
            UserName    = u.UserName,
            Email       = u.Email,
            FullName    = u.FullName,
            PhoneNumber = u.PhoneNumber,
            Avatar      = u.Avatar,
            Role        = u.Role,
            LastLoginAt = u.LastLoginAt,
            CreatedAt   = u.CreatedAt,
            BookmarkCount = u.Bookmarks.Count(b => !b.IsDeleted),
            CommentCount  = u.Comments.Count(c => !c.IsDeleted),
            RatingCount   = u.Ratings.Count(r => !r.IsDeleted),
        };
}