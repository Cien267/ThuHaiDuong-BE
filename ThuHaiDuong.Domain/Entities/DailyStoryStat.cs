using Microsoft.EntityFrameworkCore;

namespace ThuHaiDuong.Domain.Entities;

public class DailyStoryStat
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid StoryId { get; set; }
    public DateOnly StatDate { get; set; }
    public int ViewCount { get; set; } = 0;
    public int UniqueVisitors { get; set; } = 0;
    public int NewBookmarks { get; set; } = 0;
    public int NewRatings { get; set; } = 0;
 
    // Navigation
    public virtual Story Story { get; set; } = null!;
}
 
public static class DailyStoryStatModelBuilderExtensions
{
    public static void BuildDailyStoryStatModel(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DailyStoryStat>(entity =>
        {
            entity.ToTable("daily_story_stats");
            entity.HasKey(e => e.Id);
 
            entity.Property(e => e.StoryId).IsRequired();
 
            entity.Property(e => e.StatDate)
                .IsRequired()
                .HasColumnType("date");
 
            entity.Property(e => e.ViewCount)
                .IsRequired()
                .HasDefaultValue(0);
 
            entity.Property(e => e.UniqueVisitors)
                .IsRequired()
                .HasDefaultValue(0);
 
            entity.Property(e => e.NewBookmarks)
                .IsRequired()
                .HasDefaultValue(0);
 
            entity.Property(e => e.NewRatings)
                .IsRequired()
                .HasDefaultValue(0);
 
            entity.HasIndex(e => new { e.StoryId, e.StatDate })
                .IsUnique()
                .HasDatabaseName("IX_DailyStoryStat_StoryId_StatDate");
 
            entity.HasIndex(e => new { e.StatDate, e.ViewCount })
                .HasDatabaseName("IX_DailyStoryStat_StatDate_ViewCount");
 
            entity.HasOne(e => e.Story)
                .WithMany(s => s.DailyStoryStats)
                .HasForeignKey(e => e.StoryId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}