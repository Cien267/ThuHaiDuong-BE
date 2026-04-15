using System.Linq.Expressions;
using ThuHaiDuong.Application.Payloads.ResultModels.Common;

namespace ThuHaiDuong.Application.Payloads.ResultModels.Admin.Category;

public class CategoryResult : DataResponseBase
{
    public string Name { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string? Description { get; set; }
    public Guid? ParentId { get; set; }
    public string? ParentName { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
    public int StoryCount { get; set; }
 
    public static Expression<Func<Domain.Entities.Category, CategoryResult>> FromCategory =>
        c => new CategoryResult
        {
            Id = c.Id,
            Name = c.Name,
            Slug = c.Slug,
            Description = c.Description,
            ParentId = c.ParentId,
            ParentName = c.Parent != null ? c.Parent.Name : null,
            SortOrder = c.SortOrder,
            IsActive = c.IsActive,
            StoryCount = c.StoryCategories.Count(sc => !sc.Story.DeletedAt.HasValue),
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt,
        };
}