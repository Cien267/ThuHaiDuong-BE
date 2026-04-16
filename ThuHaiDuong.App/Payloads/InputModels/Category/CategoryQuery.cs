using ThuHaiDuong.Application.Payloads.InputModels.Common;

namespace ThuHaiDuong.Application.Payloads.InputModels.Category;

public class CategoryQuery : PaginationParams
{
    public string? Name { get; set; }
    public bool? IsActive { get; set; }
    public Guid? ParentId { get; set; }
}