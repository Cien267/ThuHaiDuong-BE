using ThuHaiDuong.Domain.Entities;

namespace ThuHaiDuong.Domain.InterfaceRepositories;

public interface ICommentRepository
{
    Task<Comment?> GetByIdAsync(Guid id);
}