using System.ComponentModel.DataAnnotations;
using ThuHaiDuong.Application.Payloads.InputModels.Common;

namespace ThuHaiDuong.Application.Payloads.InputModels.Admin.Comment;

public class CommentQuery : PaginationParams
{
    [Required]
    public Guid StoryId { get; set; }
 
    public Guid? ChapterId { get; set; }
}