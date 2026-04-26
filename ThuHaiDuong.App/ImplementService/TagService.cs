using ThuHaiDuong.Shared.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using ThuHaiDuong.Application.InterfaceService;
using ThuHaiDuong.Application.Payloads.InputModels.Tag;
using ThuHaiDuong.Application.Payloads.Responses;
using ThuHaiDuong.Application.Payloads.ResultModels.Tag;
using ThuHaiDuong.Application.Payloads.ResultModels.User.Tag;
using ThuHaiDuong.Domain.Entities;
using ThuHaiDuong.Domain.InterfaceRepositories;
using ThuHaiDuong.Shared.Models;

namespace ThuHaiDuong.Application.ImplementService;

public class TagService : ITagService
{
    private readonly IBaseRepository<Tag> _baseRepo;
    private readonly ITagRepository _tagRepo;
 
    public TagService(
        IBaseRepository<Tag> baseRepo,
        ITagRepository tagRepo)
    {
        _baseRepo = baseRepo;
        _tagRepo = tagRepo;
    }
 
    // ── CLIENT ────────────────────────────────────────────────────────────────
 
    public async Task<List<TagSummary>> GetAllAsync(string? search = null)
    {
        var query = _baseRepo.BuildQueryable([], t => !t.IsDeleted);
 
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(t => t.Name.ToLower().Contains(s));
        }
 
        return await query
            .OrderBy(t => t.Name)
            .Select(TagSummary.FromTag)
            .ToListAsync();
    }
 
    // ── ADMIN ─────────────────────────────────────────────────────────────────
 
    public async Task<PagedResult<TagResult>> GetListAsync(TagQuery query)
    {
        var dbQuery = _baseRepo.BuildQueryable(
            ["StoryTags.Story"],
            t => !t.IsDeleted
        );
 
        if (!string.IsNullOrWhiteSpace(query.Name))
        {
            var name = query.Name.ToLower();
            dbQuery = dbQuery.Where(t => t.Name.ToLower().Contains(name));
        }
 
        var total = await dbQuery.CountAsync();
 
        dbQuery = _baseRepo.ApplySorting(dbQuery, query.SortBy, query.SortDescending);
 
        var items = await dbQuery
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(TagResult.FromTag)
            .ToListAsync();
 
        return new PagedResult<TagResult>(items, total, query.PageNumber, query.PageSize);
    }
 
    public async Task<TagResult> GetByIdAsync(Guid id)
    {
        var query = _baseRepo.BuildQueryable(
            ["StoryTags.Story"],
            t => t.Id == id && !t.IsDeleted
        );
 
        return await query
            .Select(TagResult.FromTag)
            .FirstOrDefaultAsync()
            ?? throw new ResponseErrorObject("Không tìm thấy tag", StatusCodes.Status404NotFound);
    }
 
    public async Task<TagResult> CreateAsync(CreateTagInput input)
    {
        var slug = string.IsNullOrWhiteSpace(input.Slug)
            ? input.Name.GenerateSlug()
            : input.Slug.Trim().ToLower();
 
        if (await _tagRepo.SlugExistsAsync(slug))
            throw new ResponseErrorObject("Slug đã tồn tại", StatusCodes.Status409Conflict);
 
        var tag = new Tag
        {
            Name = input.Name.Trim(),
            Slug = slug,
        };
 
        await _baseRepo.CreateAsync(tag);
 
        return await GetByIdAsync(tag.Id);
    }
 
    public async Task<TagResult> UpdateAsync(Guid id, UpdateTagInput input)
    {
        var tag = await _baseRepo.GetByIdAsync(id)
            ?? throw new ResponseErrorObject("Không tìm thấy tag", StatusCodes.Status404NotFound);
 
        var slug = string.IsNullOrWhiteSpace(input.Slug)
            ? input.Name.GenerateSlug()
            : input.Slug.Trim().ToLower();
 
        if (await _tagRepo.SlugExistsAsync(slug, excludeId: id))
            throw new ResponseErrorObject("Slug đã tồn tại", StatusCodes.Status409Conflict);
 
        tag.Name = input.Name.Trim();
        tag.Slug = slug;
 
        await _baseRepo.UpdateAsync(tag);
 
        return await GetByIdAsync(id);
    }
 
    public async Task DeleteAsync(Guid id)
    {
        var tag = await _baseRepo.GetByIdAsync(id)
            ?? throw new ResponseErrorObject("Không tìm thấy tag", StatusCodes.Status404NotFound);
 
        if (await _tagRepo.HasStoriesAsync(id))
            throw new ResponseErrorObject(
                "Không thể xóa tag vẫn gắn với truyện",
                StatusCodes.Status409Conflict);
 
        await _baseRepo.DeleteAsync(id);
    }
}