namespace ThuHaiDuong.Application.InterfaceService;

public interface ICurrentUserService
{
    Guid? GetUserId();
    string GetUserName();
    string GetEmail();
    IEnumerable<string> GetRoles();
    IEnumerable<string> GetPermissions();
}