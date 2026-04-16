using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using ThuHaiDuong.Application.InterfaceService;
using ThuHaiDuong.Application.Payloads.InputModels.Admin.Story;
using ThuHaiDuong.Application.Payloads.Responses;
using ThuHaiDuong.Application.Payloads.ResultModels.Admin.Story;
using ThuHaiDuong.Application.Payloads.ResultModels.User.Category;
using ThuHaiDuong.Application.Payloads.ResultModels.User.Chapter;
using ThuHaiDuong.Application.Payloads.ResultModels.User.Story;
using ThuHaiDuong.Application.Payloads.ResultModels.User.Tag;
using ThuHaiDuong.Domain.Entities;
using ThuHaiDuong.Domain.InterfaceRepositories;
using ThuHaiDuong.Shared.Constants;
using ThuHaiDuong.Shared.Extensions;
using ThuHaiDuong.Shared.Models;

namespace ThuHaiDuong.Application.ImplementService;

public class StoryService : IStoryService
{
    private readonly IBaseRepository<Story> _baseRepo;
    private readonly IBaseRepository<Author> _authorRepo;
    private readonly IBaseRepository<Category> _categoryRepo;
    private readonly IBaseRepository<Tag> _tagRepo;
    private readonly IStoryRepository _storyRepo;
 
    public StoryService(
        IBaseRepository<Story> baseRepo,
        IBaseRepository<Author> authorRepo,
        IBaseRepository<Category> categoryRepo,
        IBaseRepository<Tag> tagRepo,
        IStoryRepository storyRepo)
    {
        _baseRepo     = baseRepo;
        _authorRepo   = authorRepo;
        _categoryRepo = categoryRepo;
        _tagRepo      = tagRepo;
        _storyRepo    = storyRepo;
    }
 
    // ── CLIENT ────────────────────────────────────────────────────────────────
 
    public async Task<PagedResult<StorySummary>> GetListAsync(StoryQuery query)
    {
        var dbQuery = _baseRepo.BuildQueryable(
            ["Author", "StoryCategories.Category", "StoryTags.Tag"],
            s => !s.DeletedAt.HasValue
                 && (s.Status == StoryStatus.Publishing || s.Status == StoryStatus.Completed)
        );
 
        dbQuery = ApplyCommonFilters(dbQuery, query);
 
        var total = await dbQuery.CountAsync();
 
        dbQuery = ApplySorting(dbQuery, query.SortBy, query.SortDescending);
 
        var items = await dbQuery
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(StorySummary.FromStory)
            .ToListAsync();
 
        return new PagedResult<StorySummary>(items, total, query.PageNumber, query.PageSize);
    }
 
    public async Task<StoryDetail> GetBySlugAsync(string slug)
    {
        var story = await _storyRepo.GetDetailBySlugAsync(slug)
            ?? throw new ResponseErrorObject("Story not found", StatusCodes.Status404NotFound);
        
        return new StoryDetail
        {
            Id              = story.Id,
            Title           = story.Title,
            Slug            = story.Slug,
            Description     = story.Description,
            CoverImageUrl   = story.CoverImageUrl,
            Status          = story.Status,
            StoryType       = story.StoryType,
            ReleaseSchedule = story.ReleaseSchedule,
            NextChapterAt   = story.NextChapterAt,
            TotalChapters   = story.TotalChapters,
            TotalViews      = story.TotalViews,
            AverageRating   = story.AverageRating,
            RatingCount     = story.RatingCount,
            LastChapterAt   = story.LastChapterAt,
            CreatedAt       = story.CreatedAt,
            AuthorId        = story.AuthorId,
            AuthorName      = story.AuthorName,
            AuthorSlug      = story.Author.Slug,
            AuthorAvatarUrl = story.Author.AvatarUrl,
            Categories      = story.StoryCategories
                .Where(sc => !sc.Category.DeletedAt.HasValue && sc.Category.IsActive)
                .Select(sc => new CategorySummaryItem
                {
                    Id   = sc.CategoryId,
                    Name = sc.Category.Name,
                    Slug = sc.Category.Slug,
                }).ToList(),
            Tags            = story.StoryTags
                .Where(st => !st.Tag.DeletedAt.HasValue)
                .Select(st => new TagSummaryItem
                {
                    Id   = st.TagId,
                    Name = st.Tag.Name,
                    Slug = st.Tag.Slug,
                }).ToList(),
            Chapters        = story.Chapters
                .Select(c => new ChapterSummaryItem
                {
                    Id            = c.Id,
                    ChapterNumber = c.ChapterNumber,
                    Title         = c.Title,
                    IsVip         = c.IsVip,
                    PublishedAt   = c.PublishedAt,
                }).ToList(),
        };
    }
 
