using Microsoft.EntityFrameworkCore;

namespace ThuHaiDuong.Domain.Entities;

public class Bookmark : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid StoryId { get; set; }
 
    // Navigation
    public virtual User User { get; set; } = null!;
    public virtual Story Story { get; set; } = null!;
}
 
public static class BookmarkModelBuilderExtensions
{
    public static void BuildBookmarkModel(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Bookmark>(entity =>
        {
            entity.ToTable("bookmarks");
            entity.HasKey(e => e.Id);
 
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.StoryId).IsRequired();
 
            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasColumnType("datetime2");
 
            entity.Property(e => e.UpdatedAt)
                .IsRequired()
                .HasColumnType("datetime2");
 
            entity.Property(e => e.DeletedAt)
                .IsRequired()
                .HasColumnType("datetime2");
 
            entity.HasIndex(e => new { e.UserId, e.StoryId })
                .IsUnique()
                .HasDatabaseName("IX_Bookmark_UserId_StoryId");
 
            entity.HasIndex(e => new { e.UserId, e.CreatedAt })
                .HasDatabaseName("IX_Bookmark_UserId_CreatedAt");
 
            entity.HasOne(e => e.User)
                .WithMany(u => u.Bookmarks)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
 
            entity.HasOne(e => e.Story)
                .WithMany(s => s.Bookmarks)
                .HasForeignKey(e => e.StoryId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}