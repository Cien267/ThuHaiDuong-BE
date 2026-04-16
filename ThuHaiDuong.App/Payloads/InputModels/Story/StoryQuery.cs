using ThuHaiDuong.Application.Payloads.InputModels.Common;

namespace ThuHaiDuong.Application.Payloads.InputModels.Story;

public class StoryQuery : PaginationParams
{
    public string? Keyword { get; set; }
 
    public Guid? CategoryId { get; set; }
    public Guid? TagId { get; set; }
    public Guid? AuthorId { get; set; }
    public string? Country { get; set; }
    public string? StoryType { get; set; }
 
    // Admin-only filters
    public string? Status { get; set; }
    public string? ContentSource { get; set; }
    public Guid? UploadedByUserId { get; set; }
}