    // ── CREATE ────────────────────────────────────────────────────────────────
 
    public async Task<StoryResult> CreateAsync(CreateStoryInput input, Guid uploadedByUserId)
    {
        await ValidateAuthorExistsAsync(input.AuthorId);
        await ValidateCategoriesExistAsync(input.CategoryIds);
        await ValidateTagsExistAsync(input.TagIds);
        ValidateSerialFields(input.StoryType, input.ReleaseSchedule);
 
        var slug = string.IsNullOrWhiteSpace(input.Slug)
            ? input.Title.GenerateSlug()
            : input.Slug.Trim().ToLower();
 
        if (await _storyRepo.SlugExistsAsync(slug))
            throw new ResponseErrorObject("Slug already exists", StatusCodes.Status409Conflict);
 
        var author = await _authorRepo.GetByIdAsync(input.AuthorId);
 
        var story = new Story
        {
            Title           = input.Title.Trim(),
            Slug            = slug,
            AuthorId        = input.AuthorId,
            AuthorName      = author!.Name,         // cache display name
            UploadedByUserId = uploadedByUserId,
            SourceUrl       = input.SourceUrl?.Trim(),
            Description     = input.Description?.Trim(),
            CoverImageUrl   = input.CoverImageUrl?.Trim(),
            Status          = StoryStatus.Draft,    // luôn bắt đầu từ Draft
            StoryType       = input.StoryType,
            ReleaseSchedule = input.StoryType == ThuHaiDuong.Shared.Constants.StoryType.Serial
                ? input.ReleaseSchedule
                : null,
            NextChapterAt   = input.StoryType == ThuHaiDuong.Shared.Constants.StoryType.Serial
                ? input.NextChapterAt
                : null,
            ContentSource   = input.ContentSource,
        };
 
        await _baseRepo.CreateAsync(story);
 
        // Gắn categories và tags
        await _storyRepo.SyncCategoriesAsync(story.Id, input.CategoryIds);
        await _storyRepo.SyncTagsAsync(story.Id, input.TagIds);
 
        return await GetByIdAdminAsync(story.Id);
    }
 
    // ── UPDATE ────────────────────────────────────────────────────────────────
 
