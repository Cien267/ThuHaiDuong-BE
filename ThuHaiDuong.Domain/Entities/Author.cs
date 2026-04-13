using Microsoft.EntityFrameworkCore;

namespace ThuHaiDuong.Domain.Entities;

public class Author : BaseEntity
{
    public string Name { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string? PenName { get; set; }
 
    // "CN" | "VN" | "KR" | "JP" ...
    public string? Country { get; set; }
    public string? Description { get; set; }
    public string? AvatarUrl { get; set; }
 
    // Navigation
    public virtual ICollection<Story> Stories { get; set; } = [];
}
 
public static class AuthorModelBuilderExtensions
{
    public static void BuildAuthorModel(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Author>(entity =>
        {
            entity.ToTable("authors");
            entity.HasKey(e => e.Id);
 
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(200);
 
            entity.Property(e => e.Slug)
                .IsRequired()
                .HasMaxLength(250);
 
            entity.Property(e => e.PenName)
                .HasMaxLength(200);
 
            entity.Property(e => e.Country)
                .HasMaxLength(10);
 
            entity.Property(e => e.Description)
                .HasMaxLength(2000);
 
            entity.Property(e => e.AvatarUrl)
                .HasMaxLength(500);
 
            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasColumnType("datetime2");
 
            entity.Property(e => e.UpdatedAt)
                .IsRequired()
                .HasColumnType("datetime2");
 
            entity.Property(e => e.DeletedAt)
                .IsRequired()
                .HasColumnType("datetime2");
 
            // URL /tac-gia/{slug}
            entity.HasIndex(e => e.Slug)
                .IsUnique()
                .HasDatabaseName("IX_Author_Slug");
 
            entity.HasIndex(e => e.Name)
                .HasDatabaseName("IX_Author_Name");
 
            entity.HasIndex(e => e.Country)
                .HasDatabaseName("IX_Author_Country");
        });
    }
}