using Microsoft.EntityFrameworkCore;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Interfaces;
using TaskFlow.Infrastructure.Persistence;

namespace TaskFlow.Infrastructure.Repositories;

public sealed class TaskRepository : ITaskRepository
{
    private readonly AppDbContext _context;

    public TaskRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _context.Tasks
            .Include(t => t.Assignee)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public async Task<IReadOnlyList<TaskItem>> GetByBoardIdAsync(Guid boardId, CancellationToken cancellationToken = default) =>
        await _context.Tasks
            .Where(t => t.BoardId == boardId)
            .Include(t => t.Assignee)
            .OrderBy(t => t.Priority)
            .ThenBy(t => t.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<(IReadOnlyList<TaskItem> Items, int TotalCount)> GetByBoardIdPagedAsync(
        Guid boardId, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.Tasks
            .Where(t => t.BoardId == boardId)
            .Include(t => t.Assignee)
            .OrderBy(t => t.Priority)
            .ThenBy(t => t.CreatedAt);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public async Task AddAsync(TaskItem task, CancellationToken cancellationToken = default) =>
        await _context.Tasks.AddAsync(task, cancellationToken);

    public void Update(TaskItem task) =>
        _context.Tasks.Update(task);

    public void Delete(TaskItem task) =>
        _context.Tasks.Remove(task);
}
