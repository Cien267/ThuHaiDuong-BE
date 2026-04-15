using ThuHaiDuong.Application.Payloads.InputModels.Admin.Category;
using ThuHaiDuong.Application.Payloads.ResultModels.Admin.Category;
using ThuHaiDuong.Application.Payloads.ResultModels.User.Category;
using ThuHaiDuong.Shared.Models;

namespace ThuHaiDuong.Application.InterfaceService;

public interface ICategoryService
{
    Task<List<CategorySummary>> GetTreeAsync();
 
    Task<CategorySummary> GetBySlugAsync(string slug);
 
    Task<PagedResult<CategoryResult>> GetListAsync(CategoryQuery query);
    Task<CategoryResult> GetByIdAsync(Guid id);
    Task<CategoryResult> CreateAsync(CreateCategoryInput input);
    Task<CategoryResult> UpdateAsync(Guid id, UpdateCategoryInput input);
    Task DeleteAsync(Guid id);
}