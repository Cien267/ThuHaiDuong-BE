using ThuHaiDuong.Application.Payloads.InputModels.Common;

namespace ThuHaiDuong.Application.Payloads.InputModels.Admin.Tag;

public class TagQuery : PaginationParams
{
    public string? Name { get; set; }
}