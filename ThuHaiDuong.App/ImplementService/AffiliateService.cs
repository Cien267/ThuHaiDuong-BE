using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using ThuHaiDuong.Application.InterfaceService;
using ThuHaiDuong.Application.Payloads.InputModels.Affiliate;
using ThuHaiDuong.Application.Payloads.Responses;
using ThuHaiDuong.Application.Payloads.ResultModels.Affiliate;
using ThuHaiDuong.Domain.Entities;
using ThuHaiDuong.Domain.InterfaceRepositories;
using ThuHaiDuong.Shared.Constants;
using ThuHaiDuong.Shared.Models;

namespace ThuHaiDuong.Application.ImplementService;

public class AffiliateService : IAffiliateService
{
    private readonly IBaseRepository<AffiliateLink>  _baseRepo;
    private readonly IBaseRepository<AffiliateClick> _clickRepo;
    private readonly IAffiliateRepository            _affiliateRepo;
 
    public AffiliateService(
        IBaseRepository<AffiliateLink>  baseRepo,
        IBaseRepository<AffiliateClick> clickRepo,
        IAffiliateRepository            affiliateRepo)
    {
        _baseRepo      = baseRepo;
        _clickRepo     = clickRepo;
        _affiliateRepo = affiliateRepo;
    }
 
    // ── CLIENT ────────────────────────────────────────────────────────────────
 
    public async Task<List<AffiliateDisplayResult>> GetDisplayLinksAsync(
        Guid storyId, Guid chapterId)
    {
        var now = DateTime.UtcNow;
        var candidatesQuery = _baseRepo.BuildQueryable(
            ["AffiliateLinkStories", "AffiliateLinkChapters"],
            l =>
                l.IsActive &&
                !l.IsDeleted &&
                (l.StartDate == null || l.StartDate <= now) &&
                (l.EndDate == null || l.EndDate >= now));
        var allCandidates = await candidatesQuery.ToListAsync();
        
        var scored = allCandidates
            .Select(l => new
            {
                Link  = l,
                Score = l.AffiliateLinkChapters.Any(c => c.ChapterId == chapterId) ? 3
                    : l.AffiliateLinkStories.Any(s => s.StoryId == storyId) ? 2
                    : l.AffiliateLinkStories.Count == 0
                      && l.AffiliateLinkChapters.Count == 0 ? 1
                    : 0,   // gắn story/chapter khác → không hiển thị ở đây
            })
            .Where(x => x.Score > 0)
            .ToList();
        
        var result = scored
            .GroupBy(x => x.Link.Placement)
            .Select(g => g
                .OrderByDescending(x => x.Score)
                .ThenByDescending(x => x.Link.Priority)
                .First().Link)
            .Select(l => new AffiliateDisplayResult
            {
                Id          = l.Id,
                Name        = l.Name,
                Placement   = l.Placement,
                RedirectUrl = $"/go/{l.TrackingCode}",
            })
            .ToList();
 
        return result;
    }
 
    public async Task<string> TrackAndGetTargetUrlAsync(
        string trackingCode,
        Guid? userId,
        Guid? chapterId,
        string? ipAddress,
        string? userAgent,
        string? referrer)
    {
        var link = await _affiliateRepo.GetByTrackingCodeAsync(trackingCode)
            ?? throw new ResponseErrorObject("Link not found", StatusCodes.Status404NotFound);
 
        // Chống spam: 1 IP + link = 1 click trong 1 giờ
        var isSpam = !string.IsNullOrWhiteSpace(ipAddress)
            && await _affiliateRepo.IsSpamClickAsync(link.Id, ipAddress!);
 
        if (!isSpam)
        {
            var click = new AffiliateClick
            {
                AffiliateLinkId = link.Id,
                UserId          = userId,
                ChapterId       = chapterId,
                IpAddress       = ipAddress,
                UserAgent       = userAgent,
                Referrer        = referrer,
                ClickedAt       = DateTime.UtcNow,
            };
 
            await _clickRepo.CreateAsync(click);
        }
 
        return link.TargetUrl;
    }
 
    // ── ADMIN — CRUD ──────────────────────────────────────────────────────────
 