    public async Task<StoryResult> UpdateAsync(
        Guid id,
        UpdateStoryInput input,
        Guid requestUserId,
        string requestUserRole)
    {
        var story = await GetStoryOrThrowAsync(id);
 
        // Chỉ được sửa khi Draft hoặc Rejected
        if (story.Status != StoryStatus.Draft && story.Status != StoryStatus.Rejected)
            throw new ResponseErrorObject(
                "Story can only be edited when in Draft or Rejected status.",
                StatusCodes.Status409Conflict);
 
        // Contributor chỉ được sửa story của chính mình
        EnsureOwnershipOrAdmin(story, requestUserId, requestUserRole);
 
        await ValidateAuthorExistsAsync(input.AuthorId);
        await ValidateCategoriesExistAsync(input.CategoryIds);
        await ValidateTagsExistAsync(input.TagIds);
        ValidateSerialFields(input.StoryType, input.ReleaseSchedule);
 
        var slug = string.IsNullOrWhiteSpace(input.Slug)
            ? input.Title.GenerateSlug()
            : input.Slug.Trim().ToLower();
 
        if (await _storyRepo.SlugExistsAsync(slug, excludeId: id))
            throw new ResponseErrorObject("Slug already exists", StatusCodes.Status409Conflict);
 
        var author = await _authorRepo.GetByIdAsync(input.AuthorId);
 
        story.Title           = input.Title.Trim();
        story.Slug            = slug;
        story.AuthorId        = input.AuthorId;
        story.AuthorName      = author!.Name;
        story.SourceUrl       = input.SourceUrl?.Trim();
        story.Description     = input.Description?.Trim();
        story.CoverImageUrl   = input.CoverImageUrl?.Trim();
        story.StoryType       = input.StoryType;
        story.ReleaseSchedule = input.StoryType == ThuHaiDuong.Shared.Constants.StoryType.Serial
            ? input.ReleaseSchedule
            : null;
        story.NextChapterAt   = input.StoryType == ThuHaiDuong.Shared.Constants.StoryType.Serial
            ? input.NextChapterAt
            : null;
 
        // Reset RejectionReason khi sửa lại sau Rejected
        if (story.Status == StoryStatus.Rejected)
            story.RejectionReason = null;
 
        await _baseRepo.UpdateAsync(story);
 
        await _storyRepo.SyncCategoriesAsync(id, input.CategoryIds);
        await _storyRepo.SyncTagsAsync(id, input.TagIds);
 
        return await GetByIdAdminAsync(id);
    }
 
    // ── SUBMIT FOR REVIEW ─────────────────────────────────────────────────────
 
    public async Task SubmitForReviewAsync(Guid id, Guid requestUserId, string requestUserRole)
    {
        var story = await GetStoryOrThrowAsync(id);
 
        if (story.Status != StoryStatus.Draft && story.Status != StoryStatus.Rejected)
            throw new ResponseErrorObject(
                "Only Draft or Rejected stories can be submitted for review.",
                StatusCodes.Status409Conflict);
 
        EnsureOwnershipOrAdmin(story, requestUserId, requestUserRole);
 
        // Cần có ít nhất 1 chapter trước khi submit
        var hasChapter = await _baseRepo
            .BuildQueryable(["Chapters"], s => s.Id == id)
            .AnyAsync(s => s.Chapters.Any(c => !c.DeletedAt.HasValue));
 
        if (!hasChapter)
            throw new ResponseErrorObject(
                "Story must have at least one chapter before submitting for review.",
                StatusCodes.Status422UnprocessableEntity);
 
        story.Status = StoryStatus.PendingReview;
        await _baseRepo.UpdateAsync(story);
    }
 
    // ── REVIEW (SUPER ADMIN ONLY) ─────────────────────────────────────────────
 
    public async Task ReviewAsync(Guid id, ReviewStoryInput input)
    {
        var story = await GetStoryOrThrowAsync(id);
 
        if (story.Status != StoryStatus.PendingReview)
            throw new ResponseErrorObject(
                "Only PendingReview stories can be reviewed.",
                StatusCodes.Status409Conflict);
 
        if (!input.IsApproved)
        {
            if (string.IsNullOrWhiteSpace(input.RejectionReason))
                throw new ResponseErrorObject(
                    "Rejection reason is required when rejecting a story.",
                    StatusCodes.Status400BadRequest);
 
            story.Status          = StoryStatus.Rejected;
            story.RejectionReason = input.RejectionReason.Trim();
        }
        else
        {
            story.Status          = StoryStatus.Approved;
            story.RejectionReason = null;
        }
 
        await _baseRepo.UpdateAsync(story);
    }
 
    // ── UPDATE STATUS (ADMIN) ─────────────────────────────────────────────────
 
