using Microsoft.EntityFrameworkCore;

namespace ThuHaiDuong.Domain.Entities
{
    public class Role : BaseEntity
    {
        public string RoleCode { get; set; } = null!;
        public string RoleName { get; set; } = null!;

        public virtual ICollection<Permissions> Permissions { get; set; } = new List<Permissions>();
    }

    public static class RoleModelBuilderExtensions
    {
        public static void BuildRoleModel(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Role>(entity =>
            {
                entity.ToTable("roles");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.RoleCode)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.RoleName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.HasMany(e => e.Permissions)
                    .WithOne(p => p.Role)
                    .HasForeignKey(p => p.RoleId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.RoleCode)
                    .IsUnique()
                    .HasDatabaseName("IX_Role_RoleCode");

                entity.HasIndex(e => e.RoleName)
                    .HasDatabaseName("IX_Role_RoleName");
            });
        }
    }
}