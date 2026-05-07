using Microsoft.EntityFrameworkCore;
using ThuHaiDuong.Application.Payloads.InputModels.Comment;
using ThuHaiDuong.Application.Payloads.ResultModels.Comment;
using ThuHaiDuong.Domain.Entities;
using ThuHaiDuong.Domain.InterfaceRepositories;
using ThuHaiDuong.Infrastructure.DataContext;

namespace ThuHaiDuong.Infrastructure.ImplementRepositories;

public class CommentRepository : ICommentRepository
{
    private readonly AppDbContext _context;
 
    public CommentRepository(AppDbContext context)
    {
        _context = context;
    }
 
    public async Task<Comment?> GetByIdAsync(Guid id)
    {
        return await _context.Comments
            .FirstOrDefaultAsync(c => c.Id == id && !c.DeletedAt.HasValue);
    }
}