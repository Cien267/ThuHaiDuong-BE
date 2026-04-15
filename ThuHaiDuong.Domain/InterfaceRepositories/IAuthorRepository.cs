namespace ThuHaiDuong.Domain.InterfaceRepositories;

public interface IAuthorRepository
{
    Task<bool> SlugExistsAsync(string slug, Guid? excludeId = null);
    Task<bool> HasStoriesAsync(Guid authorId);
}