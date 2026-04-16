using ThuHaiDuong.Application.Payloads.InputModels.Common;

namespace ThuHaiDuong.Application.Payloads.InputModels.Chapter;

public class ChapterQuery : PaginationParams
{
    public Guid StoryId { get; set; }
    public string? Status { get; set; }
    public bool? IsVip { get; set; }
}