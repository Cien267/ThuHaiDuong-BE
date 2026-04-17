using ThuHaiDuong.Application.Payloads.InputModels.Common;

namespace ThuHaiDuong.Application.Payloads.InputModels.Affiliate;

public class AffiliateLinkQuery : PaginationParams
{
    public string? Name { get; set; }
    public string? Placement { get; set; }
    public bool? IsActive { get; set; }
}