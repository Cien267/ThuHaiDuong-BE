using System.ComponentModel.DataAnnotations;

namespace ThuHaiDuong.Application.Payloads.InputModels.Admin.Story;

public class UpdateStoryStatusInput
{
    [Required]
    public string Status { get; set; } = null!;
}