using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using ThuHaiDuong.Application.InterfaceService;
using ThuHaiDuong.Application.Payloads.InputModels.Chapter;
using ThuHaiDuong.Application.Payloads.Responses;
using ThuHaiDuong.Application.Payloads.ResultModels.Chapter;
using ThuHaiDuong.Application.Payloads.ResultModels.User.Chapter;
using ThuHaiDuong.Domain.Entities;
using ThuHaiDuong.Domain.InterfaceRepositories;
using ThuHaiDuong.Shared.Constants;
using ThuHaiDuong.Shared.Models;

namespace ThuHaiDuong.Application.ImplementService;

public class ChapterService : IChapterService
{
    private readonly IBaseRepository<Chapter> _baseRepo;
    private readonly IBaseRepository<Story>   _storyRepo;
    private readonly IChapterRepository       _chapterRepo;
 
    public ChapterService(
        IBaseRepository<Chapter> baseRepo,
        IBaseRepository<Story>   storyRepo,
        IChapterRepository       chapterRepo)
    {
        _baseRepo    = baseRepo;
        _storyRepo   = storyRepo;
        _chapterRepo = chapterRepo;
    }
 
    // ── CLIENT ────────────────────────────────────────────────────────────────
 
    public async Task<ChapterReadResult> GetForReadingAsync(Guid chapterId)
    {
        var chapter = await _chapterRepo.GetChapterForReadingAsync(chapterId)
            ?? throw new ResponseErrorObject("Chapter not found", StatusCodes.Status404NotFound);
        var prev = await _chapterRepo.GetPrevChapterAsync(chapter);
        var next = await _chapterRepo.GetNextChapterAsync(chapter);
        
        return new ChapterReadResult
        {
            Id            = chapter.Id,
            ChapterNumber = chapter.ChapterNumber,
            Title         = chapter.Title,
            Content       = chapter.Content,
            IsVip         = chapter.IsVip,
            WordCount     = chapter.WordCount,
            PublishedAt   = chapter.PublishedAt,
            StoryId       = chapter.StoryId,
            StoryTitle    = chapter.Story.Title,
            StorySlug     = chapter.Story.Slug,
            PrevChapter   = prev != null ? new ChapterNavItem
            {
                Id            = prev.Id,
                ChapterNumber = prev.ChapterNumber,
                Title         = prev.Title,
                IsVip         = prev.IsVip,
            } : null,
            NextChapter   = next != null ? new ChapterNavItem
            {
                Id            = next.Id,
                ChapterNumber = next.ChapterNumber,
                Title         = next.Title,
                IsVip         = next.IsVip,
            } : null,
        };
    }
 
    public async Task<ChapterReadResult> GetForReadingByNumberAsync(
        Guid storyId, int chapterNumber)
    {
        var chapter = await _chapterRepo.GetChapterForReadingByNumberAsync(storyId, chapterNumber)
            ?? throw new ResponseErrorObject("Chapter not found", StatusCodes.Status404NotFound);
        
        var prev = await _chapterRepo.GetPrevChapterAsync(chapter);
        var next = await _chapterRepo.GetNextChapterAsync(chapter);
        
        return new ChapterReadResult
        {
            Id            = chapter.Id,
            ChapterNumber = chapter.ChapterNumber,
            Title         = chapter.Title,
            Content       = chapter.Content,
            IsVip         = chapter.IsVip,
            WordCount     = chapter.WordCount,
            PublishedAt   = chapter.PublishedAt,
            StoryId       = chapter.StoryId,
            StoryTitle    = chapter.Story.Title,
            StorySlug     = chapter.Story.Slug,
            PrevChapter   = prev != null ? new ChapterNavItem
            {
                Id            = prev.Id,
                ChapterNumber = prev.ChapterNumber,
                Title         = prev.Title,
                IsVip         = prev.IsVip,
            } : null,
            NextChapter   = next != null ? new ChapterNavItem
            {
                Id            = next.Id,
                ChapterNumber = next.ChapterNumber,
                Title         = next.Title,
                IsVip         = next.IsVip,
            } : null,
        };
    }
 
    public async Task<PagedResult<ChapterListItem>> GetListAsync(Guid storyId)
    {
        // Client chỉ thấy Published, không phân trang — trả hết (chapter list thường đủ nhỏ)
        var query = _baseRepo.BuildQueryable(
            [],
            c => c.StoryId == storyId
                 && c.Status == ChapterStatus.Published
                 && !c.IsDeleted
        );
 
        var total = await query.CountAsync();
 
        var items = await query
            .OrderBy(c => c.ChapterNumber)
            .Select(ChapterListItem.FromChapter)
            .ToListAsync();
 
        return new PagedResult<ChapterListItem>(items, total, 1, total);
    }
 
