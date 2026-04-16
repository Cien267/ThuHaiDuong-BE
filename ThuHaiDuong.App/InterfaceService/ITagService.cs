using ThuHaiDuong.Application.Payloads.InputModels.Tag;
using ThuHaiDuong.Application.Payloads.ResultModels.Tag;
using ThuHaiDuong.Application.Payloads.ResultModels.User.Tag;
using ThuHaiDuong.Shared.Models;

namespace ThuHaiDuong.Application.InterfaceService;

public interface ITagService
{
    Task<List<TagSummary>> GetAllAsync(string? search = null);
 
    Task<PagedResult<TagResult>> GetListAsync(TagQuery query);
    Task<TagResult> GetByIdAsync(Guid id);
    Task<TagResult> CreateAsync(CreateTagInput input);
    Task<TagResult> UpdateAsync(Guid id, UpdateTagInput input);
    Task DeleteAsync(Guid id);
}