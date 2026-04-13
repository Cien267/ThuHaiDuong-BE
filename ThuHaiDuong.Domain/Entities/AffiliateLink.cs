using Microsoft.EntityFrameworkCore;

namespace ThuHaiDuong.Domain.Entities;

public class AffiliateLink : BaseEntity
{
    public string Name { get; set; } = null!;
    public string TargetUrl { get; set; } = null!;
 
    // Slug ngắn dùng trong URL redirect: /go/{TrackingCode}
    public string TrackingCode { get; set; } = null!;
 
    // "in-chapter" | "sidebar" | "popup" | "global"
    public string Placement { get; set; } = "in-chapter";
 
    // Ưu tiên hiển thị khi nhiều link cùng match
    public int Priority { get; set; } = 0;
 
    public bool IsActive { get; set; } = true;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
 
    // Navigation
    public virtual ICollection<AffiliateLinkStory> AffiliateLinkStories { get; set; } = [];
    public virtual ICollection<AffiliateClick> AffiliateClicks { get; set; } = [];
}
 
public static class AffiliateLinkModelBuilderExtensions
{
    public static void BuildAffiliateLinkModel(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AffiliateLink>(entity =>
        {
            entity.ToTable("affiliate_links");
            entity.HasKey(e => e.Id);
 
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(200);
 
            entity.Property(e => e.TargetUrl)
                .IsRequired()
                .HasMaxLength(2000);
 
            entity.Property(e => e.TrackingCode)
                .IsRequired()
                .HasMaxLength(50);
 
            entity.Property(e => e.Placement)
                .IsRequired()
                .HasMaxLength(30)
                .HasDefaultValue("in-chapter");
 
            entity.Property(e => e.Priority)
                .IsRequired()
                .HasDefaultValue(0);
 
            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValue(true);
 
            entity.Property(e => e.StartDate)
                .HasColumnType("datetime2")
                .IsRequired(false);
 
            entity.Property(e => e.EndDate)
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
 
            // Hot path: redirect /go/{code}
            entity.HasIndex(e => e.TrackingCode)
                .IsUnique()
                .HasDatabaseName("IX_AffiliateLink_TrackingCode");
 
            entity.HasIndex(e => new { e.IsActive, e.Placement, e.Priority })
                .HasDatabaseName("IX_AffiliateLink_IsActive_Placement_Priority");
        });
    }
}