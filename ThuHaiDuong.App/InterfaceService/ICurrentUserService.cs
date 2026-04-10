namespace ThuHaiDuong.Application.InterfaceService;

public interface ICurrentUserService
{
    Guid? GetUserId();
    Guid? GetBrokerageId();
    string GetUserName();
    string GetEmail();
    IEnumerable<string> GetRoles();
    IEnumerable<string> GetPermissions();
}