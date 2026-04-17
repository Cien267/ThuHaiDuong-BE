using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using ThuHaiDuong.Application.InterfaceService;
using ThuHaiDuong.Application.Payloads.InputModels.Analytics;
using ThuHaiDuong.Application.Payloads.Responses;
using ThuHaiDuong.Application.Payloads.ResultModels.Analytics;
using ThuHaiDuong.Domain.Entities;
using ThuHaiDuong.Domain.InterfaceRepositories;
using ThuHaiDuong.Shared.Constants;

namespace ThuHaiDuong.Application.ImplementService;

public class AnalyticsService : IAnalyticsService
{
    private readonly IAnalyticsRepository _analyticsRepo;
    private readonly IBaseRepository<Story>  _storyRepo;
    private readonly IBaseRepository<Chapter>  _chapterRepo;
    private readonly IBaseRepository<DailyStoryStat>  _dailyStoryStatRepo;
    private readonly IBaseRepository<ChapterView>  _chapterViewRepo;
    private readonly IBaseRepository<User>  _userRepo;
    private readonly IBaseRepository<Comment>  _commentRepo;
    private readonly IBaseRepository<Rating>  _ratingRepo;
    private readonly IBaseRepository<Bookmark>  _bookMarkRepo;
 
    public AnalyticsService(IAnalyticsRepository analyticsRepo,
        IBaseRepository<Story> storyRepo,
        IBaseRepository<DailyStoryStat> dailyStoryStatRepo,
        IBaseRepository<Chapter>  chapterRepo,
        IBaseRepository<ChapterView>  chapterViewRepo,
        IBaseRepository<User> userRepo,
        IBaseRepository<Comment> commentRepo,
        IBaseRepository<Rating> ratingRepo,
        IBaseRepository<Bookmark> bookmarkRepo)
    {
        _analyticsRepo = analyticsRepo;
        _storyRepo =  storyRepo;
        _dailyStoryStatRepo = dailyStoryStatRepo;
        _chapterRepo = chapterRepo;
        _chapterViewRepo = chapterViewRepo;
        _userRepo = userRepo;
        _commentRepo = commentRepo;
        _ratingRepo = ratingRepo;
        _bookMarkRepo = bookmarkRepo;
    }
 
    // ── CLIENT ────────────────────────────────────────────────────────────────
 
    public async Task TrackChapterViewAsync(
        TrackChapterViewInput input,
        Guid? userId,
        string? ipAddress)
    {
        var view = new ChapterView
        {
            ChapterId = input.ChapterId,
            StoryId   = input.StoryId,
            UserId    = userId,
            SessionId = input.SessionId,
            IpAddress = ipAddress,
            ViewedAt  = DateTime.UtcNow,
        };
 
        // Fire-and-forget style: ghi view + tăng counter song song
        // Không await để không block response trả về cho client
        // Trong production nên dùng background queue (Channel<T> hoặc Hangfire)
        _ = Task.Run(async () =>
        {
            await _analyticsRepo.RecordChapterViewAsync(view);
            await _analyticsRepo.IncrementViewCountersAsync(
                input.ChapterId, input.StoryId);
        });
    }
 
