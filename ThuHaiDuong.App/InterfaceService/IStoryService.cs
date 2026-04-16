using ThuHaiDuong.Application.Payloads.InputModels.Story;
using ThuHaiDuong.Application.Payloads.ResultModels.Story;
using ThuHaiDuong.Application.Payloads.ResultModels.User.Story;
using ThuHaiDuong.Shared.Models;

namespace ThuHaiDuong.Application.InterfaceService;

public interface IStoryService
{
    // ── CLIENT (public) ───────────────────────────────────────────────────────
 
    Task<PagedResult<StorySummary>> GetListAsync(StoryQuery query);
 
    Task<StoryDetail> GetBySlugAsync(string slug);
 
    // ── CONTRIBUTOR / ADMIN ───────────────────────────────────────────────────
 
    Task<StoryResult> CreateAsync(CreateStoryInput input, Guid uploadedByUserId);
 
    Task<StoryResult> UpdateAsync(Guid id, UpdateStoryInput input, Guid requestUserId, string requestUserRole);
 
    Task SubmitForReviewAsync(Guid id, Guid requestUserId, string requestUserRole);
 
    // ── SUPER ADMIN ONLY ──────────────────────────────────────────────────────
 
    Task ReviewAsync(Guid id, ReviewStoryInput input);
 
    Task UpdateStatusAsync(Guid id, UpdateStoryStatusInput input);
 
    // ── ADMIN + SUPER ADMIN ───────────────────────────────────────────────────
 
    Task<PagedResult<StoryResult>> GetListAdminAsync(StoryQuery query);
 
    Task<StoryResult> GetByIdAdminAsync(Guid id);
 
    Task DeleteAsync(Guid id, Guid requestUserId, string requestUserRole);
}