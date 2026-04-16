using System.ComponentModel.DataAnnotations;

namespace ThuHaiDuong.Application.Payloads.InputModels.Category;

public class UpdateCategoryInput
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = null!;
 
    [MaxLength(120)]
    public string? Slug { get; set; }
 
    [MaxLength(500)]
    public string? Description { get; set; }
 
    public Guid? ParentId { get; set; }
 
    public int SortOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;
}