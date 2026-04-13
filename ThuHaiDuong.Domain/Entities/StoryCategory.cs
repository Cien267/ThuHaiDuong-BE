using Microsoft.EntityFrameworkCore;

namespace ThuHaiDuong.Domain.Entities;

public class StoryCategory
{
    public Guid StoryId { get; set; }
    public Guid CategoryId { get; set; }
 
    // Navigation
    public virtual Story Story { get; set; } = null!;
    public virtual Category Category { get; set; } = null!;
}
 
public static class StoryCategoryModelBuilderExtensions
{
    public static void BuildStoryCategoryModel(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<StoryCategory>(entity =>
        {
            entity.ToTable("story_categories");
 
            entity.HasKey(e => new { e.StoryId, e.CategoryId });
 
            entity.HasOne(e => e.Story)
                .WithMany(s => s.StoryCategories)
                .HasForeignKey(e => e.StoryId)
                .OnDelete(DeleteBehavior.Cascade);
 
            entity.HasOne(e => e.Category)
                .WithMany(c => c.StoryCategories)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);
 
            entity.HasIndex(e => e.CategoryId)
                .HasDatabaseName("IX_StoryCategory_CategoryId");
        });
    }
}