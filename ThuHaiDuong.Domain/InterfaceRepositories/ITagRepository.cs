using ThuHaiDuong.Domain.Entities;

namespace ThuHaiDuong.Domain.InterfaceRepositories;

public interface ITagRepository
{
    Task<bool> SlugExistsAsync(string slug, Guid? excludeId = null);
 
    Task<List<Tag>> GetByIdsAsync(IEnumerable<Guid> ids);
 
    Task<bool> HasStoriesAsync(Guid tagId);
}