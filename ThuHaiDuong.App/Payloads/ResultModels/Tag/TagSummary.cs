using System.Linq.Expressions;
using ThuHaiDuong.Application.Payloads.ResultModels.Common;

namespace ThuHaiDuong.Application.Payloads.ResultModels.User.Tag;

public class TagSummary : DataResponseBase
{
    public string Name { get; set; } = null!;
    public string Slug { get; set; } = null!;
 
    public static Expression<Func<Domain.Entities.Tag, TagSummary>> FromTag =>
        t => new TagSummary
        {
            Id = t.Id,
            Name = t.Name,
            Slug = t.Slug,
        };
}