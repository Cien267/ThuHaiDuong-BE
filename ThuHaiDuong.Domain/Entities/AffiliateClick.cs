using Microsoft.EntityFrameworkCore;

namespace ThuHaiDuong.Domain.Entities;

public class AffiliateClick : BaseEntity
{
    public Guid AffiliateLinkId { get; set; }
 
    // Null = anonymous user
    public Guid? UserId { get; set; }
 
    // Chương user đang đọc khi click
    public Guid? ChapterId { get; set; }
 
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? Referrer { get; set; }
    public DateTime ClickedAt { get; set; } = DateTime.UtcNow;
 
    // Navigation
    public virtual AffiliateLink AffiliateLink { get; set; } = null!;
    public virtual User? User { get; set; }
    public virtual Chapter? Chapter { get; set; }
}
 
public static class AffiliateClickModelBuilderExtensions
{
    public static void BuildAffiliateClickModel(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AffiliateClick>(entity =>
        {
            entity.ToTable("affiliate_clicks");
            entity.HasKey(e => e.Id);
 
            entity.Property(e => e.AffiliateLinkId).IsRequired();
 
            entity.Property(e => e.UserId).IsRequired(false);
            entity.Property(e => e.ChapterId).IsRequired(false);
 
            entity.Property(e => e.IpAddress).HasMaxLength(45);
            entity.Property(e => e.UserAgent).HasMaxLength(500);
            entity.Property(e => e.Referrer).HasMaxLength(1000);
 
            entity.Property(e => e.ClickedAt)
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
 
            entity.HasIndex(e => new { e.AffiliateLinkId, e.ClickedAt })
                .HasDatabaseName("IX_AffiliateClick_LinkId_ClickedAt");
 
            entity.HasIndex(e => e.ClickedAt)
                .HasDatabaseName("IX_AffiliateClick_ClickedAt");
 
            entity.HasIndex(e => e.UserId)
                .HasDatabaseName("IX_AffiliateClick_UserId");
 
            entity.HasOne(e => e.AffiliateLink)
                .WithMany(a => a.AffiliateClicks)
                .HasForeignKey(e => e.AffiliateLinkId)
                .OnDelete(DeleteBehavior.Cascade);
 
            entity.HasOne(e => e.User)
                .WithMany(u => u.AffiliateClicks)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);
 
            entity.HasOne(e => e.Chapter)
                .WithMany(c => c.AffiliateClicks)
                .HasForeignKey(e => e.ChapterId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);
        });
    }
}