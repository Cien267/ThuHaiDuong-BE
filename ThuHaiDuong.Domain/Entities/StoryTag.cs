using Microsoft.EntityFrameworkCore;

namespace ThuHaiDuong.Domain.Entities;

public class StoryTag
{
    public Guid StoryId { get; set; }
    public Guid TagId { get; set; }
 
    // Navigation
    public virtual Story Story { get; set; } = null!;
    public virtual Tag Tag { get; set; } = null!;
}
 
public static class StoryTagModelBuilderExtensions
{
    public static void BuildStoryTagModel(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<StoryTag>(entity =>
        {
            entity.ToTable("story_tags");
 
            entity.HasKey(e => new { e.StoryId, e.TagId });
 
            entity.HasOne(e => e.Story)
                .WithMany(s => s.StoryTags)
                .HasForeignKey(e => e.StoryId)
                .OnDelete(DeleteBehavior.Cascade);
 
            entity.HasOne(e => e.Tag)
                .WithMany(t => t.StoryTags)
                .HasForeignKey(e => e.TagId)
                .OnDelete(DeleteBehavior.Cascade);
 
            entity.HasIndex(e => e.TagId)
                .HasDatabaseName("IX_StoryTag_TagId");
        });
    }
}