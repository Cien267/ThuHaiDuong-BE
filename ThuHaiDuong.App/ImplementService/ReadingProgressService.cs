using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using ThuHaiDuong.Application.InterfaceService;
using ThuHaiDuong.Application.Payloads.InputModels.ReadingProgress;
using ThuHaiDuong.Application.Payloads.Responses;
using ThuHaiDuong.Application.Payloads.ResultModels.ReadingProgress;
using ThuHaiDuong.Domain.Entities;
using ThuHaiDuong.Domain.InterfaceRepositories;
using ThuHaiDuong.Shared.Constants;
using ThuHaiDuong.Shared.Models;

namespace ThuHaiDuong.Application.ImplementService;

public class ReadingProgressService : IReadingProgressService
{
    private readonly IBaseRepository<Story>          _storyRepo;
    private readonly IBaseRepository<Chapter>        _chapterRepo;
    private readonly IBaseRepository<ReadingHistory> _historyBaseRepo;
    private readonly IBaseRepository<UserReadingProgress> _userReadingProgressRepo;
    private readonly IReadingProgressRepository      _progressRepo;
 
    public ReadingProgressService(
        IBaseRepository<Story>          storyRepo,
        IBaseRepository<Chapter>        chapterRepo,
        IBaseRepository<ReadingHistory> historyBaseRepo,
        IReadingProgressRepository      progressRepo,
        IBaseRepository<UserReadingProgress> userReadingProgressRepo)
    {
        _storyRepo       = storyRepo;
        _chapterRepo     = chapterRepo;
        _historyBaseRepo = historyBaseRepo;
        _progressRepo    = progressRepo;
        _userReadingProgressRepo = userReadingProgressRepo;
    }
 
    public async Task UpdateAsync(Guid userId, UpdateReadingProgressInput input)
    {
        // Validate story tồn tại và đang public
        var story = await _storyRepo.GetByIdAsync(input.StoryId);
        if (story == null || story.IsDeleted
            || (story.Status != StoryStatus.Publishing
                && story.Status != StoryStatus.Completed))
            throw new ResponseErrorObject(
                "Story not found.", StatusCodes.Status404NotFound);
 
        // Validate chapter tồn tại, Published, và thuộc đúng story
        var chapter = await _chapterRepo.GetByIdAsync(input.ChapterId);
        if (chapter == null || chapter.IsDeleted
            || chapter.StoryId != input.StoryId
            || chapter.Status != ChapterStatus.Published)
            throw new ResponseErrorObject(
                "Chapter not found.", StatusCodes.Status404NotFound);
 
        // Chạy song song: upsert progress + upsert history
        await Task.WhenAll(
            _progressRepo.UpsertProgressAsync(
                userId, input.StoryId, input.ChapterId, input.ChapterNumber),
            _progressRepo.UpsertHistoryAsync(
                userId, input.StoryId, input.ChapterId)
        );
    }
 
    public async Task<ReadingProgressResult?> GetProgressAsync(Guid userId, Guid storyId)
    {
        var progressQuery = _userReadingProgressRepo.BuildQueryable(["Story", "LastChapter"],
            p =>
                p.UserId == userId &&
                p.StoryId == storyId &&
                !p.IsDeleted);
        var progress = await progressQuery.FirstOrDefaultAsync();
        
        if (progress == null) return null;

        var readCountQuery = _historyBaseRepo.BuildQueryable([],
            h =>
                h.UserId == userId &&
                h.StoryId == storyId &&
                !h.IsDeleted);
        var readCount = await readCountQuery.CountAsync();
        var totalPublished = progress.Story.TotalChapters;
        
        return new ReadingProgressResult
        {
            StoryId                = storyId,
            StoryTitle             = progress.Story.Title,
            StorySlug              = progress.Story.Slug,
            TotalPublishedChapters = totalPublished,
            LastChapterId          = progress.LastChapterId,
            LastChapterNumber      = progress.LastChapterNumber,
            LastChapterTitle       = progress.LastChapter.Title,
            LastReadAt             = progress.LastReadAt,
            ReadChapterCount       = readCount,
        };
    }
 
    public async Task<PagedResult<ReadingHistoryItem>> GetHistoryAsync(
        Guid userId, ReadingHistoryQuery query)
    {
        var dbQuery = _historyBaseRepo.BuildQueryable([],
            h => h.UserId == userId && !h.IsDeleted && !h.Story.IsDeleted);
        if (query.StoryId.HasValue)
            dbQuery = dbQuery.Where(h => h.StoryId == query.StoryId.Value);
        var total = await dbQuery.CountAsync();
        
        var items = await dbQuery
            .OrderByDescending(h => h.ReadAt)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(ReadingHistoryItem.FromHistory)
            .ToListAsync();
        
        return new PagedResult<ReadingHistoryItem>(
            items, total, query.PageNumber, query.PageSize);
    }
 
    public async Task<HashSet<Guid>> GetReadChapterIdsAsync(Guid userId, Guid storyId)
    {
        return await _progressRepo.GetReadChapterIdsAsync(userId, storyId);
    }
 
    public async Task ClearHistoryAsync(Guid userId, Guid storyId)
    {
        // Xóa mềm toàn bộ history của user với story này
        var query = _historyBaseRepo.BuildQueryable(
            [],
            h => h.UserId == userId && h.StoryId == storyId && !h.IsDeleted
        );
 
        var histories = await query.ToListAsync();
 
        if (histories.Count == 0) return;
 
        foreach (var h in histories)
            await _historyBaseRepo.DeleteAsync(h.Id);
 
        // Xóa cả UserReadingProgress để nút "Đọc tiếp" reset về đầu
        await _progressRepo.UpsertProgressAsync(userId, storyId,
            Guid.Empty, 0); // reset — hoặc xóa hẳn nếu BaseRepo hỗ trợ
    }
}