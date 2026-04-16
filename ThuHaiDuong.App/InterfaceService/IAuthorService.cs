using ThuHaiDuong.Application.Payloads.InputModels.Author;
using ThuHaiDuong.Application.Payloads.ResultModels.Author;
using ThuHaiDuong.Application.Payloads.ResultModels.User.Author;
using ThuHaiDuong.Shared.Models;

namespace ThuHaiDuong.Application.InterfaceService;

public interface IAuthorService
{
    // ── CLIENT ────────────────────────────────────────────────────────────────
    Task<PagedResult<AuthorSummary>> GetListAsync(AuthorQuery query);
    Task<AuthorSummary> GetBySlugAsync(string slug);
 
    // ── ADMIN ─────────────────────────────────────────────────────────────────
    Task<PagedResult<AuthorResult>> GetListAdminAsync(AuthorQuery query);
    Task<AuthorResult> GetByIdAsync(Guid id);
    Task<AuthorResult> CreateAsync(CreateAuthorInput input);
    Task<AuthorResult> UpdateAsync(Guid id, UpdateAuthorInput input);
    Task DeleteAsync(Guid id);
}