    public async Task<PagedResult<AffiliateLinkResult>> GetListAsync(AffiliateLinkQuery query)
    {
        var dbQuery = _baseRepo.BuildQueryable(
            ["AffiliateClicks", "AffiliateLinkStories.Story", "AffiliateLinkChapters.Chapter.Story"],
            l => !l.IsDeleted
        );
 
        if (!string.IsNullOrWhiteSpace(query.Name))
        {
            var name = query.Name.ToLower();
            dbQuery = dbQuery.Where(l => l.Name.ToLower().Contains(name));
        }
 
        if (!string.IsNullOrWhiteSpace(query.Placement))
            dbQuery = dbQuery.Where(l => l.Placement == query.Placement);
 
        if (query.IsActive.HasValue)
            dbQuery = dbQuery.Where(l => l.IsActive == query.IsActive.Value);
 
        var total = await dbQuery.CountAsync();
 
        dbQuery = query.SortBy.ToLower() switch
        {
            "name"      => query.SortDescending
                ? dbQuery.OrderByDescending(l => l.Name)
                : dbQuery.OrderBy(l => l.Name),
            "createdat" => query.SortDescending
                ? dbQuery.OrderByDescending(l => l.CreatedAt)
                : dbQuery.OrderBy(l => l.CreatedAt),
            _           => query.SortDescending
                ? dbQuery.OrderByDescending(l => l.Priority)
                : dbQuery.OrderBy(l => l.Priority),
        };
 
        var items = await dbQuery
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(AffiliateLinkResult.FromLink)
            .ToListAsync();
 
        return new PagedResult<AffiliateLinkResult>(items, total, query.PageNumber, query.PageSize);
    }
 
    public async Task<AffiliateLinkResult> GetByIdAsync(Guid id)
    {
        var query = _baseRepo.BuildQueryable(
            ["AffiliateClicks", "AffiliateLinkStories.Story", "AffiliateLinkChapters.Chapter.Story"],
            l => l.Id == id && !l.IsDeleted
        );
 
        return await query
            .Select(AffiliateLinkResult.FromLink)
            .FirstOrDefaultAsync()
            ?? throw new ResponseErrorObject("Affiliate link not found", StatusCodes.Status404NotFound);
    }
 
    public async Task<AffiliateLinkResult> CreateAsync(CreateAffiliateLinkInput input)
    {
        ValidatePlacement(input.Placement);
        ValidateDateRange(input.StartDate, input.EndDate);
 
        var trackingCode = string.IsNullOrWhiteSpace(input.TrackingCode)
            ? GenerateTrackingCode()
            : input.TrackingCode.Trim().ToLower();
 
        if (await _affiliateRepo.TrackingCodeExistsAsync(trackingCode))
            throw new ResponseErrorObject(
                "Tracking code already exists.", StatusCodes.Status409Conflict);
 
        var link = new AffiliateLink
        {
            Name         = input.Name.Trim(),
            TargetUrl    = input.TargetUrl.Trim(),
            TrackingCode = trackingCode,
            Placement    = input.Placement,
            Priority     = input.Priority,
            IsActive     = input.IsActive,
            StartDate    = input.StartDate,
            EndDate      = input.EndDate,
        };
 
        await _baseRepo.CreateAsync(link);
 
        await _affiliateRepo.SyncStoryTargetsAsync(link.Id, input.StoryIds);
        await _affiliateRepo.SyncChapterTargetsAsync(link.Id, input.ChapterIds);
 
        return await GetByIdAsync(link.Id);
    }
 
    public async Task<AffiliateLinkResult> UpdateAsync(Guid id, UpdateAffiliateLinkInput input)
    {
        var link = await _baseRepo.GetByIdAsync(id)
            ?? throw new ResponseErrorObject("Affiliate link not found", StatusCodes.Status404NotFound);
 
        ValidatePlacement(input.Placement);
        ValidateDateRange(input.StartDate, input.EndDate);
 
        link.Name      = input.Name.Trim();
        link.TargetUrl = input.TargetUrl.Trim();
        link.Placement = input.Placement;
        link.Priority  = input.Priority;
        link.IsActive  = input.IsActive;
        link.StartDate = input.StartDate;
        link.EndDate   = input.EndDate;
 
        await _baseRepo.UpdateAsync(link);
 
        await _affiliateRepo.SyncStoryTargetsAsync(id, input.StoryIds);
        await _affiliateRepo.SyncChapterTargetsAsync(id, input.ChapterIds);
 
        return await GetByIdAsync(id);
    }
 
    public async Task DeleteAsync(Guid id)
    {
        _ = await _baseRepo.GetByIdAsync(id)
            ?? throw new ResponseErrorObject("Affiliate link not found", StatusCodes.Status404NotFound);
 
        await _baseRepo.DeleteAsync(id);
    }
 
    // ── ADMIN — REPORTS ───────────────────────────────────────────────────────
 
