using ThuHaiDuong.Application.Payloads.InputModels.Common;

namespace ThuHaiDuong.Application.Payloads.InputModels.Author;

public class AuthorQuery : PaginationParams
{
    public string? Name { get; set; }
    public string? Country { get; set; }
}