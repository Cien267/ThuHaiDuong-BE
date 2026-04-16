using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using ThuHaiDuong.Application.InterfaceService;
using ThuHaiDuong.Application.Payloads.InputModels.Category;
using ThuHaiDuong.Application.Payloads.Responses;
using ThuHaiDuong.Application.Payloads.ResultModels.Category;
using ThuHaiDuong.Application.Payloads.ResultModels.User.Category;
using ThuHaiDuong.Domain.Entities;
using ThuHaiDuong.Domain.InterfaceRepositories;
using ThuHaiDuong.Shared.Extensions;
using ThuHaiDuong.Shared.Models;

namespace ThuHaiDuong.Application.ImplementService;

public class CategoryService : ICategoryService
{
    private readonly IBaseRepository<Category> _baseRepo;
    private readonly ICategoryRepository _categoryRepo;
 
    public CategoryService(
        IBaseRepository<Category> baseRepo,
        ICategoryRepository categoryRepo)
    {
        _baseRepo = baseRepo;
        _categoryRepo = categoryRepo;
    }
 
    // ── CLIENT ────────────────────────────────────────────────────────────────
 
    public async Task<List<CategorySummary>> GetTreeAsync()
    {
        var query = _baseRepo.BuildQueryable(
            ["Children"],
            c => !c.DeletedAt.HasValue && c.IsActive
        );
 
        var all = await query
            .OrderBy(c => c.SortOrder)
            .Select(CategorySummary.FromCategory)
            .ToListAsync();
 
        var lookup = all.ToDictionary(c => c.Id);
 
        var roots = new List<CategorySummary>();
        foreach (var item in all)
        {
            if (item.ParentId == null)
            {
                roots.Add(item);
            }
            else if (lookup.TryGetValue(item.ParentId.Value, out var parent))
            {
                parent.Children.Add(item);
            }
        }
 
        return roots;
    }
 
    public async Task<CategorySummary> GetBySlugAsync(string slug)
    {
        var query = _baseRepo.BuildQueryable(
            ["Children"],
            c => c.Slug == slug && !c.DeletedAt.HasValue && c.IsActive
        );
 
        var category = await query
            .Select(CategorySummary.FromCategory)
            .FirstOrDefaultAsync()
            ?? throw new ResponseErrorObject("Category not found", StatusCodes.Status404NotFound);
 
        return category;
    }
 
    // ── ADMIN ─────────────────────────────────────────────────────────────────
 
    public async Task<PagedResult<CategoryResult>> GetListAsync(CategoryQuery query)
    {
        var dbQuery = _baseRepo.BuildQueryable(
            ["Parent", "StoryCategories.Story"],
            c => !c.DeletedAt.HasValue
        );
 
        if (!string.IsNullOrWhiteSpace(query.Name))
        {
            var name = query.Name.ToLower();
            dbQuery = dbQuery.Where(c => c.Name.ToLower().Contains(name));
        }
 
        if (query.IsActive.HasValue)
            dbQuery = dbQuery.Where(c => c.IsActive == query.IsActive.Value);
 
        if (query.ParentId.HasValue)
            dbQuery = dbQuery.Where(c => c.ParentId == query.ParentId.Value);
 
        var total = await dbQuery.CountAsync();
 
        dbQuery = _baseRepo.ApplySorting(dbQuery, query.SortBy, query.SortDescending);
 
        var items = await dbQuery
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(CategoryResult.FromCategory)
            .ToListAsync();
 
        return new PagedResult<CategoryResult>(items, total, query.PageNumber, query.PageSize);
    }
 
    public async Task<CategoryResult> GetByIdAsync(Guid id)
    {
        var query = _baseRepo.BuildQueryable(
            ["Parent", "StoryCategories.Story"],
            c => c.Id == id && !c.DeletedAt.HasValue
        );
 
        return await query
            .Select(CategoryResult.FromCategory)
            .FirstOrDefaultAsync()
            ?? throw new ResponseErrorObject("Không tìm thấy phân loại", StatusCodes.Status404NotFound);
    }
 
    public async Task<CategoryResult> CreateAsync(CreateCategoryInput input)
    {
        var slug = string.IsNullOrWhiteSpace(input.Slug)
            ? input.Name.GenerateSlug()
            : input.Slug.Trim().ToLower();
 
        if (await _categoryRepo.SlugExistsAsync(slug))
            throw new ResponseErrorObject("Slug đã tồn tại", StatusCodes.Status409Conflict);
 
        if (input.ParentId.HasValue)
        {
            var parentExists = await _baseRepo.GetByIdAsync(input.ParentId.Value);
            if (parentExists == null || parentExists.DeletedAt.HasValue)
                throw new ResponseErrorObject("Không tìm thấy phân loại cha", StatusCodes.Status404NotFound);
        }
 
        var category = new Category
        {
            Name        = input.Name.Trim(),
            Slug        = slug,
            Description = input.Description?.Trim(),
            ParentId    = input.ParentId,
            SortOrder   = input.SortOrder,
            IsActive    = input.IsActive,
        };
 
        await _baseRepo.CreateAsync(category);
 
        return await GetByIdAsync(category.Id);
    }
 
    public async Task<CategoryResult> UpdateAsync(Guid id, UpdateCategoryInput input)
    {
        var category = await _baseRepo.GetByIdAsync(id)
            ?? throw new ResponseErrorObject("Không tìm thấy phân loại", StatusCodes.Status404NotFound);
 
        var slug = string.IsNullOrWhiteSpace(input.Slug)
            ? input.Name.GenerateSlug()
            : input.Slug.Trim().ToLower();
 
        if (await _categoryRepo.SlugExistsAsync(slug, excludeId: id))
            throw new ResponseErrorObject("Slug đã tồn tại", StatusCodes.Status409Conflict);
 
        if (input.ParentId.HasValue && input.ParentId.Value == id)
            throw new ResponseErrorObject("Phân loại không thể làm cha chính nó", StatusCodes.Status400BadRequest);
 
        if (input.ParentId.HasValue)
        {
            var parentExists = await _baseRepo.GetByIdAsync(input.ParentId.Value);
            if (parentExists == null || parentExists.DeletedAt.HasValue)
                throw new ResponseErrorObject("Không tìm thấy phân loại cha", StatusCodes.Status404NotFound);
        }
 
        category.Name        = input.Name.Trim();
        category.Slug        = slug;
        category.Description = input.Description?.Trim();
        category.ParentId    = input.ParentId;
        category.SortOrder   = input.SortOrder;
        category.IsActive    = input.IsActive;
 
        await _baseRepo.UpdateAsync(category);
 
        return await GetByIdAsync(id);
    }
 
    public async Task DeleteAsync(Guid id)
    {
        var category = await _baseRepo.GetByIdAsync(id)
            ?? throw new ResponseErrorObject("Không tìm thấy phân loại", StatusCodes.Status404NotFound);
 
        if (await _categoryRepo.HasChildrenAsync(id))
            throw new ResponseErrorObject(
                "Không thể xóa phân loại có các phân loại con. Hãy xóa các phân loại con trước.",
                StatusCodes.Status409Conflict);
 
        if (await _categoryRepo.HasStoriesAsync(id))
            throw new ResponseErrorObject(
                "Không thể xóa phân loại vẫn có truyện được phân loại vào",
                StatusCodes.Status409Conflict);
 
        await _baseRepo.DeleteAsync(id);  // soft delete
    }
}