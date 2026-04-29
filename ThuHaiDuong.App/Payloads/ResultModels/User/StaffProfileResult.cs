using System.Linq.Expressions;
using ThuHaiDuong.Application.Payloads.ResultModels.Common;

namespace ThuHaiDuong.Application.Payloads.ResultModels.User;

public class StaffProfileResult : DataResponseBase
{
    public string UserName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? FullName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Avatar { get; set; }
    public string Role { get; set; } = null!;
    public DateTime? LastLoginAt { get; set; }
 
    public static Expression<Func<Domain.Entities.User, StaffProfileResult>> FromUser =>
        u => new StaffProfileResult
        {
            Id          = u.Id,
            UserName    = u.UserName,
            Email       = u.Email,
            FullName    = u.FullName,
            PhoneNumber = u.PhoneNumber,
            Avatar      = u.Avatar,
            Role        = u.Role,
            LastLoginAt = u.LastLoginAt,
            CreatedAt   = u.CreatedAt,
        };
}