using System.Linq.Expressions;
using ThuHaiDuong.Application.Payloads.ResultModels.Common;

namespace ThuHaiDuong.Application.Payloads.ResultModels.Admin.Author;

public class AuthorResult : DataResponseBase
{
    public string Name { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string? PenName { get; set; }
    public string? Country { get; set; }
    public string? Description { get; set; }
    public string? AvatarUrl { get; set; }
    public int StoryCount { get; set; }
    public int PublishedStoryCount { get; set; }
 
    public static Expression<Func<Domain.Entities.Author, AuthorResult>> FromAuthor =>
        a => new AuthorResult
        {
            Id                 = a.Id,
            Name               = a.Name,
            Slug               = a.Slug,
            PenName            = a.PenName,
            Country            = a.Country,
            Description        = a.Description,
            AvatarUrl          = a.AvatarUrl,
            StoryCount         = a.Stories.Count(s => !s.DeletedAt.HasValue),
            PublishedStoryCount = a.Stories.Count(s => !s.DeletedAt.HasValue && (s.Status == "Publishing" || s.Status == "Completed")),
            CreatedAt          = a.CreatedAt,
            UpdatedAt          = a.UpdatedAt,
        };
}