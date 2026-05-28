using Microsoft.EntityFrameworkCore;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Interfaces;
using TaskFlow.Infrastructure.Persistence;

namespace TaskFlow.Infrastructure.Repositories;

public sealed class BoardRepository : IBoardRepository
{
    private readonly AppDbContext _context;

    public BoardRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Board?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _context.Boards.FindAsync([id], cancellationToken);

    public async Task<Board?> GetByIdWithTasksAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _context.Boards
            .Include(b => b.Tasks)
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Board>> GetByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken = default) =>
        await _context.Boards
            .Where(b => b.OwnerId == ownerId)
            .Include(b => b.Tasks)
            .OrderByDescending(b => b.UpdatedAt)
            .ToListAsync(cancellationToken);

    public async Task<(IReadOnlyList<Board> Items, int TotalCount)> GetByOwnerIdPagedAsync(
        Guid ownerId, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.Boards
            .Where(b => b.OwnerId == ownerId)
            .Include(b => b.Tasks)
            .OrderByDescending(b => b.UpdatedAt);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public async Task AddAsync(Board board, CancellationToken cancellationToken = default) =>
        await _context.Boards.AddAsync(board, cancellationToken);

    public void Update(Board board) =>
        _context.Boards.Update(board);

    public void Delete(Board board) =>
        _context.Boards.Remove(board);
}
