using Microsoft.EntityFrameworkCore;

namespace ThuHaiDuong.Domain.Entities;

public class ChapterView : BaseEntity
{
    public Guid ChapterId { get; set; }
 
    // Denormalized để tránh JOIN khi query "truyện nào được đọc nhiều"
    public Guid StoryId { get; set; }
 
    // Null = anonymous
    public Guid? UserId { get; set; }
 
    // Session ID cho anonymous tracking (cookie/localStorage)
    public string? SessionId { get; set; }
    public string? IpAddress { get; set; }
    public DateTime ViewedAt { get; set; } = DateTime.UtcNow;
 
    // Navigation
    public virtual Chapter Chapter { get; set; } = null!;
    public virtual Story Story { get; set; } = null!;
    public virtual User? User { get; set; }
}
 
public static class ChapterViewModelBuilderExtensions
{
    public static void BuildChapterViewModel(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ChapterView>(entity =>
        {
            entity.ToTable("chapter_views");
            entity.HasKey(e => e.Id);
 
            entity.Property(e => e.ChapterId).IsRequired();
            entity.Property(e => e.StoryId).IsRequired();
            entity.Property(e => e.UserId).IsRequired(false);
 
            entity.Property(e => e.SessionId)
                .HasMaxLength(100);
 
            entity.Property(e => e.IpAddress)
                .HasMaxLength(45);
 
            entity.Property(e => e.ViewedAt)
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
 
            // Hot path: background job đếm view theo chapter trong khoảng thời gian
            entity.HasIndex(e => new { e.ChapterId, e.ViewedAt })
                .HasDatabaseName("IX_ChapterView_ChapterId_ViewedAt");
 
            // Hot path: đếm view theo story (dashboard "truyện hot")
            entity.HasIndex(e => new { e.StoryId, e.ViewedAt })
                .HasDatabaseName("IX_ChapterView_StoryId_ViewedAt");
 
            // Dedup: 1 session + chapter = 1 view trong 30 phút (check ở application layer)
            entity.HasIndex(e => new { e.SessionId, e.ChapterId })
                .HasDatabaseName("IX_ChapterView_SessionId_ChapterId");
 
            entity.HasOne(e => e.Chapter)
                .WithMany(c => c.ChapterViews)
                .HasForeignKey(e => e.ChapterId)
                .OnDelete(DeleteBehavior.Cascade);
 
            entity.HasOne(e => e.Story)
                .WithMany()
                .HasForeignKey(e => e.StoryId)
                .OnDelete(DeleteBehavior.NoAction);
 
            entity.HasOne(e => e.User)
                .WithMany(u => u.ChapterViews)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);
        });
    }
}