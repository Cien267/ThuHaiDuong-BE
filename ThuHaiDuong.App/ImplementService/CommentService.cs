using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using ThuHaiDuong.Application.InterfaceService;
using ThuHaiDuong.Application.Payloads.InputModels.Comment;
using ThuHaiDuong.Application.Payloads.Responses;
using ThuHaiDuong.Application.Payloads.ResultModels.Comment;
using ThuHaiDuong.Domain.Entities;
using ThuHaiDuong.Domain.InterfaceRepositories;
using ThuHaiDuong.Shared.Constants;
using ThuHaiDuong.Shared.Models;

namespace ThuHaiDuong.Application.ImplementService;

public class CommentService : ICommentService
{
    private readonly IBaseRepository<Comment> _baseRepo;
    private readonly IBaseRepository<Story>   _storyRepo;
    private readonly ICommentRepository       _commentRepo;
 
    public CommentService(
        IBaseRepository<Comment> baseRepo,
        IBaseRepository<Story>   storyRepo,
        ICommentRepository       commentRepo)
    {
        _baseRepo    = baseRepo;
        _storyRepo   = storyRepo;
        _commentRepo = commentRepo;
    }
 
    public async Task<PagedResult<CommentResult>> GetListAsync(CommentQuery query)
    {
        var rootQuery = _baseRepo.BuildQueryable(
            [],
            c =>
                c.StoryId == query.StoryId &&
                c.ChapterId == query.ChapterId &&
                c.ParentCommentId == null &&
                !c.IsHidden &&
                !c.DeletedAt.HasValue
        );
        
        var total = await rootQuery.CountAsync();
        
        var rootComments = await rootQuery
            .Include(c => c.User)
            .OrderByDescending(c => c.CreatedAt)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(CommentResult.FromComment)
            .ToListAsync();

        if (rootComments.Count == 0)
            return new PagedResult<CommentResult>(rootComments, total, query.PageNumber, query.PageSize);
        
        var rootIds = rootComments.Select(c => c.Id).ToList();
        var repliesQuery = _baseRepo.BuildQueryable(
            ["User"],
            c =>
                c.ParentCommentId != null &&
                rootIds.Contains(c.ParentCommentId.Value) &&
                !c.IsHidden &&
                !c.DeletedAt.HasValue
        );
        
        var replies = await repliesQuery.OrderBy(c => c.CreatedAt)
            .Select(CommentResult.FromComment)
            .ToListAsync();
        
        var replyLookup = replies
            .GroupBy(r => r.ParentCommentId!.Value)
            .ToDictionary(g => g.Key, g => g.ToList());
        
        foreach (var root in rootComments)
        {
            if (replyLookup.TryGetValue(root.Id, out var rootReplies))
                root.Replies = rootReplies;
        }
        
        return new PagedResult<CommentResult>(rootComments, total, query.PageNumber, query.PageSize);
    }
 
    public async Task<CommentResult> CreateAsync(CreateCommentInput input, Guid? userId)
    {
        // Validate story tồn tại và đang public
        var story = await _storyRepo.GetByIdAsync(input.StoryId);
        if (story == null || story.DeletedAt.HasValue
            || (story.Status != StoryStatus.Publishing
                && story.Status != StoryStatus.Completed))
            throw new ResponseErrorObject("Story not found", StatusCodes.Status404NotFound);
 
        // Anonymous phải có GuestName
        if (userId == null && string.IsNullOrWhiteSpace(input.GuestName))
            throw new ResponseErrorObject(
                "Guest name is required for anonymous comments.",
                StatusCodes.Status400BadRequest);
 
        // Validate ParentCommentId nếu là reply
        if (input.ParentCommentId.HasValue)
        {
            var parent = await _commentRepo.GetByIdAsync(input.ParentCommentId.Value);
            if (parent == null)
                throw new ResponseErrorObject(
                    "Parent comment not found.",
                    StatusCodes.Status404NotFound);
 
            // Không cho reply của reply (chỉ hỗ trợ 1 cấp)
            if (parent.ParentCommentId != null)
                throw new ResponseErrorObject(
                    "Cannot reply to a reply. Only one level of nesting is supported.",
                    StatusCodes.Status400BadRequest);
 
            // Reply phải cùng story
            if (parent.StoryId != input.StoryId)
                throw new ResponseErrorObject(
                    "Parent comment does not belong to this story.",
                    StatusCodes.Status400BadRequest);
        }
 
        var comment = new Comment
        {
            UserId          = userId,
            StoryId         = input.StoryId,
            ChapterId       = input.ChapterId,
            ParentCommentId = input.ParentCommentId,
            Content         = input.Content.Trim(),
            GuestName       = userId == null ? input.GuestName?.Trim() : null,
            GuestEmail      = userId == null ? input.GuestEmail?.Trim() : null,
        };
 
        await _baseRepo.CreateAsync(comment);
 
        // Reload để trả về đầy đủ Author info
        var query = _baseRepo.BuildQueryable(
            ["User"],
            c => c.Id == comment.Id
        );
 
        return await query
            .Select(CommentResult.FromComment)
            .FirstAsync();
    }
 
    public async Task<PagedResult<CommentResult>> GetListAdminAsync(AdminCommentQuery query)
    {
        var dbQuery = _baseRepo.BuildQueryable(
            ["User"],
            c => c.StoryId == query.StoryId
                 && c.ChapterId == query.ChapterId
                 && c.ParentCommentId == null
                 && !c.DeletedAt.HasValue
        );
 
        if (query.IsHidden.HasValue)
            dbQuery = dbQuery.Where(c => c.IsHidden == query.IsHidden.Value);
 
        if (query.IsGuest.HasValue)
        {
            dbQuery = query.IsGuest.Value
                ? dbQuery.Where(c => c.UserId == null)
                : dbQuery.Where(c => c.UserId != null);
        }
 
        var total = await dbQuery.CountAsync();
 
        var items = await dbQuery
            .OrderByDescending(c => c.CreatedAt)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(CommentResult.FromComment)
            .ToListAsync();
 
        return new PagedResult<CommentResult>(items, total, query.PageNumber, query.PageSize);
    }
 
    public async Task ToggleHideAsync(Guid commentId)
    {
        var comment = await _commentRepo.GetByIdAsync(commentId)
            ?? throw new ResponseErrorObject("Comment not found", StatusCodes.Status404NotFound);
 
        comment.IsHidden = !comment.IsHidden;
        await _baseRepo.UpdateAsync(comment);
    }
 
    public async Task DeleteAsync(Guid commentId)
    {
        _ = await _commentRepo.GetByIdAsync(commentId)
            ?? throw new ResponseErrorObject("Comment not found", StatusCodes.Status404NotFound);
 
        await _baseRepo.DeleteAsync(commentId);
    }
}