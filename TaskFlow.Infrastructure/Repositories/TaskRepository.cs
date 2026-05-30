using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Common.Cursors;
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

    public async Task<TaskItem?> GetByIdWithTagsAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _context.Tasks
            .Include(t => t.Assignee)
            .Include(t => t.Tags)
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

    public async Task<(IReadOnlyList<TaskItem> Items, string? NextCursor)> GetByBoardIdWithCursorAsync(
        Guid boardId,
        DateTime? afterCreatedAt,
        Guid? afterId,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        // Ordenamos siempre por (CreatedAt ASC, Id ASC) — determinista y cubierto por índice
        var query = _context.Tasks
            .Where(t => t.BoardId == boardId)
            .Include(t => t.Assignee)
            .AsQueryable();

        // Aplicar el filtro del cursor: traer sólo filas "después" del último ítem visto
        if (afterCreatedAt.HasValue && afterId.HasValue)
        {
            // WHERE (CreatedAt > afterCreatedAt)
            //    OR (CreatedAt = afterCreatedAt AND Id > afterId)
            query = query.Where(t =>
                t.CreatedAt > afterCreatedAt.Value ||
                (t.CreatedAt == afterCreatedAt.Value && t.Id.CompareTo(afterId.Value) > 0));
        }

        // Pedimos pageSize + 1 para detectar si hay página siguiente sin COUNT(*)
        var items = await query
            .OrderBy(t => t.CreatedAt)
            .ThenBy(t => t.Id)
            .Take(pageSize + 1)
            .ToListAsync(cancellationToken);

        string? nextCursor = null;

        if (items.Count > pageSize)
        {
            // Hay al menos una fila más: la descartamos y generamos el cursor
            items.RemoveAt(items.Count - 1);
            var last = items[^1];
            nextCursor = new TaskCursor(last.CreatedAt, last.Id).Encode();
        }

        return (items, nextCursor);
    }

    public async Task AddAsync(TaskItem task, CancellationToken cancellationToken = default) =>
        await _context.Tasks.AddAsync(task, cancellationToken);

    public void Update(TaskItem task) =>
        _context.Tasks.Update(task);

    public void Delete(TaskItem task) =>
        _context.Tasks.Remove(task);
}
