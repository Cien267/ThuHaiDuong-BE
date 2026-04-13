using Microsoft.EntityFrameworkCore;

namespace ThuHaiDuong.Domain.Entities;

public class Tag : BaseEntity
{
    public string Name { get; set; } = null!;
    public string Slug { get; set; } = null!;
 
    // Navigation
    public virtual ICollection<StoryTag> StoryTags { get; set; } = [];
}
 
public static class TagModelBuilderExtensions
{
    public static void BuildTagModel(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Tag>(entity =>
        {
            entity.ToTable("tags");
            entity.HasKey(e => e.Id);
 
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);
 
            entity.Property(e => e.Slug)
                .IsRequired()
                .HasMaxLength(120);
 
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
                .HasDatabaseName("IX_Tag_Slug");
 
            entity.HasIndex(e => e.Name)
                .HasDatabaseName("IX_Tag_Name");
        });
    }
}