using System.ComponentModel.DataAnnotations;

namespace ThuHaiDuong.Application.Payloads.InputModels.Admin.Category;

public class CreateCategoryInput
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = null!;
 
    [MaxLength(120)]
    public string? Slug { get; set; }           // null → tự generate từ Name
 
    [MaxLength(500)]
    public string? Description { get; set; }
 
    public Guid? ParentId { get; set; }         // null → root category
 
    public int SortOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;
}