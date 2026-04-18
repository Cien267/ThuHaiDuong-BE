using Microsoft.EntityFrameworkCore;

namespace ThuHaiDuong.Domain.Entities
{
    public class RefreshToken : BaseEntity
    {
        public Guid UserId { get; set; }
        public virtual User User { get; set; } = null!;

        public string Token { get; set; } = null!;

        public DateTime ExpiresAt { get; set; }

        public bool IsRevoked { get; set; }
        public DateTime? RevokedAt { get; set; }

        public DateTime CreatedAt { get; set; }
    }

    public static class RefreshTokenModelBuilderExtensions
    {
        public static void BuildRefreshTokenModel(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.ToTable("refresh_tokens");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.UserId)
                    .IsRequired();

                entity.Property(e => e.Token)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(e => e.ExpiresAt)
                    .IsRequired();

                entity.Property(e => e.IsRevoked)
                    .IsRequired()
                    .HasDefaultValue(false);

                entity.Property(e => e.RevokedAt)
                    .IsRequired(false);

                entity.Property(e => e.CreatedAt)
                    .IsRequired()
                    .HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(e => e.User)
                    .WithMany(u => u.RefreshTokens)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.Token)
                    .IsUnique()
                    .HasDatabaseName("IX_RefreshToken_Token");

                entity.HasIndex(e => e.UserId)
                    .HasDatabaseName("IX_RefreshToken_UserId");

                entity.HasIndex(e => new { e.UserId, e.IsRevoked, e.ExpiresAt })
                    .HasDatabaseName("IX_RefreshToken_UserId_IsRevoked_ExpiresAt");

                entity.HasIndex(e => e.ExpiresAt)
                    .HasDatabaseName("IX_RefreshToken_ExpiresAt");
            });
        }
    }
}