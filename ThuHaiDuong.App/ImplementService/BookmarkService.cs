using Microsoft.AspNetCore.Http;
using ThuHaiDuong.Application.InterfaceService;
using ThuHaiDuong.Application.Payloads.Responses;
using ThuHaiDuong.Application.Payloads.ResultModels.Bookmark;
using ThuHaiDuong.Application.Payloads.ResultModels.User.Bookmark;
using ThuHaiDuong.Domain.Entities;
using ThuHaiDuong.Domain.InterfaceRepositories;
using ThuHaiDuong.Shared.Constants;

namespace ThuHaiDuong.Application.ImplementService;

public class BookmarkService : IBookmarkService
{
    private readonly IBaseRepository<Bookmark> _baseRepo;
    private readonly IBaseRepository<Story>    _storyRepo;
    private readonly IBookmarkRepository       _bookmarkRepo;
 
    public BookmarkService(
        IBaseRepository<Bookmark> baseRepo,
        IBaseRepository<Story>    storyRepo,
        IBookmarkRepository       bookmarkRepo)
    {
        _baseRepo     = baseRepo;
        _storyRepo    = storyRepo;
        _bookmarkRepo = bookmarkRepo;
    }
 
    public async Task<BookmarkToggleResult> ToggleAsync(Guid userId, Guid storyId)
    {
        // Validate story tồn tại và đang public
        var story = await _storyRepo.GetByIdAsync(storyId);
        if (story == null || story.IsDeleted
                          || (story.Status != StoryStatus.Publishing
                              && story.Status != StoryStatus.Completed))
            throw new ResponseErrorObject("Story not found", StatusCodes.Status404NotFound);
 
        var existing = await _bookmarkRepo.GetAsync(userId, storyId);
 
        if (existing != null)
        {
            // Đã bookmark → xóa (soft delete)
            await _baseRepo.DeleteAsync(existing.Id);
            return new BookmarkToggleResult { IsBookmarked = false, StoryId = storyId };
        }
        else
        {
            // Chưa bookmark → thêm mới
            var bookmark = new Bookmark
            {
                UserId  = userId,
                StoryId = storyId,
            };
            await _baseRepo.CreateAsync(bookmark);
            return new BookmarkToggleResult { IsBookmarked = true, StoryId = storyId };
        }
    }
 
    public async Task<bool> IsBookmarkedAsync(Guid userId, Guid storyId)
    {
        return await _bookmarkRepo.ExistsAsync(userId, storyId);
    }
 
    public async Task<List<BookmarkResult>> GetUserBookmarksAsync(Guid userId)
    {
        var bookmarks = await _bookmarkRepo.GetUserBookmarksByIdAsync(userId);
        var storyIds = bookmarks.Select(b => b.StoryId).ToList();
        var progresses = await _bookmarkRepo.GetUserReadingProgressByStoryIdsAsync(userId, storyIds);
        var result = new List<BookmarkResult>{};
        
        foreach (var bookmark in bookmarks)
        {
            var item = new BookmarkResult
            {
                StoryId            = bookmark.StoryId,
                StoryTitle         = bookmark.Story.Title,
                StorySlug          = bookmark.Story.Slug,
                StoryCoverImageUrl = bookmark.Story.CoverImageUrl,
                StoryStatus        = bookmark.Story.Status,
                TotalChapters      = bookmark.Story.TotalChapters,
                LastChapterAt      = bookmark.Story.LastChapterAt,
                BookmarkedAt       = bookmark.CreatedAt,
                LastReadChapter    = null,
            };
            if (progresses.TryGetValue(bookmark.StoryId, out var progress))
            {
                item.LastReadChapter = new ChapterProgressItem
                {
                    ChapterId     = progress.LastChapterId,
                    ChapterNumber = progress.LastChapterNumber,
                    ChapterTitle  = progress.LastChapter.Title,
                    LastReadAt    = progress.LastReadAt,
                };
            }

            result.Add(item);
        }
 
        return result;
    }
}