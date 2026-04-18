using Microsoft.EntityFrameworkCore;

namespace ThuHaiDuong.Domain.Entities;

public class ReadingHistory : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid ChapterId { get; set; }
    public Guid StoryId { get; set; }       // denormalized — tránh JOIN khi query lịch sử
 
    public DateTime ReadAt { get; set; } = DateTime.UtcNow;
 
    // Navigation
    public virtual User User { get; set; } = null!;
    public virtual Chapter Chapter { get; set; } = null!;
    public virtual Story Story { get; set; } = null!;
}

public static class ReadingHistoryModelBuilderExtensions
{
    public static void BuildReadingHistoryModel(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ReadingHistory>(entity =>
        {
            entity.ToTable("reading_histories");
            entity.HasKey(e => e.Id);
 
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.ChapterId).IsRequired();
            entity.Property(e => e.StoryId).IsRequired();
 
            entity.Property(e => e.ReadAt)
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
 
            // Mỗi user + chapter chỉ có 1 record → UPSERT pattern
            entity.HasIndex(e => new { e.UserId, e.ChapterId })
                .IsUnique()
                .HasDatabaseName("IX_ReadingHistory_UserId_ChapterId");
 
            // Query lịch sử đọc của user theo story
            entity.HasIndex(e => new { e.UserId, e.StoryId, e.ReadAt })
                .HasDatabaseName("IX_ReadingHistory_UserId_StoryId_ReadAt");
 
            // Query lịch sử gần nhất của user (trang "Đọc gần đây")
            entity.HasIndex(e => new { e.UserId, e.ReadAt })
                .HasDatabaseName("IX_ReadingHistory_UserId_ReadAt");
 
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
 
            entity.HasOne(e => e.Chapter)
                .WithMany()
                .HasForeignKey(e => e.ChapterId)
                .OnDelete(DeleteBehavior.NoAction);
 
            entity.HasOne(e => e.Story)
                .WithMany()
                .HasForeignKey(e => e.StoryId)
                .OnDelete(DeleteBehavior.NoAction);
        });
    }
}