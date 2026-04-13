using Microsoft.EntityFrameworkCore;

namespace ThuHaiDuong.Domain.Entities;

public class UserReadingProgress : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid StoryId { get; set; }
    public Guid LastChapterId { get; set; }
    public int LastChapterNumber { get; set; }
    public DateTime LastReadAt { get; set; } = DateTime.UtcNow;
 
    // Navigation
    public virtual User User { get; set; } = null!;
    public virtual Story Story { get; set; } = null!;
    public virtual Chapter LastChapter { get; set; } = null!;
}
 
public static class UserReadingProgressModelBuilderExtensions
{
    public static void BuildUserReadingProgressModel(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserReadingProgress>(entity =>
        {
            entity.ToTable("user_reading_progresses");
            entity.HasKey(e => e.Id);
 
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.StoryId).IsRequired();
            entity.Property(e => e.LastChapterId).IsRequired();
 
            entity.Property(e => e.LastChapterNumber)
                .IsRequired();
 
            entity.Property(e => e.LastReadAt)
                .IsRequired()
                .HasColumnType("datetime2");
 
            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasColumnType("datetime2");
 
            entity.Property(e => e.UpdatedAt)
                .IsRequired()
                .HasColumnType("datetime2");
 
            entity.Property(e => e.DeletedAt)
                .IsRequired()
                .HasColumnType("datetime2");
 
            entity.HasIndex(e => new { e.UserId, e.StoryId })
                .IsUnique()
                .HasDatabaseName("IX_UserReadingProgress_UserId_StoryId");
 
            entity.HasIndex(e => new { e.UserId, e.LastReadAt })
                .HasDatabaseName("IX_UserReadingProgress_UserId_LastReadAt");
 
            entity.HasOne(e => e.User)
                .WithMany(u => u.ReadingProgresses)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
 
            entity.HasOne(e => e.Story)
                .WithMany(s => s.ReadingProgresses)
                .HasForeignKey(e => e.StoryId)
                .OnDelete(DeleteBehavior.Cascade);
 
            entity.HasOne(e => e.LastChapter)
                .WithMany()
                .HasForeignKey(e => e.LastChapterId)
                .OnDelete(DeleteBehavior.NoAction);
        });
    }
}