using System.Linq.Expressions;
using ThuHaiDuong.Application.Payloads.ResultModels.Common;

namespace ThuHaiDuong.Application.Payloads.ResultModels.Admin.Tag;

public class TagResult : DataResponseBase
{
    public string Name { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public int StoryCount { get; set; }
 
    public static Expression<Func<Domain.Entities.Tag, TagResult>> FromTag =>
        t => new TagResult
        {
            Id = t.Id,
            Name = t.Name,
            Slug = t.Slug,
            StoryCount = t.StoryTags.Count,
            CreatedAt = t.CreatedAt,
        };
}