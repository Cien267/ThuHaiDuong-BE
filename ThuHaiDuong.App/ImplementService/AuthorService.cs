using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using ThuHaiDuong.Application.InterfaceService;
using ThuHaiDuong.Application.Payloads.InputModels.Admin.Author;
using ThuHaiDuong.Application.Payloads.Responses;
using ThuHaiDuong.Application.Payloads.ResultModels.Admin.Author;
using ThuHaiDuong.Application.Payloads.ResultModels.User.Author;
using ThuHaiDuong.Domain.Entities;
using ThuHaiDuong.Domain.InterfaceRepositories;
using ThuHaiDuong.Shared.Extensions;
using ThuHaiDuong.Shared.Models;

namespace ThuHaiDuong.Application.ImplementService;

public class AuthorService : IAuthorService
{
    private readonly IBaseRepository<Author> _baseRepo;
    private readonly IAuthorRepository _authorRepo;
 
    public AuthorService(
        IBaseRepository<Author> baseRepo,
        IAuthorRepository authorRepo)
    {
        _baseRepo = baseRepo;
        _authorRepo = authorRepo;
    }
 
    // ── CLIENT ────────────────────────────────────────────────────────────────
 
    public async Task<PagedResult<AuthorSummary>> GetListAsync(AuthorQuery query)
    {
        var dbQuery = _baseRepo.BuildQueryable(
            ["Stories"],
            a => !a.DeletedAt.HasValue
                 && a.Stories.Any(s => !s.DeletedAt.HasValue
                    && (s.Status == "Publishing" || s.Status == "Completed"))
        );
 
        dbQuery = ApplyFilters(dbQuery, query);
 
        var total = await dbQuery.CountAsync();
 
        dbQuery = _baseRepo.ApplySorting(dbQuery, query.SortBy, query.SortDescending);
 
        var items = await dbQuery
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(AuthorSummary.FromAuthor)
            .ToListAsync();
 
        return new PagedResult<AuthorSummary>(items, total, query.PageNumber, query.PageSize);
    }
 
    public async Task<AuthorSummary> GetBySlugAsync(string slug)
    {
        var query = _baseRepo.BuildQueryable(
            ["Stories"],
            a => a.Slug == slug && !a.DeletedAt.HasValue
        );
 
        return await query
            .Select(AuthorSummary.FromAuthor)
            .FirstOrDefaultAsync()
            ?? throw new ResponseErrorObject("Author not found", StatusCodes.Status404NotFound);
    }
 
    // ── ADMIN ─────────────────────────────────────────────────────────────────
 
    public async Task<PagedResult<AuthorResult>> GetListAdminAsync(AuthorQuery query)
    {
        var dbQuery = _baseRepo.BuildQueryable(
            ["Stories"],
            a => !a.DeletedAt.HasValue
        );
 
        dbQuery = ApplyFilters(dbQuery, query);
 
        var total = await dbQuery.CountAsync();
 
        dbQuery = _baseRepo.ApplySorting(dbQuery, query.SortBy, query.SortDescending);
 
        var items = await dbQuery
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(AuthorResult.FromAuthor)
            .ToListAsync();
 
        return new PagedResult<AuthorResult>(items, total, query.PageNumber, query.PageSize);
    }
 
    public async Task<AuthorResult> GetByIdAsync(Guid id)
    {
        var query = _baseRepo.BuildQueryable(
            ["Stories"],
            a => a.Id == id && !a.DeletedAt.HasValue
        );
 
        return await query
            .Select(AuthorResult.FromAuthor)
            .FirstOrDefaultAsync()
            ?? throw new ResponseErrorObject("Author not found", StatusCodes.Status404NotFound);
    }
 
    public async Task<AuthorResult> CreateAsync(CreateAuthorInput input)
    {
        var slug = string.IsNullOrWhiteSpace(input.Slug)
            ? input.Name.GenerateSlug()
            : input.Slug.Trim().ToLower();
 
        if (await _authorRepo.SlugExistsAsync(slug))
            throw new ResponseErrorObject("Slug already exists", StatusCodes.Status409Conflict);
 
        var author = new Author
        {
            Name        = input.Name.Trim(),
            Slug        = slug,
            PenName     = input.PenName?.Trim(),
            Country     = input.Country?.Trim().ToUpper(),
            Description = input.Description?.Trim(),
            AvatarUrl   = input.AvatarUrl?.Trim(),
        };
 
        await _baseRepo.CreateAsync(author);
 
        return await GetByIdAsync(author.Id);
    }
 
    public async Task<AuthorResult> UpdateAsync(Guid id, UpdateAuthorInput input)
    {
        var author = await _baseRepo.GetByIdAsync(id)
            ?? throw new ResponseErrorObject("Author not found", StatusCodes.Status404NotFound);
 
        var slug = string.IsNullOrWhiteSpace(input.Slug)
            ? input.Name.GenerateSlug()
            : input.Slug.Trim().ToLower();
 
        if (await _authorRepo.SlugExistsAsync(slug, excludeId: id))
            throw new ResponseErrorObject("Slug already exists", StatusCodes.Status409Conflict);
 
        var oldName = author.Name;
 
        author.Name        = input.Name.Trim();
        author.Slug        = slug;
        author.PenName     = input.PenName?.Trim();
        author.Country     = input.Country?.Trim().ToUpper();
        author.Description = input.Description?.Trim();
        author.AvatarUrl   = input.AvatarUrl?.Trim();
 
        await _baseRepo.UpdateAsync(author);
 
        if (oldName != author.Name)
            await SyncAuthorNameOnStoriesAsync(id, author.Name);
 
        return await GetByIdAsync(id);
    }
 
    public async Task DeleteAsync(Guid id)
    {
        _ = await _baseRepo.GetByIdAsync(id)
            ?? throw new ResponseErrorObject("Author not found", StatusCodes.Status404NotFound);
 
        if (await _authorRepo.HasStoriesAsync(id))
            throw new ResponseErrorObject(
                "Cannot delete author that still has stories. Reassign or delete stories first.",
                StatusCodes.Status409Conflict);
 
        await _baseRepo.DeleteAsync(id);
    }
 
    // ── PRIVATE HELPERS ───────────────────────────────────────────────────────
 
    private static IQueryable<Author> ApplyFilters(IQueryable<Author> query, AuthorQuery filter)
    {
        if (!string.IsNullOrWhiteSpace(filter.Name))
        {
            var name = filter.Name.ToLower();
            query = query.Where(a =>
                a.Name.ToLower().Contains(name) ||
                (a.PenName != null && a.PenName.ToLower().Contains(name)));
        }
 
        if (!string.IsNullOrWhiteSpace(filter.Country))
            query = query.Where(a => a.Country == filter.Country.ToUpper());
 
        return query;
    }
 
    private async Task SyncAuthorNameOnStoriesAsync(Guid authorId, string newName)
    {
        await _baseRepo
            .BuildQueryable([], s => s.Id == authorId && !s.DeletedAt.HasValue)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.Name, newName));
    }
}