    // ── ADMIN ─────────────────────────────────────────────────────────────────
 
    public async Task<PagedResult<ChapterListItem>> GetListAdminAsync(ChapterQuery query)
    {
        var dbQuery = _baseRepo.BuildQueryable(
            [],
            c => c.StoryId == query.StoryId && !c.IsDeleted
        );
 
        if (!string.IsNullOrWhiteSpace(query.Status))
            dbQuery = dbQuery.Where(c => c.Status == query.Status);
 
        if (query.IsVip.HasValue)
            dbQuery = dbQuery.Where(c => c.IsVip == query.IsVip.Value);
 
        var total = await dbQuery.CountAsync();
 
        dbQuery = query.SortBy.ToLower() switch
        {
            "createdat"     => query.SortDescending
                ? dbQuery.OrderByDescending(c => c.CreatedAt)
                : dbQuery.OrderBy(c => c.CreatedAt),
            "viewcount"     => query.SortDescending
                ? dbQuery.OrderByDescending(c => c.ViewCount)
                : dbQuery.OrderBy(c => c.ViewCount),
            _               => query.SortDescending
                ? dbQuery.OrderByDescending(c => c.ChapterNumber)
                : dbQuery.OrderBy(c => c.ChapterNumber),
        };
 
        var items = await dbQuery
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(ChapterListItem.FromChapter)
            .ToListAsync();
 
        return new PagedResult<ChapterListItem>(items, total, query.PageNumber, query.PageSize);
    }
 
    // ── CREATE ────────────────────────────────────────────────────────────────
 
    public async Task<ChapterResult> CreateAsync(
        CreateChapterInput input,
        Guid requestUserId,
        string requestUserRole)
    {
        var story = await GetStoryOrThrowAsync(input.StoryId);
 
        // Story phải đã qua Approved trở lên mới cho thêm chapter
        EnsureStoryIsEditable(story);
 
        // Contributor chỉ thêm chapter cho story của mình
        EnsureStoryOwnershipOrAdmin(story, requestUserId, requestUserRole);
 
        // Validate status input
        EnsureValidStatus(input.Status);
 
        // Tự động tính ChapterNumber nếu không truyền
        int chapterNumber;
        if (input.ChapterNumber.HasValue)
        {
            chapterNumber = input.ChapterNumber.Value;
            if (await _chapterRepo.ChapterNumberExistsAsync(input.StoryId, chapterNumber))
                throw new ResponseErrorObject(
                    $"Chapter number {chapterNumber} already exists in this story.",
                    StatusCodes.Status409Conflict);
        }
        else
        {
            var maxNumber = await _chapterRepo.GetMaxChapterNumberAsync(input.StoryId);
            chapterNumber = maxNumber + 1;
        }
 
        var chapter = new Chapter
        {
            StoryId       = input.StoryId,
            ChapterNumber = chapterNumber,
            Title         = input.Title.Trim(),
            Content       = input.Content,
            Status        = input.Status,
            IsVip         = input.IsVip,
            WordCount     = CountWords(input.Content),
            PublishedAt   = input.Status == ChapterStatus.Published
                ? DateTime.UtcNow
                : null,
        };
 
        await _baseRepo.CreateAsync(chapter);
 
        // Sync TotalChapters + LastChapterAt trên Story
        await _chapterRepo.SyncStoryCountersAsync(input.StoryId);
 
        return await GetByIdAdminAsync(chapter.Id);
    }
 
    // ── UPDATE ────────────────────────────────────────────────────────────────
 
    public async Task<ChapterResult> UpdateAsync(
        Guid id,
        UpdateChapterInput input,
        Guid requestUserId,
        string requestUserRole)
    {
        var chapter = await GetChapterOrThrowAsync(id);
        var story   = await GetStoryOrThrowAsync(chapter.StoryId);
 
        EnsureStoryOwnershipOrAdmin(story, requestUserId, requestUserRole);
 
        chapter.Title     = input.Title.Trim();
        chapter.Content   = input.Content;
        chapter.IsVip     = input.IsVip;
        chapter.WordCount = CountWords(input.Content);
 
        await _baseRepo.UpdateAsync(chapter);
 
        return await GetByIdAdminAsync(id);
    }
 
    // ── UPDATE STATUS ─────────────────────────────────────────────────────────
 
