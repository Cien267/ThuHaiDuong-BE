using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using ThuHaiDuong.Application.InterfaceService;
using ThuHaiDuong.Application.Payloads.InputModels.User.Rating;
using ThuHaiDuong.Application.Payloads.Responses;
using ThuHaiDuong.Application.Payloads.ResultModels.User.Rating;
using ThuHaiDuong.Domain.Entities;
using ThuHaiDuong.Domain.InterfaceRepositories;
using ThuHaiDuong.Shared.Constants;
using ThuHaiDuong.Shared.Models;

namespace ThuHaiDuong.Application.ImplementService;

public class RatingService : IRatingService
{
    private readonly IBaseRepository<Rating> _baseRepo;
    private readonly IBaseRepository<Story>  _storyRepo;
    private readonly IRatingRepository       _ratingRepo;
 
    public RatingService(
        IBaseRepository<Rating> baseRepo,
        IBaseRepository<Story>  storyRepo,
        IRatingRepository       ratingRepo)
    {
        _baseRepo   = baseRepo;
        _storyRepo  = storyRepo;
        _ratingRepo = ratingRepo;
    }
 
    public async Task<RatingResult> CreateAsync(Guid userId, CreateRatingInput input)
    {
        // Validate story tồn tại và đang public
        var story = await _storyRepo.GetByIdAsync(input.StoryId);
        if (story == null || story.IsDeleted
            || (story.Status != StoryStatus.Publishing
                && story.Status != StoryStatus.Completed))
            throw new ResponseErrorObject("Story not found", StatusCodes.Status404NotFound);
 
        // Mỗi user chỉ rate 1 lần
        if (await _ratingRepo.UserHasRatedAsync(userId, input.StoryId))
            throw new ResponseErrorObject(
                "You have already rated this story.",
                StatusCodes.Status409Conflict);
 
        var rating = new Rating
        {
            UserId  = userId,
            StoryId = input.StoryId,
            Score   = input.Score,
            Comment = input.Comment?.Trim(),
        };
 
        await _baseRepo.CreateAsync(rating);
 
        // Sync AverageRating + RatingCount trên Story ngay lập tức
        await _ratingRepo.SyncStoryRatingAsync(input.StoryId);
 
        // Reload với User navigation để trả về đầy đủ
        var query = _baseRepo.BuildQueryable(
            ["User"],
            r => r.Id == rating.Id
        );
 
        return await query
            .Select(RatingResult.FromRating)
            .FirstAsync();
    }
 
    public async Task<RatingSummary> GetSummaryAsync(Guid storyId, Guid? currentUserId = null)
    {
        var story = await _storyRepo.GetByIdAsync(storyId)
            ?? throw new ResponseErrorObject("Story not found", StatusCodes.Status404NotFound);
 
        var distribution = await _ratingRepo.GetScoreDistributionAsync(storyId);
 
        RatingResult? myRating = null;
        if (currentUserId.HasValue)
        {
            var userRating = await _ratingRepo.GetUserRatingAsync(currentUserId.Value, storyId);
            if (userRating != null)
            {
                myRating = new RatingResult
                {
                    Id        = userRating.Id,
                    StoryId   = userRating.StoryId,
                    UserId    = userRating.UserId,
                    UserName  = userRating.User.UserName,
                    Score     = userRating.Score,
                    Comment   = userRating.Comment,
                    CreatedAt = userRating.CreatedAt,
                };
            }
        }
 
        return new RatingSummary
        {
            AverageScore      = story.AverageRating,
            TotalRatings      = story.RatingCount,
            ScoreDistribution = distribution,
            MyRating          = myRating,
        };
    }
 
    public async Task<PagedResult<RatingResult>> GetListAdminAsync(
        Guid storyId, int page, int pageSize)
    {
        var query = _baseRepo.BuildQueryable(
            ["User"],
            r => r.StoryId == storyId && !r.IsDeleted
        );
 
        var total = await query.CountAsync();
 
        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(RatingResult.FromRating)
            .ToListAsync();
 
        return new PagedResult<RatingResult>(items, total, page, pageSize);
    }
 
    public async Task DeleteAsync(Guid ratingId)
    {
        var rating = await _baseRepo.GetByIdAsync(ratingId)
            ?? throw new ResponseErrorObject("Rating not found", StatusCodes.Status404NotFound);
 
        await _baseRepo.DeleteAsync(ratingId);
 
        // Sync lại rating story sau khi xóa
        await _ratingRepo.SyncStoryRatingAsync(rating.StoryId);
    }
}