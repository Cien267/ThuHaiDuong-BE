using ThuHaiDuong.Application.Payloads.InputModels.Chapter;
using ThuHaiDuong.Application.Payloads.ResultModels.Chapter;
using ThuHaiDuong.Application.Payloads.ResultModels.User.Chapter;
using ThuHaiDuong.Shared.Models;

namespace ThuHaiDuong.Application.InterfaceService;

public interface IChapterService
{
    // ── CLIENT ────────────────────────────────────────────────────────────────
 
    Task<ChapterReadResult> GetForReadingAsync(Guid chapterId);
 
    Task<ChapterReadResult> GetForReadingByNumberAsync(Guid storyId, int chapterNumber);
 
    // ── CONTRIBUTOR / ADMIN ───────────────────────────────────────────────────
 
    Task<ChapterResult> CreateAsync(CreateChapterInput input, Guid requestUserId, string requestUserRole);
    Task<ChapterResult> UpdateAsync(Guid id, UpdateChapterInput input, Guid requestUserId, string requestUserRole);
 
    // ── ADMIN ONLY ────────────────────────────────────────────────────────────
 
    Task<PagedResult<ChapterListItem>> GetListAdminAsync(ChapterQuery query);
 
    Task<PagedResult<ChapterListItem>> GetListAsync(Guid storyId);
 
    Task UpdateStatusAsync(Guid id, UpdateChapterStatusInput input, Guid requestUserId, string requestUserRole);
 
    Task DeleteAsync(Guid id, Guid requestUserId, string requestUserRole);
}