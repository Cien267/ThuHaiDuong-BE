using ThuHaiDuong.Application.Payloads.InputModels.Common;

namespace ThuHaiDuong.Application.Payloads.InputModels.ReadingProgress;

public class ReadingHistoryQuery : PaginationParams
{
    public Guid? StoryId { get; set; }
}