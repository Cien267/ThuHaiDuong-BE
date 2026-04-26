using System.Linq.Expressions;
using ThuHaiDuong.Application.Payloads.ResultModels.Common;

namespace ThuHaiDuong.Application.Payloads.ResultModels.User.Author;

public class AuthorSummary : DataResponseBase
{
    public string Name { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string? PenName { get; set; }
    public string? Country { get; set; }
    public string? AvatarUrl { get; set; }
    public int StoryCount { get; set; }
 
    public static Expression<Func<Domain.Entities.Author, AuthorSummary>> FromAuthor =>
        a => new AuthorSummary
        {
            Id         = a.Id,
            Name       = a.Name,
            Slug       = a.Slug,
            PenName    = a.PenName,
            Country    = a.Country,
            AvatarUrl  = a.AvatarUrl,
            StoryCount = a.Stories.Count(s => !s.IsDeleted && (s.Status == "Publishing" || s.Status == "Completed")),
        };
}