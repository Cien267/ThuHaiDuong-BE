using Microsoft.EntityFrameworkCore;

namespace ThuHaiDuong.Domain.Entities;

public class AffiliateLinkChapter
{
    public Guid AffiliateLinkId { get; set; }
    public Guid ChapterId { get; set; }
 
    // Navigation
    public virtual AffiliateLink AffiliateLink { get; set; } = null!;
    public virtual Chapter Chapter { get; set; } = null!;
}
 
public static class AffiliateLinkChapterModelBuilderExtensions
{
    public static void BuildAffiliateLinkChapterModel(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AffiliateLinkChapter>(entity =>
        {
            entity.ToTable("affiliate_link_chapters");
            entity.HasKey(e => new { e.AffiliateLinkId, e.ChapterId });
 
            entity.HasOne(e => e.AffiliateLink)
                .WithMany(a => a.AffiliateLinkChapters)
                .HasForeignKey(e => e.AffiliateLinkId)
                .OnDelete(DeleteBehavior.Cascade);
 
            entity.HasOne(e => e.Chapter)
                .WithMany()
                .HasForeignKey(e => e.ChapterId)
                .OnDelete(DeleteBehavior.Cascade);
 
            entity.HasIndex(e => e.ChapterId)
                .HasDatabaseName("IX_AffiliateLinkChapter_ChapterId");
        });
    }
}