    public async Task<List<AffiliateDailyStatResult>> GetDailyStatsAsync(
        Guid? linkId, DateTime from, DateTime to)
    {
        if (from > to)
            throw new ResponseErrorObject(
                "FromDate must be before ToDate.", StatusCodes.Status400BadRequest);
 
        var raw = await _affiliateRepo.GetDailyStatsAsync(linkId, from, to);
        return raw.GroupBy(c => c.ClickedAt.Date)
            .Select(g => new
            {
                Date        = g.Key,
                TotalClicks = g.Count(),
                UniqueIps   = g.Select(c => c.IpAddress).Distinct().Count(),
            })
            .OrderBy(x => x.Date)
            .Select(x => new AffiliateDailyStatResult
            {
                Date        = DateOnly.FromDateTime(x.Date),
                TotalClicks = x.TotalClicks,
                UniqueIps   = x.UniqueIps,
            }).ToList();
    }
 
    public async Task<List<AffiliateLinkStatResult>> GetLinkStatsAsync(
        DateTime? from, DateTime? to)
    {
        var query = _clickRepo.BuildQueryable(
            [], null);
        
        if (from.HasValue) query = query.Where(c => c.ClickedAt >= from.Value);
        if (to.HasValue)   query = query.Where(c => c.ClickedAt <= to.Value);
        
        var stats = await query
            .GroupBy(c => c.AffiliateLinkId)
            .Select(g => new
            {
                AffiliateLinkId = g.Key,
                TotalClicks     = g.Count(),
                UniqueIps       = g.Select(c => c.IpAddress).Distinct().Count(),
                LastClickedAt   = g.Max(c => (DateTime?)c.ClickedAt),
            })
            .ToListAsync();
        
        var linkIds = stats.Select(s => s.AffiliateLinkId).ToList();
        
        var linksQuery = _baseRepo.BuildQueryable(new List<string>(),
            l => linkIds.Contains(l.Id) && !l.IsDeleted);
        
        var links = await linksQuery.ToDictionaryAsync(l => l.Id);
        return stats
            .Where(s => links.ContainsKey(s.AffiliateLinkId))
            .Select(s => new AffiliateLinkStatResult
            {
                AffiliateLinkId = s.AffiliateLinkId,
                LinkName        = links[s.AffiliateLinkId].Name,
                TrackingCode    = links[s.AffiliateLinkId].TrackingCode,
                Placement       = links[s.AffiliateLinkId].Placement,
                TotalClicks     = s.TotalClicks,
                UniqueIps       = s.UniqueIps,
                LastClickedAt   = s.LastClickedAt,
            })
            .OrderByDescending(s => s.TotalClicks)
            .ToList();
    }
 
    public async Task<PagedResult<AffiliateClickResult>> GetClicksAsync(
        AffiliateClickReportQuery query)
    {
        var dbQuery = _clickRepo.BuildQueryable(
            ["AffiliateLink", "User", "Chapter.Story"],
            _ => true
        );
 
        if (query.AffiliateLinkId.HasValue)
            dbQuery = dbQuery.Where(c => c.AffiliateLinkId == query.AffiliateLinkId.Value);
 
        if (query.ChapterId.HasValue)
            dbQuery = dbQuery.Where(c => c.ChapterId == query.ChapterId.Value);
 
        if (query.StoryId.HasValue)
            dbQuery = dbQuery.Where(c => c.Chapter != null
                                         && c.Chapter.StoryId == query.StoryId.Value);
 
        if (query.FromDate.HasValue)
            dbQuery = dbQuery.Where(c => c.ClickedAt >= query.FromDate.Value);
 
        if (query.ToDate.HasValue)
            dbQuery = dbQuery.Where(c => c.ClickedAt <= query.ToDate.Value);
 
        var total = await dbQuery.CountAsync();
 
        var items = await dbQuery
            .OrderByDescending(c => c.ClickedAt)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(AffiliateClickResult.FromClick)
            .ToListAsync();
 
        return new PagedResult<AffiliateClickResult>(items, total, query.PageNumber, query.PageSize);
    }
 
    // ── HELPERS ───────────────────────────────────────────────────────────────
 
    // Generate tracking code ngẫu nhiên 8 ký tự URL-safe
    private static string GenerateTrackingCode()
    {
        const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
        var random = new Random();
        return new string(
            Enumerable.Repeat(chars, 8)
                .Select(s => s[random.Next(s.Length)])
                .ToArray());
    }
 
    private static void ValidatePlacement(string placement)
    {
        if (!AffiliatePlacement.All.Contains(placement))
            throw new ResponseErrorObject(
                $"Placement must be one of: {string.Join(", ", AffiliatePlacement.All)}",
                StatusCodes.Status400BadRequest);
    }
 
    private static void ValidateDateRange(DateTime? start, DateTime? end)
    {
        if (start.HasValue && end.HasValue && start.Value > end.Value)
            throw new ResponseErrorObject(
                "StartDate must be before EndDate.",
                StatusCodes.Status400BadRequest);
    }
}