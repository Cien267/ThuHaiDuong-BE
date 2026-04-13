using Microsoft.EntityFrameworkCore;

namespace ThuHaiDuong.Domain.Entities;

public class Story : BaseEntity
{
    public string Title { get; set; } = null!;
    public string Slug { get; set; } = null!;
 
    public string AuthorName { get; set; } = null!;
 
    public Guid? UploadedByUserId { get; set; }
 
    // original URL if crawled from outsource
    public string? SourceUrl { get; set; }
 
    public string? Description { get; set; }
    public string? CoverImageUrl { get; set; }
 
    // "Draft" | "Publishing" | "Completed" | "Paused" | "Deleted"
    public string Status { get; set; } = "Draft";
 
    // "Manual" | "Crawled" | "UGC"
    public string ContentSource { get; set; } = "Manual";
 
    public int TotalChapters { get; set; } = 0;
    public long TotalViews { get; set; } = 0;
    public decimal AverageRating { get; set; } = 0;
    public int RatingCount { get; set; } = 0;
 
    public DateTime? LastChapterAt { get; set; }
 
    // Navigation
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
 
            entity.HasIndex(e => e.Slug)
                .IsUnique()
                .HasDatabaseName("IX_Story_Slug");
 
            entity.HasIndex(e => new { e.Status, e.LastChapterAt })
                .HasDatabaseName("IX_Story_Status_LastChapterAt");
 
            entity.HasIndex(e => new { e.Status, e.TotalViews })
                .HasDatabaseName("IX_Story_Status_TotalViews");
 
            entity.HasIndex(e => e.UploadedByUserId)
                .HasDatabaseName("IX_Story_UploadedByUserId");
 
            entity.HasIndex(e => e.ContentSource)
                .HasDatabaseName("IX_Story_ContentSource");
        });
    }
}