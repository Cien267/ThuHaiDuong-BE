using System.ComponentModel.DataAnnotations;

namespace ThuHaiDuong.Application.Payloads.InputModels.Story;

public class UpdateStoryStatusInput
{
    [Required]
    public string Status { get; set; } = null!;
}