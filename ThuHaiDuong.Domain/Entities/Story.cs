using Microsoft.EntityFrameworkCore;

namespace ThuHaiDuong.Domain.Entities;

public class Story : BaseEntity
{
    public string Title { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public Guid AuthorId { get; set; }
 
    // Cache display name to avoid JOIN when only the name is needed for display
    // Synced from Author.Name on create/update
    public string AuthorName { get; set; } = null!;
 
    // Uploader (admin/contributor), nullable because it may be auto-crawled
    public Guid? UploadedByUserId { get; set; }
 
    // Original URL if crawled from an external source
    public string? SourceUrl { get; set; }
 
    public string? Description { get; set; }
    public string? CoverImageUrl { get; set; }
 
    // "Draft" | "Publishing" | "Completed" | "Paused"
    public string Status { get; set; } = "Draft";
 
    // Content characteristic — completely independent from Status
    // "Serial"    = chapters are being released on schedule (translated/ongoing works)
    // "Completed" = fully finished with all chapters (converted from original source)
    public string StoryType { get; set; } = "Completed";
 
    // Chapter release schedule — only meaningful when StoryType = "Serial"
    // "Daily" | "Weekly" | "BiWeekly" | "Monthly" | null
    public string? ReleaseSchedule { get; set; }
 
    // Estimated time for the next chapter release — used to display a countdown for readers
    public DateTime? NextChapterAt { get; set; }
 
    // "Manual" | "Crawled" | "UGC"
    public string ContentSource { get; set; } = "Manual";
 
    // Denormalized counters — updated by background job
    public int TotalChapters { get; set; } = 0;
    public long TotalViews { get; set; } = 0;
    public decimal AverageRating { get; set; } = 0;
    public int RatingCount { get; set; } = 0;
 
    public DateTime? LastChapterAt { get; set; }
 
    // Navigation
    public virtual Author Author { get; set; } = null!;
    public virtual User? UploadedByUser { get; set; }
    public virtual ICollection<Chapter> Chapters { get; set; } = [];
    public virtual ICollection<StoryCategory> StoryCategories { get; set; } = [];
    public virtual ICollection<StoryTag> StoryTags { get; set; } = [];
    public virtual ICollection<UserReadingProgress> ReadingProgresses { get; set; } = [];
    public virtual ICollection<Bookmark> Bookmarks { get; set; } = [];
    public virtual ICollection<Rating> Ratings { get; set; } = [];
    public virtual ICollection<Comment> Comments { get; set; } = [];
    public virtual ICollection<AffiliateLinkStory> AffiliateLinkStories { get; set; } = [];
    public virtual ICollection<DailyStoryStat> DailyStoryStats { get; set; } = [];
}
 
public static class StoryModelBuilderExtensions
{
    public static void BuildStoryModel(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Story>(entity =>
        {
            entity.ToTable("stories");
            entity.HasKey(e => e.Id);
 
            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(300);
 
            entity.Property(e => e.Slug)
                .IsRequired()
                .HasMaxLength(350);
 
            entity.Property(e => e.AuthorId)
                .IsRequired();
 
            entity.Property(e => e.AuthorName)
                .IsRequired()
                .HasMaxLength(200);
 
            entity.Property(e => e.UploadedByUserId)
                .IsRequired(false);
 
            entity.Property(e => e.SourceUrl)
                .HasMaxLength(1000);
 
            entity.Property(e => e.Description)
                .HasColumnType("nvarchar(max)");
 
            entity.Property(e => e.CoverImageUrl)
                .HasMaxLength(500);
 
            entity.Property(e => e.Status)
                .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValue("Draft");
 
            entity.Property(e => e.StoryType)
                .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValue("Completed");
 
            entity.Property(e => e.ReleaseSchedule)
                .HasMaxLength(20)
                .IsRequired(false);
 
            entity.Property(e => e.NextChapterAt)
                .HasColumnType("datetime2")
                .IsRequired(false);
 
            entity.Property(e => e.ContentSource)
                .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValue("Manual");
 
            entity.Property(e => e.TotalChapters)
                .IsRequired()
                .HasDefaultValue(0);
 
            entity.Property(e => e.TotalViews)
                .IsRequired()
                .HasDefaultValue(0L);
 
            entity.Property(e => e.AverageRating)
                .IsRequired()
                .HasColumnType("decimal(3,2)")
                .HasDefaultValue(0m);
 
            entity.Property(e => e.RatingCount)
                .IsRequired()
                .HasDefaultValue(0);
 
            entity.Property(e => e.LastChapterAt)
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
 
            entity.HasCheckConstraint("CK_Story_StoryType",
                "[StoryType] IN ('Serial', 'Completed')");
 
            entity.HasCheckConstraint("CK_Story_ReleaseSchedule",
                "[ReleaseSchedule] IS NULL OR [ReleaseSchedule] IN ('Daily', 'Weekly', 'BiWeekly', 'Monthly')");
 
            entity.HasIndex(e => e.Slug)
                .IsUnique()
                .HasDatabaseName("IX_Story_Slug");
 
            entity.HasIndex(e => new { e.Status, e.LastChapterAt })
                .HasDatabaseName("IX_Story_Status_LastChapterAt");
 
            entity.HasIndex(e => new { e.Status, e.TotalViews })
                .HasDatabaseName("IX_Story_Status_TotalViews");
 
            entity.HasIndex(e => new { e.StoryType, e.Status, e.LastChapterAt })
                .HasDatabaseName("IX_Story_StoryType_Status_LastChapterAt");
 
            entity.HasIndex(e => new { e.StoryType, e.NextChapterAt })
                .HasDatabaseName("IX_Story_StoryType_NextChapterAt");
 
            entity.HasIndex(e => new { e.AuthorId, e.Status })
                .HasDatabaseName("IX_Story_AuthorId_Status");
 
            entity.HasIndex(e => e.UploadedByUserId)
                .HasDatabaseName("IX_Story_UploadedByUserId");
 
            entity.HasIndex(e => e.ContentSource)
                .HasDatabaseName("IX_Story_ContentSource");
 
            // Relationships
            entity.HasOne(e => e.Author)
                .WithMany(a => a.Stories)
                .HasForeignKey(e => e.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);
 
            entity.HasOne(e => e.UploadedByUser)
                .WithMany(u => u.UploadedStories)
                .HasForeignKey(e => e.UploadedByUserId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}