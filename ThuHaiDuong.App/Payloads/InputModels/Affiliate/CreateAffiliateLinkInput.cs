using System.ComponentModel.DataAnnotations;
using ThuHaiDuong.Shared.Constants;

namespace ThuHaiDuong.Application.Payloads.InputModels.Affiliate;

public class CreateAffiliateLinkInput
{
    [Required, MaxLength(200)]
    public string Name { get; set; } = null!;
 
    [Required, MaxLength(2000)]
    public string TargetUrl { get; set; } = null!;
 
    // null → tự generate ngẫu nhiên (8 ký tự)
    [MaxLength(50)]
    public string? TrackingCode { get; set; }
 
    [Required]
    public string Placement { get; set; } = AffiliatePlacement.InChapter;
 
    public int Priority { get; set; } = 0;
    public bool IsActive { get; set; } = true;
 
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
 
    // Gắn vào story/chapter cụ thể ngay lúc tạo (optional)
    public List<Guid> StoryIds { get; set; } = [];
    public List<Guid> ChapterIds { get; set; } = [];
}