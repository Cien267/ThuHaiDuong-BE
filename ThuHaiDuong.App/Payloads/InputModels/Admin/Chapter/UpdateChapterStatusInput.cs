using System.ComponentModel.DataAnnotations;

namespace ThuHaiDuong.Application.Payloads.InputModels.Admin.Chapter;

public class UpdateChapterStatusInput
{
    [Required]
    public string Status { get; set; } = null!;    // "Draft" | "Published" | "Hidden"
}