    public async Task UpdateStatusAsync(
        Guid id,
        UpdateChapterStatusInput input,
        Guid requestUserId,
        string requestUserRole)
    {
        EnsureValidStatus(input.Status);
 
        var chapter = await GetChapterOrThrowAsync(id);
        var story   = await GetStoryOrThrowAsync(chapter.StoryId);
 
        EnsureStoryOwnershipOrAdmin(story, requestUserId, requestUserRole);
 
        var wasPublished = chapter.Status == ChapterStatus.Published;
        var willPublish  = input.Status == ChapterStatus.Published;
 
        chapter.Status = input.Status;
 
        // Ghi nhận thời điểm publish lần đầu
        if (willPublish && chapter.PublishedAt == null)
            chapter.PublishedAt = DateTime.UtcNow;
 
        await _baseRepo.UpdateAsync(chapter);
 
        // Sync story counters nếu status published thay đổi
        if (wasPublished != willPublish)
            await _chapterRepo.SyncStoryCountersAsync(chapter.StoryId);
    }
 
    // ── DELETE ────────────────────────────────────────────────────────────────
 
    public async Task DeleteAsync(
        Guid id,
        Guid requestUserId,
        string requestUserRole)
    {
        var chapter = await GetChapterOrThrowAsync(id);
        var story   = await GetStoryOrThrowAsync(chapter.StoryId);
 
        EnsureStoryOwnershipOrAdmin(story, requestUserId, requestUserRole);
 
        var wasPublished = chapter.Status == ChapterStatus.Published;
 
        await _baseRepo.DeleteAsync(id);
 
        // Sync TotalChapters + LastChapterAt ngay lập tức
        if (wasPublished)
            await _chapterRepo.SyncStoryCountersAsync(chapter.StoryId);
    }
 
    // ── PRIVATE HELPERS ───────────────────────────────────────────────────────
 
    private async Task<Story> GetStoryOrThrowAsync(Guid storyId)
    {
        return await _storyRepo.GetByIdAsync(storyId)
            ?? throw new ResponseErrorObject("Story not found", StatusCodes.Status404NotFound);
    }
 
    private async Task<Chapter> GetChapterOrThrowAsync(Guid id)
    {
        return await _baseRepo.GetByIdAsync(id)
            ?? throw new ResponseErrorObject("Chapter not found", StatusCodes.Status404NotFound);
    }
 
    private async Task<ChapterResult> GetByIdAdminAsync(Guid id)
    {
        var query = _baseRepo.BuildQueryable(
            ["Story"],
            c => c.Id == id && !c.IsDeleted
        );
 
        return await query
            .Select(ChapterResult.FromChapter)
            .FirstOrDefaultAsync()
            ?? throw new ResponseErrorObject("Chapter not found", StatusCodes.Status404NotFound);
    }
 
    // Story phải ở trạng thái có thể thêm chapter
    // Approved/Publishing/Completed/Paused đều cho phép (admin muốn chuẩn bị trước)
    // Chỉ chặn Draft, PendingReview, Rejected
    private static void EnsureStoryIsEditable(Story story)
    {
        var blocked = new[]
        {
            StoryStatus.Draft,
            StoryStatus.PendingReview,
            StoryStatus.Rejected,
        };
 
        if (blocked.Contains(story.Status))
            throw new ResponseErrorObject(
                $"Cannot add chapters to a story with status '{story.Status}'. " +
                "Story must be Approved or beyond.",
                StatusCodes.Status409Conflict);
    }
 
    private static void EnsureStoryOwnershipOrAdmin(
        Story story, Guid userId, string role)
    {
        if (role is "Admin" or "SuperAdmin") return;
 
        if (story.UploadedByUserId != userId)
            throw new ResponseErrorObject(
                "You do not have permission to modify chapters of this story.",
                StatusCodes.Status403Forbidden);
    }
 
    private static void EnsureValidStatus(string status)
    {
        var valid = new[]
        {
            ChapterStatus.Draft,
            ChapterStatus.Published,
            ChapterStatus.Hidden,
        };
 
        if (!valid.Contains(status))
            throw new ResponseErrorObject(
                $"Status must be one of: {string.Join(", ", valid)}",
                StatusCodes.Status400BadRequest);
    }
 
    // Đếm số từ từ HTML content — strip tags trước rồi mới đếm
    private static int CountWords(string htmlContent)
    {
        if (string.IsNullOrWhiteSpace(htmlContent)) return 0;
 
        // Strip HTML tags
        var text = Regex.Replace(htmlContent, "<.*?>", " ");
 
        // Decode HTML entities
        text = System.Net.WebUtility.HtmlDecode(text);
 
        // Đếm từ (split bằng whitespace)
        var words = text.Split(
            new[] { ' ', '\t', '\n', '\r' },
            StringSplitOptions.RemoveEmptyEntries);
 
        return words.Length;
    }
}