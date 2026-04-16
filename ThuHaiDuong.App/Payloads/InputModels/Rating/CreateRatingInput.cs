using System.ComponentModel.DataAnnotations;

namespace ThuHaiDuong.Application.Payloads.InputModels.User.Rating;

public class CreateRatingInput
{
    [Required]
    public Guid StoryId { get; set; }
 
    [Required, Range(1, 5)]
    public int Score { get; set; }
 
    [MaxLength(2000)]
    public string? Comment { get; set; }
}