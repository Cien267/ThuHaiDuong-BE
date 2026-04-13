using Microsoft.EntityFrameworkCore;

namespace ThuHaiDuong.Domain.Entities;

public class Rating : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid StoryId { get; set; }
 
    // 1–5 stars
    public int Score { get; set; }
    public string? Comment { get; set; }
 
    // Navigation
    public virtual User User { get; set; } = null!;
    public virtual Story Story { get; set; } = null!;
}
 
public static class RatingModelBuilderExtensions
{
    public static void BuildRatingModel(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Rating>(entity =>
        {
            entity.ToTable("ratings");
            entity.HasKey(e => e.Id);
 
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.StoryId).IsRequired();
 
            entity.Property(e => e.Score)
                .IsRequired();
 
            entity.Property(e => e.Comment)
                .HasMaxLength(2000);
 
            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasColumnType("datetime2");
 
            entity.Property(e => e.UpdatedAt)
                .IsRequired()
                .HasColumnType("datetime2");
 
            entity.Property(e => e.DeletedAt)
                .IsRequired()
                .HasColumnType("datetime2");
 
            // Mỗi user chỉ được rate 1 lần per story
            entity.HasIndex(e => new { e.UserId, e.StoryId })
                .IsUnique()
                .HasDatabaseName("IX_Rating_UserId_StoryId");
 
            entity.HasIndex(e => e.StoryId)
                .HasDatabaseName("IX_Rating_StoryId");
 
            entity.HasCheckConstraint("CK_Rating_Score", "[Score] BETWEEN 1 AND 5");
 
            entity.HasOne(e => e.User)
                .WithMany(u => u.Ratings)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
 
            entity.HasOne(e => e.Story)
                .WithMany(s => s.Ratings)
                .HasForeignKey(e => e.StoryId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}