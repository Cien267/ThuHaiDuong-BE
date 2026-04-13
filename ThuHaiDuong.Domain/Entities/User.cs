using Microsoft.EntityFrameworkCore;

namespace ThuHaiDuong.Domain.Entities
{
    public class User : BaseEntity
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string FullName { get; set; }
        public string? Avatar { get; set; }
        public string Email { get; set; }
        public string? PhoneNumber { get; set; }
        // "Reader" | "Contributor" | "Admin" | "SuperAdmin"
        public string Role { get; set; } = "Reader";
        public bool IsActive { get; set; } = true;
        public DateTime? LastLoginAt { get; set; }
 
        // Navigation
        public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = [];
        public virtual ICollection<Story> UploadedStories { get; set; } = [];
        public virtual ICollection<UserReadingProgress> ReadingProgresses { get; set; } = [];
        public virtual ICollection<Bookmark> Bookmarks { get; set; } = [];
        public virtual ICollection<Rating> Ratings { get; set; } = [];
        public virtual ICollection<Comment> Comments { get; set; } = [];
        public virtual ICollection<Subscription> Subscriptions { get; set; } = [];
        public virtual ICollection<AffiliateClick> AffiliateClicks { get; set; } = [];
        public virtual ICollection<ChapterView> ChapterViews { get; set; } = [];
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

                entity.Property(e => e.Role)
                .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValue("Reader");
 
            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValue(true);
 
            entity.Property(e => e.LastLoginAt)
                .IsRequired(false);
 
            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasColumnType("datetime2");
 
            entity.Property(e => e.UpdatedAt)
                .IsRequired()
                .HasColumnType("datetime2");
 
            entity.Property(e => e.DeletedAt)
                .IsRequired()
                .HasColumnType("datetime2");
 
            entity.HasIndex(e => e.UserName)
                .IsUnique()
                .HasDatabaseName("IX_User_UserName");
 
            entity.HasIndex(e => e.Email)
                .IsUnique()
                .HasDatabaseName("IX_User_Email");
 
            entity.HasIndex(e => e.Role)
                .HasDatabaseName("IX_User_Role");
 
            entity.HasMany(e => e.RefreshTokens)
                .WithOne(r => r.User)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);
 
            entity.HasMany(e => e.UploadedStories)
                .WithOne(s => s.UploadedByUser)
                .HasForeignKey(s => s.UploadedByUserId)
                .OnDelete(DeleteBehavior.SetNull);
 
            entity.HasMany(e => e.ReadingProgresses)
                .WithOne(p => p.User)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);
 
            entity.HasMany(e => e.Bookmarks)
                .WithOne(b => b.User)
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Cascade);
 
            entity.HasMany(e => e.Ratings)
                .WithOne(r => r.User)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);
 
            entity.HasMany(e => e.Comments)
                .WithOne(c => c.User)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);
 
            entity.HasMany(e => e.Subscriptions)
                .WithOne(s => s.User)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);
 
            entity.HasMany(e => e.AffiliateClicks)
                .WithOne(a => a.User)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.SetNull);
 
            entity.HasMany(e => e.ChapterViews)
                .WithOne(v => v.User)
                .HasForeignKey(v => v.UserId)
                .OnDelete(DeleteBehavior.SetNull);
            });
        }
    }
}
