using ThuHaiDuong.Application.InterfaceService.Common;

namespace ThuHaiDuong.Application.Payloads.InputModels.Common;

public class PaginationParams : IPaginationParams
{
    private int _pageNumber = 1;
    private int _pageSize = 10;
    private const int MaxPageSize = 1000;

    public int PageNumber
    {
        get => _pageNumber;
        set => _pageNumber = value < 1 ?  1 : value;
    }
    
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
    }

    public string SortBy { get; set; } = "Id";
    public bool SortDescending { get; set; } = true;
}