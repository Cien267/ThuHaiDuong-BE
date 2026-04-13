using Microsoft.EntityFrameworkCore;

namespace ThuHaiDuong.Domain.Entities;

public class Chapter : BaseEntity
{
    public Guid StoryId { get; set; }
    public int ChapterNumber { get; set; }
    public string Title { get; set; } = null!;
    public string Content { get; set; } = null!;
 
    // "Draft" | "Published" | "Scheduled" | "Hidden"
    public string Status { get; set; } = "Draft";
 
    // Require subscription to read
    public bool IsVip { get; set; } = false;
 
    public int WordCount { get; set; } = 0;
 
    // Denormalized — updated by background job
    public long ViewCount { get; set; } = 0;
 
    public DateTime? PublishedAt { get; set; }
 
    // Navigation
    public virtual Story Story { get; set; } = null!;
    public virtual ICollection<ChapterView> ChapterViews { get; set; } = [];
    public virtual ICollection<Comment> Comments { get; set; } = [];
    public virtual ICollection<AffiliateClick> AffiliateClicks { get; set; } = [];
}
 
public static class ChapterModelBuilderExtensions
{
    public static void BuildChapterModel(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Chapter>(entity =>
        {
            entity.ToTable("chapters");
            entity.HasKey(e => e.Id);
 
            entity.Property(e => e.StoryId)
                .IsRequired();
 
            entity.Property(e => e.ChapterNumber)
                .IsRequired();
 
            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(300);
 
            entity.Property(e => e.Content)
                .IsRequired()
                .HasColumnType("nvarchar(max)");
 
            entity.Property(e => e.Status)
                .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValue("Draft");
 
            entity.Property(e => e.IsVip)
                .IsRequired()
                .HasDefaultValue(false);
 
            entity.Property(e => e.WordCount)
                .IsRequired()
                .HasDefaultValue(0);
 
            entity.Property(e => e.ViewCount)
                .IsRequired()
                .HasDefaultValue(0L);
 
            entity.Property(e => e.PublishedAt)
                .HasColumnType("datetime2")
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
 
            // Hot path: lấy chương theo StoryId + số thứ tự (prev/next)
            entity.HasIndex(e => new { e.StoryId, e.ChapterNumber })
                .IsUnique()
                .HasDatabaseName("IX_Chapter_StoryId_ChapterNumber");
 
            entity.HasIndex(e => new { e.StoryId, e.Status, e.ChapterNumber })
                .HasDatabaseName("IX_Chapter_StoryId_Status_ChapterNumber");
 
            entity.HasOne(e => e.Story)
                .WithMany(s => s.Chapters)
                .HasForeignKey(e => e.StoryId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}