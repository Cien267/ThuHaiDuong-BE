using System.ComponentModel.DataAnnotations;

namespace ThuHaiDuong.Application.Payloads.InputModels.Comment;

public class CreateCommentInput
{
    [Required]
    public Guid StoryId { get; set; }
 
    public Guid? ChapterId { get; set; }
 
    // null = comment gốc; có giá trị = reply
    public Guid? ParentCommentId { get; set; }
 
    [Required, MaxLength(5000)]
    public string Content { get; set; } = null!;
 
    // Chỉ dùng khi anonymous (chưa login)
    [MaxLength(100)]
    public string? GuestName { get; set; }
 
    [MaxLength(256), EmailAddress]
    public string? GuestEmail { get; set; }
}