namespace ThuHaiDuong.Domain.InterfaceRepositories;

public interface ICategoryRepository
{
    Task<bool> SlugExistsAsync(string slug, Guid? excludeId = null);
 
    Task<bool> HasStoriesAsync(Guid categoryId);
 
    Task<bool> HasChildrenAsync(Guid categoryId);
}