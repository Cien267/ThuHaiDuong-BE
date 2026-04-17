using ThuHaiDuong.Application.Payloads.InputModels.Common;

namespace ThuHaiDuong.Application.Payloads.InputModels.Analytics;

public class StoryRankingQuery : PaginationParams
{
    // "today" | "week" | "month" | "all"
    public string Period { get; set; } = "today";
}