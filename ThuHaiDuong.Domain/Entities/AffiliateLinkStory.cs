using Microsoft.EntityFrameworkCore;

namespace ThuHaiDuong.Domain.Entities;

public class AffiliateLinkStory
{
    public Guid AffiliateLinkId { get; set; }
    public Guid StoryId { get; set; }
 
    // Navigation
    public virtual AffiliateLink AffiliateLink { get; set; } = null!;
    public virtual Story Story { get; set; } = null!;
}
 
public static class AffiliateLinkStoryModelBuilderExtensions
{
    public static void BuildAffiliateLinkStoryModel(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AffiliateLinkStory>(entity =>
        {
            entity.ToTable("affiliate_link_stories");
            entity.HasKey(e => new { e.AffiliateLinkId, e.StoryId });
 
            entity.HasOne(e => e.AffiliateLink)
                .WithMany(a => a.AffiliateLinkStories)
                .HasForeignKey(e => e.AffiliateLinkId)
                .OnDelete(DeleteBehavior.Cascade);
 
            entity.HasOne(e => e.Story)
                .WithMany(s => s.AffiliateLinkStories)
                .HasForeignKey(e => e.StoryId)
                .OnDelete(DeleteBehavior.Cascade);
 
            entity.HasIndex(e => e.StoryId)
                .HasDatabaseName("IX_AffiliateLinkStory_StoryId");
        });
    }
}