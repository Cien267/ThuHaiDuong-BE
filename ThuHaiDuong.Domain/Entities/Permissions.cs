using Microsoft.EntityFrameworkCore;

namespace ThuHaiDuong.Domain.Entities
{
    public class Permissions : BaseEntity
    {
        public Guid UserId { get; set; }
        public virtual User User { get; set; } = null!;
        
        public Guid RoleId { get; set; }
        public virtual Role Role { get; set; } = null!;
    }

    public static class PermissionsModelBuilderExtensions
    {
        public static void BuildPermissionsModel(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Permissions>(entity =>
            {
                entity.ToTable("permissions");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.UserId)
                    .IsRequired();

                entity.Property(e => e.RoleId)
                    .IsRequired();

                entity.HasOne(e => e.User)
                    .WithMany(u => u.Permissions)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Role)
                    .WithMany(r => r.Permissions)
                    .HasForeignKey(e => e.RoleId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.UserId)
                    .HasDatabaseName("IX_Permissions_UserId");

                entity.HasIndex(e => e.RoleId)
                    .HasDatabaseName("IX_Permissions_RoleId");

                entity.HasIndex(e => new { e.UserId, e.RoleId })
                    .IsUnique()
                    .HasDatabaseName("IX_Permissions_UserId_RoleId_Unique");
            });
        }
    }
}