using ThuHaiDuong.Application.Payloads.ResultModels.Notification;
using ThuHaiDuong.Shared.Models;

namespace ThuHaiDuong.Application.InterfaceService;

public interface INotificationService
{
    Task CreateAndPushAsync(Guid userId, string type, string title,
        string message, Guid? referenceId = null);
    Task<PagedResult<NotificationResult>> GetListAsync(Guid userId, int page, int pageSize);
    Task<int> GetUnreadCountAsync(Guid userId);
    Task MarkReadAsync(Guid notificationId, Guid userId);
    Task MarkAllReadAsync(Guid userId);
    Task DeleteAsync(Guid notificationId, Guid userId);
}