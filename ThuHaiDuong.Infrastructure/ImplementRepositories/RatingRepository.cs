using Microsoft.EntityFrameworkCore;
using ThuHaiDuong.Domain.Entities;
using ThuHaiDuong.Domain.InterfaceRepositories;
using ThuHaiDuong.Infrastructure.DataContext;

namespace ThuHaiDuong.Infrastructure.ImplementRepositories;

public class RatingRepository : IRatingRepository
{
    private readonly AppDbContext _context;
 
    public RatingRepository(AppDbContext context)
    {
        _context = context;
    }
 
    public async Task<Rating?> GetUserRatingAsync(Guid userId, Guid storyId)
    {
        return await _context.Ratings
            .Include(r => r.User)
            .FirstOrDefaultAsync(r =>
                r.UserId == userId &&
                r.StoryId == storyId &&
                !r.IsDeleted);
    }
 
    public async Task<bool> UserHasRatedAsync(Guid userId, Guid storyId)
    {
        return await _context.Ratings
            .AnyAsync(r =>
                r.UserId == userId &&
                r.StoryId == storyId &&
                !r.IsDeleted);
    }
 
    public async Task<Dictionary<int, int>> GetScoreDistributionAsync(Guid storyId)
    {
        var distribution = await _context.Ratings
            .Where(r => r.StoryId == storyId && !r.IsDeleted)
            .GroupBy(r => r.Score)
            .Select(g => new { Score = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Score, x => x.Count);
 
        // Đảm bảo có đủ key 1–5 dù không có rating
        for (var i = 1; i <= 5; i++)
        {
            if (!distribution.ContainsKey(i))
                distribution[i] = 0;
        }
 
        return distribution;
    }
 
    public async Task SyncStoryRatingAsync(Guid storyId)
    {
        var ratings = await _context.Ratings
            .Where(r => r.StoryId == storyId && !r.IsDeleted)
            .ToListAsync();
 
        var count   = ratings.Count;
        var average = count > 0
            ? Math.Round((decimal)ratings.Average(r => r.Score), 2)
            : 0m;
 
        await _context.Stories
            .Where(s => s.Id == storyId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(x => x.RatingCount,   count)
                .SetProperty(x => x.AverageRating, average)
                .SetProperty(x => x.UpdatedAt,     DateTime.UtcNow));
    }
}