using Microsoft.EntityFrameworkCore;

namespace ThuHaiDuong.Domain.Entities
{
    public class User : BaseEntity
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string FullName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string? Avatar { get; set; }
        public string Email { get; set; }
        public string? PhoneNumber { get; set; }
        public bool HasOnboarded { get; set; }
        public Guid? BrokerageId  { get; set; }
        public virtual ICollection<Permissions> Permissions { get; set; }
        public virtual ICollection<RefreshToken> RefreshTokens { get; set; }
    }

    public static class UserModelBuilderExtensions
    {
        public static void BuildUserModel(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UserName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Password)
                    .IsRequired()
                    .HasMaxLength(256);

                entity.Property(e => e.FullName)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(256);

                entity.Property(e => e.PhoneNumber)
                    .HasMaxLength(20);

                entity.Property(e => e.Avatar)
                    .HasMaxLength(500);

                entity.Property(e => e.DateOfBirth)
                    .IsRequired()
                    .HasColumnType("date");

                entity.Property(e => e.HasOnboarded)
                    .IsRequired()
                    .HasDefaultValue(false);

                entity.Property(e => e.BrokerageId)
                    .IsRequired(false);

                entity.HasIndex(e => e.UserName)
                    .IsUnique()
                    .HasDatabaseName("IX_User_UserName");

                entity.HasIndex(e => e.Email)
                    .IsUnique()
                    .HasDatabaseName("IX_User_Email");

                entity.HasIndex(e => e.BrokerageId)
                    .HasDatabaseName("IX_User_BrokerageId");


                entity.HasMany(e => e.Permissions)
                    .WithOne()
                    .HasForeignKey("UserId")
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.RefreshTokens)
                    .WithOne()
                    .HasForeignKey("UserId")
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
