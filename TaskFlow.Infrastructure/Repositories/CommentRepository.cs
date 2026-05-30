using Microsoft.EntityFrameworkCore;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Interfaces;
using TaskFlow.Infrastructure.Persistence;

namespace TaskFlow.Infrastructure.Repositories;

public sealed class CommentRepository : ICommentRepository
{
    private readonly AppDbContext _context;

    public CommentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Comment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _context.Comments
            .Include(c => c.Author)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<(IReadOnlyList<Comment> Items, int TotalCount)> GetByTaskIdPagedAsync(
        Guid taskItemId, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.Comments
            .Where(c => c.TaskItemId == taskItemId)
            .Include(c => c.Author)
            .OrderByDescending(c => c.CreatedAt);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public async Task AddAsync(Comment comment, CancellationToken cancellationToken = default) =>
        await _context.Comments.AddAsync(comment, cancellationToken);

    public void Delete(Comment comment) =>
        _context.Comments.Remove(comment);
}