    public async Task UpdateStatusAsync(Guid id, UpdateStoryStatusInput input)
    {
        var story = await GetStoryOrThrowAsync(id);
 
        var allowed = StoryStatus.PublishableStatuses;
 
        if (!allowed.Contains(input.Status))
            throw new ResponseErrorObject(
                $"Status must be one of: {string.Join(", ", allowed)}",
                StatusCodes.Status400BadRequest);
 
        // Chỉ được publish sau khi đã Approved
        if (story.Status != StoryStatus.Approved
            && story.Status != StoryStatus.Publishing
            && story.Status != StoryStatus.Completed
            && story.Status != StoryStatus.Paused)
            throw new ResponseErrorObject(
                "Story must be Approved before changing publish status.",
                StatusCodes.Status409Conflict);
 
        story.Status = input.Status;
        await _baseRepo.UpdateAsync(story);
    }
 
    // ── ADMIN LIST ────────────────────────────────────────────────────────────
 
    public async Task<PagedResult<StoryResult>> GetListAdminAsync(StoryQuery query)
    {
        var dbQuery = _baseRepo.BuildQueryable(
            ["Author", "UploadedByUser", "StoryCategories.Category", "StoryTags.Tag"],
            s => !s.DeletedAt.HasValue
        );
 
        // Admin filters
        if (!string.IsNullOrWhiteSpace(query.Status))
            dbQuery = dbQuery.Where(s => s.Status == query.Status);
 
        if (!string.IsNullOrWhiteSpace(query.ContentSource))
            dbQuery = dbQuery.Where(s => s.ContentSource == query.ContentSource);
 
        if (query.UploadedByUserId.HasValue)
            dbQuery = dbQuery.Where(s => s.UploadedByUserId == query.UploadedByUserId.Value);
 
        // Common filters
        dbQuery = ApplyCommonFilters(dbQuery, query);
 
        var total = await dbQuery.CountAsync();
 
        dbQuery = ApplySorting(dbQuery, query.SortBy, query.SortDescending);
 
        var items = await dbQuery
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(StoryResult.FromStory)
            .ToListAsync();
 
        return new PagedResult<StoryResult>(items, total, query.PageNumber, query.PageSize);
    }
 
    public async Task<StoryResult> GetByIdAdminAsync(Guid id)
    {
        var query = _baseRepo.BuildQueryable(
            ["Author", "UploadedByUser", "StoryCategories.Category", "StoryTags.Tag"],
            s => s.Id == id && !s.DeletedAt.HasValue
        );
 
        return await query
            .Select(StoryResult.FromStory)
            .FirstOrDefaultAsync()
            ?? throw new ResponseErrorObject("Story not found", StatusCodes.Status404NotFound);
    }
 
    // ── DELETE ────────────────────────────────────────────────────────────────
 
    public async Task DeleteAsync(Guid id, Guid requestUserId, string requestUserRole)
    {
        var story = await GetStoryOrThrowAsync(id);
 
        // Contributor chỉ xóa được story của mình và chỉ khi Draft/Rejected
        if (requestUserRole == "Contributor")
        {
            EnsureOwnershipOrAdmin(story, requestUserId, requestUserRole);
 
            if (story.Status != StoryStatus.Draft && story.Status != StoryStatus.Rejected)
                throw new ResponseErrorObject(
                    "Contributors can only delete their own Draft or Rejected stories.",
                    StatusCodes.Status403Forbidden);
        }
 
        await _baseRepo.DeleteAsync(id);
    }
 
    // ── PRIVATE HELPERS ───────────────────────────────────────────────────────
 
    private async Task<Story> GetStoryOrThrowAsync(Guid id)
    {
        return await _baseRepo.GetByIdAsync(id)
            ?? throw new ResponseErrorObject("Story not found", StatusCodes.Status404NotFound);
    }
 
