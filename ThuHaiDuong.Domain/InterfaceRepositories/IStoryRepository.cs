using ThuHaiDuong.Domain.Entities;

namespace ThuHaiDuong.Domain.InterfaceRepositories;

public interface IStoryRepository
{
    Task<bool> SlugExistsAsync(string slug, Guid? excludeId = null);
 
    Task<Story?> GetDetailBySlugAsync(string slug);
 
    Task SyncCategoriesAsync(Guid storyId, List<Guid> categoryIds);
    Task SyncTagsAsync(Guid storyId, List<Guid> tagIds);
 
    Task UpdateTotalChaptersAsync(Guid storyId);
}