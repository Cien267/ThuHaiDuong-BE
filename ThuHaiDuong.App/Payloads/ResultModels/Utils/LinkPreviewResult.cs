namespace ThuHaiDuong.Application.Payloads.ResultModels.Utils;

public class LinkPreviewResult
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Image { get; set; }       // og:image URL
    public string? SiteName { get; set; }    // og:site_name
    public string  Url { get; set; } = null!; // URL gốc
}