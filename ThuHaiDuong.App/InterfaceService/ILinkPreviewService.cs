using ThuHaiDuong.Application.Payloads.ResultModels.Utils;

namespace ThuHaiDuong.Application.InterfaceService;

public interface ILinkPreviewService
{
    Task<LinkPreviewResult> GetPreviewAsync(string url);
}