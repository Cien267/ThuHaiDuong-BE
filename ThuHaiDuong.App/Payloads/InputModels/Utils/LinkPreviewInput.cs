using System.ComponentModel.DataAnnotations;

namespace ThuHaiDuong.Application.Payloads.InputModels.Utils;

public class LinkPreviewInput
{
    [Required, Url]
    public string Url { get; set; } = null!;
}