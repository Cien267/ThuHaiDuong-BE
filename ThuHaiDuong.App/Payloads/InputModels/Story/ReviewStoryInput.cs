using System.ComponentModel.DataAnnotations;

namespace ThuHaiDuong.Application.Payloads.InputModels.Story;

public class ReviewStoryInput
{
    [Required]
    public bool IsApproved { get; set; }
 
    [MaxLength(1000)]
    public string? RejectionReason { get; set; }
}