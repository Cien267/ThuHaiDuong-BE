using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using ThuHaiDuong.Application.Hubs;
using ThuHaiDuong.Application.InterfaceService;
using ThuHaiDuong.Application.Payloads.Responses;
using ThuHaiDuong.Application.Payloads.ResultModels.Notification;
using ThuHaiDuong.Domain.Entities;
using ThuHaiDuong.Domain.InterfaceRepositories;
using ThuHaiDuong.Shared.Models;

namespace ThuHaiDuong.Application.ImplementService;

public class NotificationService : INotificationService
{
    private readonly IBaseRepository<Notification> _repo;
    private readonly IHubContext<NotificationHub> _hub;

    public NotificationService(
        IBaseRepository<Notification> repo,
        IHubContext<NotificationHub> hub)
    {
        _repo = repo;
        _hub  = hub;
    }

    public async Task CreateAndPushAsync(
        Guid userId, string type, string title,
        string message, Guid? referenceId = null)
    {
        var notification = new Notification
        {
            UserId      = userId,
            Type        = type,
            Title       = title,
            Message     = message,
            IsRead      = false,
            ReferenceId = referenceId,
        };

        await _repo.CreateAsync(notification);

        // Push realtime đến user group
        var result = new NotificationResult
        {
            Id          = notification.Id,
            Type        = notification.Type,
            Title       = notification.Title,
            Message     = notification.Message,
            IsRead      = false,
            ReferenceId = notification.ReferenceId,
            CreatedAt   = notification.CreatedAt,
        };

        await _hub.Clients
            .Group(userId.ToString())
            .SendAsync("ReceiveNotification", result);
    }

    public async Task<PagedResult<NotificationResult>> GetListAsync(
        Guid userId, int page, int pageSize)
    {
        var query = _repo.BuildQueryable([], n => n.UserId == userId && !n.DeletedAt.HasValue);
        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(n => n.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(n => new NotificationResult
            {
                Id          = n.Id,
                Type        = n.Type,
                Title       = n.Title,
                Message     = n.Message,
                IsRead      = n.IsRead,
                ReferenceId = n.ReferenceId,
                CreatedAt   = n.CreatedAt,
            })
            .ToListAsync();

        return new PagedResult<NotificationResult>(items, total, page, pageSize);
    }

    public async Task<int> GetUnreadCountAsync(Guid userId)
    {
        var query = _repo.BuildQueryable([],
            n => n.UserId == userId && !n.IsRead && !n.DeletedAt.HasValue);
        return await query.CountAsync();
    }

    public async Task MarkReadAsync(Guid notificationId, Guid userId)
    {
        var n = await _repo.GetByIdAsync(notificationId)
            ?? throw new ResponseErrorObject("Notification not found", StatusCodes.Status404NotFound);
        if (n.UserId != userId)
            throw new ResponseErrorObject("Forbidden", StatusCodes.Status403Forbidden);

        n.IsRead = true;
        await _repo.UpdateAsync(n);
    }

    public async Task MarkAllReadAsync(Guid userId)
    {
        var query = _repo.BuildQueryable([],
            n => n.UserId == userId && !n.IsRead && !n.DeletedAt.HasValue);
        await query.ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true));
    }

    public async Task DeleteAsync(Guid notificationId, Guid userId)
    {
        var n = await _repo.GetByIdAsync(notificationId)
            ?? throw new ResponseErrorObject("Notification not found", StatusCodes.Status404NotFound);
        if (n.UserId != userId)
            throw new ResponseErrorObject("Forbidden", StatusCodes.Status403Forbidden);
        await _repo.DeleteAsync(notificationId);
    }
}