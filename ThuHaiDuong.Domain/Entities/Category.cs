using Microsoft.EntityFrameworkCore;

namespace ThuHaiDuong.Domain.Entities;

public class Category : BaseEntity
{
    public string Name { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string? Description { get; set; }
    public Guid? ParentId { get; set; }
    public int SortOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;
 
    // Navigation
    public virtual Category? Parent { get; set; }
    public virtual ICollection<Category> Children { get; set; } = [];
    public virtual ICollection<StoryCategory> StoryCategories { get; set; } = [];
}
 
public static class CategoryModelBuilderExtensions
{
    public static void BuildCategoryModel(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("categories");
            entity.HasKey(e => e.Id);
 
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);
 
            entity.Property(e => e.Slug)
                .IsRequired()
                .HasMaxLength(120);
 
            entity.Property(e => e.Description)
                .HasMaxLength(500);
 
            entity.Property(e => e.ParentId)
                .IsRequired(false);
 
            entity.Property(e => e.SortOrder)
                .IsRequired()
                .HasDefaultValue(0);
 
            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValue(true);
 
            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasColumnType("datetime2");
 
            entity.Property(e => e.UpdatedAt)
                .IsRequired()
                .HasColumnType("datetime2");
            
            entity.Property(e => e.DeletedAt)
                .IsRequired()
                .HasColumnType("datetime2");
 
            entity.HasIndex(e => e.Slug)
                .IsUnique()
                .HasDatabaseName("IX_Category_Slug");
 
            entity.HasIndex(e => e.ParentId)
                .HasDatabaseName("IX_Category_ParentId");
 
            entity.HasIndex(e => new { e.IsActive, e.SortOrder })
                .HasDatabaseName("IX_Category_IsActive_SortOrder");
 
            // Self-referencing: category can have a parent category
            entity.HasOne(e => e.Parent)
                .WithMany(e => e.Children)
                .HasForeignKey(e => e.ParentId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);
        });
    }
}