    // Filter chung cho cả client lẫn admin
    private static IQueryable<Story> ApplyCommonFilters(IQueryable<Story> query, StoryQuery filter)
    {
        if (!string.IsNullOrWhiteSpace(filter.Keyword))
        {
            var kw = filter.Keyword.ToLower();
            query = query.Where(s =>
                s.Title.ToLower().Contains(kw) ||
                s.AuthorName.ToLower().Contains(kw));
        }
 
        if (filter.CategoryId.HasValue)
            query = query.Where(s =>
                s.StoryCategories.Any(sc => sc.CategoryId == filter.CategoryId.Value));
 
        if (filter.TagId.HasValue)
            query = query.Where(s =>
                s.StoryTags.Any(st => st.TagId == filter.TagId.Value));
 
        if (filter.AuthorId.HasValue)
            query = query.Where(s => s.AuthorId == filter.AuthorId.Value);
 
        if (!string.IsNullOrWhiteSpace(filter.Country))
            query = query.Where(s => s.Author.Country == filter.Country.ToUpper());
 
        if (!string.IsNullOrWhiteSpace(filter.StoryType))
            query = query.Where(s => s.StoryType == filter.StoryType);
 
        return query;
    }
 
    private static IQueryable<Story> ApplySorting(
        IQueryable<Story> query, string sortBy, bool descending)
    {
        return sortBy.ToLower() switch
        {
            "totalviews"    => descending
                ? query.OrderByDescending(s => s.TotalViews)
                : query.OrderBy(s => s.TotalViews),
 
            "averagerating" => descending
                ? query.OrderByDescending(s => s.AverageRating)
                : query.OrderBy(s => s.AverageRating),
 
            "title"         => descending
                ? query.OrderByDescending(s => s.Title)
                : query.OrderBy(s => s.Title),
 
            "createdat"     => descending
                ? query.OrderByDescending(s => s.CreatedAt)
                : query.OrderBy(s => s.CreatedAt),
 
            // Default: lastchapterat — truyện mới update lên đầu
            _               => descending
                ? query.OrderByDescending(s => s.LastChapterAt)
                : query.OrderBy(s => s.LastChapterAt),
        };
    }
 
    private static void EnsureOwnershipOrAdmin(Story story, Guid userId, string role)
    {
        if (role is "Admin" or "SuperAdmin") return;
 
        if (story.UploadedByUserId != userId)
            throw new ResponseErrorObject(
                "You do not have permission to modify this story.",
                StatusCodes.Status403Forbidden);
    }
 
    private async Task ValidateAuthorExistsAsync(Guid authorId)
    {
        var author = await _authorRepo.GetByIdAsync(authorId);
        if (author == null || author.DeletedAt.HasValue)
            throw new ResponseErrorObject("Author not found", StatusCodes.Status404NotFound);
    }
 
    private async Task ValidateCategoriesExistAsync(List<Guid> categoryIds)
    {
        if (categoryIds.Count == 0) return;
 
        var existCount = await _categoryRepo
            .BuildQueryable([], c => categoryIds.Contains(c.Id) && !c.DeletedAt.HasValue)
            .CountAsync();
 
        if (existCount != categoryIds.Distinct().Count())
            throw new ResponseErrorObject(
                "One or more categories not found.",
                StatusCodes.Status404NotFound);
    }
 
    private async Task ValidateTagsExistAsync(List<Guid> tagIds)
    {
        if (tagIds.Count == 0) return;
 
        var existCount = await _tagRepo
            .BuildQueryable([], t => tagIds.Contains(t.Id) && !t.DeletedAt.HasValue)
            .CountAsync();
 
        if (existCount != tagIds.Distinct().Count())
            throw new ResponseErrorObject(
                "One or more tags not found.",
                StatusCodes.Status404NotFound);
    }
 
    private static void ValidateSerialFields(string storyType, string? releaseSchedule)
    {
        if (storyType != ThuHaiDuong.Shared.Constants.StoryType.Serial) return;
 
        var validSchedules = new[]
        {
            ReleaseSchedule.Daily,
            ReleaseSchedule.Weekly,
            ReleaseSchedule.BiWeekly,
            ReleaseSchedule.Monthly,
        };
 
        if (!string.IsNullOrWhiteSpace(releaseSchedule)
            && !validSchedules.Contains(releaseSchedule))
            throw new ResponseErrorObject(
                $"ReleaseSchedule must be one of: {string.Join(", ", validSchedules)}",
                StatusCodes.Status400BadRequest);
    }
}