using System.Linq.Expressions;
using ThuHaiDuong.Application.Payloads.ResultModels.Common;

namespace ThuHaiDuong.Application.Payloads.ResultModels.User.Rating;

public class RatingResult : DataResponseBase
{
    public Guid StoryId { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = null!;
    public int Score { get; set; }
    public string? Comment { get; set; }
 
    public static Expression<Func<Domain.Entities.Rating, RatingResult>> FromRating =>
        r => new RatingResult
        {
            Id        = r.Id,
            StoryId   = r.StoryId,
            UserId    = r.UserId,
            UserName  = r.User.UserName,
            Score     = r.Score,
            Comment   = r.Comment,
            CreatedAt = r.CreatedAt,
        };
}