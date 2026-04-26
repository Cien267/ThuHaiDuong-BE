using Microsoft.EntityFrameworkCore;
using ThuHaiDuong.Domain.Entities;
using ThuHaiDuong.Domain.InterfaceRepositories;
using ThuHaiDuong.Infrastructure.DataContext;

namespace ThuHaiDuong.Infrastructure.ImplementRepositories;

public class AuthRepository : IAuthRepository
{
    private readonly AppDbContext _context;
 
    public AuthRepository(AppDbContext context)
    {
        _context = context;
    }
 
    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u =>
                u.Email == email.ToLower().Trim() &&
                !u.IsDeleted);
    }
 
    public async Task<User?> GetByUserNameAsync(string userName)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u =>
                u.UserName == userName &&
                !u.IsDeleted);
    }
 
    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _context.Users
            .AnyAsync(u =>
                u.Email == email.ToLower().Trim() &&
                !u.IsDeleted);
    }
 
    public async Task<bool> UserNameExistsAsync(string userName)
    {
        return await _context.Users
            .AnyAsync(u =>
                u.UserName == userName &&
                !u.IsDeleted);
    }
 
    public async Task<RefreshToken?> GetActiveRefreshTokenAsync(Guid userId)
    {
        return await _context.RefreshTokens
            .FirstOrDefaultAsync(t =>
                t.UserId == userId &&
                !t.IsRevoked &&
                t.ExpiresAt > DateTime.UtcNow &&
                !t.IsDeleted);
    }
 
    public async Task<RefreshToken?> GetByTokenValueAsync(string token)
    {
        return await _context.RefreshTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t =>
                t.Token == token &&
                !t.IsDeleted);
    }
 
    public async Task RevokeAllUserTokensAsync(Guid userId)
    {
        await _context.RefreshTokens
            .Where(t => t.UserId == userId && !t.IsRevoked)
            .ExecuteUpdateAsync(t =>
                t.SetProperty(x => x.IsRevoked, true)
                    .SetProperty(x => x.UpdatedAt, DateTime.UtcNow));
    }
}