    public async Task<List<StoryRankingItem>> GetTopStoriesAsync(StoryRankingQuery query)
    {
        var validPeriods = new[] { "today", "week", "month", "all" };
        if (!validPeriods.Contains(query.Period))
            query.Period = "today";
        
        if (query.Period == "all")
        {
            var storyQuery1 = _storyRepo.BuildQueryable([],
                s =>
                    !s.DeletedAt.HasValue &&
                    (s.Status == StoryStatus.Publishing ||
                     s.Status == StoryStatus.Completed));
            return await storyQuery1
                .OrderByDescending(s => s.TotalViews)
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(s => new StoryRankingItem
                {
                    StoryId       = s.Id,
                    Title         = s.Title,
                    Slug          = s.Slug,
                    CoverImageUrl = s.CoverImageUrl,
                    AuthorName    = s.AuthorName,
                    ViewCount     = (int)s.TotalViews,
                    TotalViews    = s.TotalViews,
                    AverageRating = s.AverageRating,
                    TotalChapters = s.TotalChapters,
                    CategoryNames = s.StoryCategories
                        .Where(sc => sc.Category.IsActive)
                        .Select(sc => sc.Category.Name)
                        .ToList(),
                })
                .ToListAsync();
        }
        
        var from = query.Period switch
        {
            "today" => DateOnly.FromDateTime(DateTime.UtcNow),
            "week"  => DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-7)),
            "month" => DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30)),
            _       => DateOnly.FromDateTime(DateTime.UtcNow),
        };

        var dailyStoryStatQuery = _dailyStoryStatRepo.BuildQueryable([],
            d => d.StatDate >= from);
        var storyViewsInPeriod = await dailyStoryStatQuery
            .GroupBy(d => d.StoryId)
            .Select(g => new
            {
                StoryId   = g.Key,
                ViewCount = g.Sum(d => d.ViewCount),
            })
            .OrderByDescending(x => x.ViewCount)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();
        
        if (storyViewsInPeriod.Count == 0)
            return [];
        
        var storyIds = storyViewsInPeriod.Select(x => x.StoryId).ToList();

        var storyQuery2 = _storyRepo.BuildQueryable(["StoryCategories", "StoryCategories.Category"],
            s =>
                storyIds.Contains(s.Id) &&
                !s.DeletedAt.HasValue &&
                (s.Status == StoryStatus.Publishing ||
                 s.Status == StoryStatus.Completed));
        
        var stories = await storyQuery2.ToDictionaryAsync(s => s.Id);
        
        return storyViewsInPeriod
            .Where(x => stories.ContainsKey(x.StoryId))
            .Select(x =>
            {
                var s = stories[x.StoryId];
                return new StoryRankingItem
                {
                    StoryId       = s.Id,
                    Title         = s.Title,
                    Slug          = s.Slug,
                    CoverImageUrl = s.CoverImageUrl,
                    AuthorName    = s.AuthorName,
                    ViewCount     = x.ViewCount,
                    TotalViews    = s.TotalViews,
                    AverageRating = s.AverageRating,
                    TotalChapters = s.TotalChapters,
                    CategoryNames = s.StoryCategories
                        .Where(sc => sc.Category.IsActive)
                        .Select(sc => sc.Category.Name)
                        .ToList(),
                };
            })
            .ToList();
    }
 
    // ── ADMIN ─────────────────────────────────────────────────────────────────
 
    public async Task<SiteOverviewResult> GetSiteOverviewAsync(SiteOverviewQuery query)
    {
        var (from, to) = ResolveDateRange(query.From, query.To, defaultDays: 30);
        
        var viewsQuery = _chapterViewRepo.BuildQueryable([], v => v.ViewedAt >= from && v.ViewedAt <= to);
        var views = await viewsQuery.CountAsync();

        var uniqueVisitorsQuery = _chapterViewRepo.BuildQueryable([], v => v.ViewedAt >= from && v.ViewedAt <= to);
        var uniqueVisitors = await uniqueVisitorsQuery
            .Select(v => v.SessionId != null ? v.SessionId : v.IpAddress)
            .Distinct()
            .CountAsync();

        var newUsersQuery = _userRepo.BuildQueryable([], u => u.CreatedAt >= from && u.CreatedAt <= to && !u.DeletedAt.HasValue);
        var newUsers = await newUsersQuery.CountAsync();
        
        var newCommentsQuery = _commentRepo.BuildQueryable([], c => c.CreatedAt >= from && c.CreatedAt <= to && !c.DeletedAt.HasValue);
        var newComments = await newCommentsQuery.CountAsync();

        var newRatingsQuery =
            _ratingRepo.BuildQueryable([], r => r.CreatedAt >= from && r.CreatedAt <= to && !r.DeletedAt.HasValue);
        var newRatings = await newRatingsQuery.CountAsync();

        var newBookmarksQuery =
            _bookMarkRepo.BuildQueryable([], b => b.CreatedAt >= from && b.CreatedAt <= to && !b.DeletedAt.HasValue);
        var newBookmarks = await newBookmarksQuery.CountAsync();

        var totalStoriesQuery = _storyRepo.BuildQueryable([], s => !s.DeletedAt.HasValue && (s.Status == StoryStatus.Publishing || s.Status == StoryStatus.Completed));
        var totalStories = await totalStoriesQuery.CountAsync();
        
        var totalChaptersQuery = _chapterRepo.BuildQueryable([], c => !c.DeletedAt.HasValue && c.Status == ChapterStatus.Published);
        var totalChapters =  await totalChaptersQuery.CountAsync();

        var totalUsersQuery = _userRepo.BuildQueryable([], u => !u.DeletedAt.HasValue);
        var totalUsers = await totalUsersQuery.CountAsync();
        
        return new SiteOverviewResult
        {
            TotalChapterViews = views,
            UniqueVisitors    = uniqueVisitors,
            NewUsers          = newUsers,
            NewComments       = newComments,
            NewRatings        = newRatings,
            NewBookmarks      = newBookmarks,
            TotalStories      = totalStories,
            TotalChapters     = totalChapters,
            TotalUsers        = totalUsers,
        };
    }
 
    public async Task<List<DailyTrafficResult>> GetDailyTrafficAsync(SiteOverviewQuery query)
    {
        var (from, to) = ResolveDateRange(query.From, query.To, defaultDays: 30);
        
        var viewsByDayQuery = _chapterViewRepo.BuildQueryable([], v => v.ViewedAt >= from && v.ViewedAt <= to);
        var viewsByDay = await viewsByDayQuery
            .GroupBy(v => v.ViewedAt.Date)
            .Select(g => new
            {
                Date           = g.Key,
                ChapterViews   = g.Count(),
                UniqueVisitors = g
                    .Select(v => v.SessionId != null ? v.SessionId : v.IpAddress)
                    .Distinct()
                    .Count(),
            })
            .ToListAsync();
        
        var usersByDayQuery = _userRepo.BuildQueryable([], u => u.CreatedAt >= from && u.CreatedAt <= to && !u.DeletedAt.HasValue);
        var usersByDay = await usersByDayQuery
            .GroupBy(u => u.CreatedAt.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Date, x => x.Count);
        
        var commentsByDayQuery = _commentRepo.BuildQueryable([], c => c.CreatedAt >= from && c.CreatedAt <= to && !c.DeletedAt.HasValue);
        var commentsByDay = await commentsByDayQuery
            .GroupBy(c => c.CreatedAt.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Date, x => x.Count);

        var ratingsByDayQuery =
            _ratingRepo.BuildQueryable([], r => r.CreatedAt >= from && r.CreatedAt <= to && !r.DeletedAt.HasValue);
        var ratingsByDay = await ratingsByDayQuery
            .GroupBy(r => r.CreatedAt.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Date, x => x.Count);
        
        var allDates = Enumerable
            .Range(0, (to.Date - from.Date).Days + 1)
            .Select(i => from.Date.AddDays(i))
            .ToList();
        
        var viewsLookup = viewsByDay.ToDictionary(x => x.Date);
        
        return allDates.Select(date => new DailyTrafficResult
        {
            Date           = DateOnly.FromDateTime(date),
            ChapterViews   = viewsLookup.TryGetValue(date, out var v) ? v.ChapterViews   : 0,
            UniqueVisitors = viewsLookup.TryGetValue(date, out v)     ? v.UniqueVisitors  : 0,
            NewUsers       = usersByDay.GetValueOrDefault(date,    0),
            NewComments    = commentsByDay.GetValueOrDefault(date,  0),
            NewRatings     = ratingsByDay.GetValueOrDefault(date,   0),
        }).ToList();
    }
 
    public async Task<List<ChapterRankingItem>> GetTopChaptersAsync(int limit = 10)
    {
        limit = Math.Clamp(limit, 1, 50);
        var chapterQuery = _chapterRepo.BuildQueryable([],
            c =>
                !c.DeletedAt.HasValue &&
                c.Status == ChapterStatus.Published &&
                !c.Story.DeletedAt.HasValue);
        
        return await chapterQuery
            .OrderByDescending(c => c.ViewCount)
            .Take(limit)
            .Select(c => new ChapterRankingItem
            {
                ChapterId     = c.Id,
                ChapterNumber = c.ChapterNumber,
                ChapterTitle  = c.Title,
                StoryId       = c.StoryId,
                StoryTitle    = c.Story.Title,
                StorySlug     = c.Story.Slug,
                ViewCount     = c.ViewCount,
            })
            .ToListAsync();
    }
 
    public async Task<StoryAnalyticsResult> GetStoryAnalyticsAsync(Guid storyId)
    {
        var storyQuery = _storyRepo.BuildQueryable([], s => s.Id == storyId && !s.DeletedAt.HasValue);
        var story = await storyQuery.FirstOrDefaultAsync();
        if (story == null) throw new ResponseErrorObject("Story not found", StatusCodes.Status404NotFound);
        
        var bookmarkCountQuery = _bookMarkRepo.BuildQueryable([], b => b.StoryId == storyId && !b.DeletedAt.HasValue);
        var bookmarkCount = await bookmarkCountQuery.CountAsync();

        var commentCountQuery =
            _commentRepo.BuildQueryable([], c => c.StoryId == storyId && !c.DeletedAt.HasValue && !c.IsHidden);
        var commentCount = await commentCountQuery.CountAsync();
        
        var from = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30));

        var dailyViewsQuery = _dailyStoryStatRepo.BuildQueryable([], d => d.StoryId == storyId && d.StatDate >= from);
        var dailyViews = await dailyViewsQuery
            .OrderBy(d => d.StatDate)
            .Select(d => new DailyStoryViewResult
            {
                Date           = d.StatDate,
                ViewCount      = d.ViewCount,
                UniqueVisitors = d.UniqueVisitors,
            })
            .ToListAsync();
        
        var topChaptersQuery = _chapterRepo.BuildQueryable([], 
            c =>
                c.StoryId == storyId &&
                c.Status == ChapterStatus.Published &&
                !c.DeletedAt.HasValue);
        
        var topChapters = await topChaptersQuery
            .OrderByDescending(c => c.ViewCount)
            .Take(5)
            .Select(c => new ChapterRankingItem
            {
                ChapterId     = c.Id,
                ChapterNumber = c.ChapterNumber,
                ChapterTitle  = c.Title,
                StoryId       = storyId,
                StoryTitle    = story.Title,
                StorySlug     = story.Slug,
                ViewCount     = c.ViewCount,
            })
            .ToListAsync();
        
        return new StoryAnalyticsResult
        {
            StoryId       = storyId,
            Title         = story.Title,
            TotalViews    = story.TotalViews,
            AverageRating = story.AverageRating,
            RatingCount   = story.RatingCount,
            BookmarkCount = bookmarkCount,
            CommentCount  = commentCount,
            DailyViews    = dailyViews,
            TopChapters   = topChapters,
        };
    }
 
    // ── HELPERS ───────────────────────────────────────────────────────────────
 
    private static (DateTime from, DateTime to) ResolveDateRange(
        DateTime? from, DateTime? to, int defaultDays)
    {
        var resolvedTo   = to   ?? DateTime.UtcNow;
        var resolvedFrom = from ?? resolvedTo.AddDays(-defaultDays);
 
        if (resolvedFrom > resolvedTo)
            throw new ResponseErrorObject(
                "From date must be before To date.",
                StatusCodes.Status400BadRequest);
 
        return (resolvedFrom, resolvedTo);
    }
}