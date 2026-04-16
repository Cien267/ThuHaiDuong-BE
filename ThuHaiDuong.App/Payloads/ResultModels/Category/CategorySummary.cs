using System.Linq.Expressions;
using ThuHaiDuong.Application.Payloads.ResultModels.Common;

namespace ThuHaiDuong.Application.Payloads.ResultModels.User.Category;

public class CategorySummary : DataResponseBase
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string? Description { get; set; }
    public Guid? ParentId { get; set; }
    public int SortOrder { get; set; }
    public List<CategorySummary> Children { get; set; } = [];
 
    public static Expression<Func<Domain.Entities.Category, CategorySummary>> FromCategory =>
        c => new CategorySummary
        {
            Id = c.Id,
            Name = c.Name,
            Slug = c.Slug,
            Description = c.Description,
            ParentId = c.ParentId,
            SortOrder = c.SortOrder,
            Children = new  List<CategorySummary>()
        };
}