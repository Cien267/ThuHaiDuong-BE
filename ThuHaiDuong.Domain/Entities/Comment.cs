using Microsoft.EntityFrameworkCore;

namespace ThuHaiDuong.Domain.Entities;

public class Comment : BaseEntity
{
    public Guid? UserId { get; set; }
    public Guid StoryId { get; set; }
 
    // Null = comment on the story page; has value = comment on a specific chapter page
    public Guid? ChapterId { get; set; }
 
    // Null = root comment; has value = reply
    public Guid? ParentCommentId { get; set; }
 
    public string Content { get; set; } = null!;
    public int LikeCount { get; set; } = 0;
    public bool IsHidden { get; set; } = false;
    public string? GuestName { get; set; }
    public string? GuestEmail { get; set; }
 
    // Navigation
    public virtual User User { get; set; } = null!;
    public virtual Story Story { get; set; } = null!;
    public virtual Chapter? Chapter { get; set; }
    public virtual Comment? ParentComment { get; set; }
    public virtual ICollection<Comment> Replies { get; set; } = [];
}
 
public static class CommentModelBuilderExtensions
{
    public static void BuildCommentModel(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Comment>(entity =>
        {
            entity.ToTable("comments");
            entity.HasKey(e => e.Id);
 
            entity.Property(e => e.UserId);
            entity.Property(e => e.StoryId).IsRequired();
 
            entity.Property(e => e.ChapterId)
                .IsRequired(false);
 
            entity.Property(e => e.ParentCommentId)
                .IsRequired(false);
 
            entity.Property(e => e.Content)
                .IsRequired()
                .HasMaxLength(5000);
 
            entity.Property(e => e.LikeCount)
                .IsRequired()
                .HasDefaultValue(0);
 
            entity.Property(e => e.IsHidden)
                .IsRequired()
                .HasDefaultValue(false);
            
            entity.Property(e => e.GuestName)
                .HasMaxLength(100);
 
            entity.Property(e => e.GuestEmail)
                .HasMaxLength(256);
 
            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasColumnType("datetime2");
 
            entity.Property(e => e.UpdatedAt)
                .IsRequired()
                .HasColumnType("datetime2");
 
            entity.Property(e => e.DeletedAt)
                .IsRequired()
                .HasColumnType("datetime2");
 
            entity.HasIndex(e => new { e.StoryId, e.ChapterId, e.CreatedAt })
                .HasDatabaseName("IX_Comment_StoryId_ChapterId_CreatedAt");
 
            entity.HasIndex(e => new { e.ParentCommentId, e.CreatedAt })
                .HasDatabaseName("IX_Comment_ParentCommentId_CreatedAt");
 
            entity.HasIndex(e => e.UserId)
                .HasDatabaseName("IX_Comment_UserId");
 
            entity.HasOne(e => e.User)
                .WithMany(u => u.Comments)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
 
            entity.HasOne(e => e.Story)
                .WithMany(s => s.Comments)
                .HasForeignKey(e => e.StoryId)
                .OnDelete(DeleteBehavior.NoAction);
 
            entity.HasOne(e => e.Chapter)
                .WithMany(c => c.Comments)
                .HasForeignKey(e => e.ChapterId)
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired(false);
 
            // Self-referencing: comment's replies
            entity.HasOne(e => e.ParentComment)
                .WithMany(c => c.Replies)
                .HasForeignKey(e => e.ParentCommentId)
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired(false);
        });
    }
}