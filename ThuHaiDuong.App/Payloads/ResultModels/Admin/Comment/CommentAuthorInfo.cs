namespace ThuHaiDuong.Application.Payloads.ResultModels.Admin.Comment;

public class CommentAuthorInfo
{
    public Guid? UserId { get; set; }
    public string? UserName { get; set; }
    public string? Avatar { get; set; }
 
    // Anonymous guest
    public string? GuestName { get; set; }
    public bool IsGuest { get; set; }
 
    // Display name: UserName nếu có, GuestName nếu anonymous
    public string DisplayName => IsGuest
        ? (GuestName ?? "Khách")
        : (UserName ?? "Người dùng");
}