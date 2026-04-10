using Microsoft.EntityFrameworkCore;
using ThuHaiDuong.Domain.Entities;
using System.Threading.Tasks;

namespace ThuHaiDuong.Infrastructure.DataContext
{
    public class AppDbContext : DbContext, IAppDbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Permissions> Permissions { get; set; }
        public virtual DbSet<Role> Roles { get; set; }
        public virtual DbSet<RefreshToken> RefreshTokens { get; set; }
        public async Task<int> CommitChangeAsync()
        {
            return await SaveChangesAsync();
        }

        public DbSet<TEntity> SetEntity<TEntity>() where TEntity : class
        {
            return Set<TEntity>();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.BuildUserModel();
            modelBuilder.BuildRoleModel();
            modelBuilder.BuildPermissionsModel();
            modelBuilder.BuildRefreshTokenModel();
        }
    }
}
