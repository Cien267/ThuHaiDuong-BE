using Microsoft.EntityFrameworkCore;

namespace ThuHaiDuong.Infrastructure.DataContext
{
    public interface IDbContext : IDisposable
    {
        DbSet<TEntity> SetEntity<TEntity>() where TEntity : class;
        Task<int> CommitChangeAsync();
    }
}
