using TaskFlow.Domain.Entities;

namespace TaskFlow.Domain.Interfaces;

public interface ITaskRepository
{
    Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<TaskItem?> GetByIdWithTagsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TaskItem>> GetByBoardIdAsync(Guid boardId, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<TaskItem> Items, int TotalCount)> GetByBoardIdPagedAsync(Guid boardId, int pageNumber, int pageSize, CancellationToken cancellationToken = default);

    /// <summary>
    /// Paginación con cursores (keyset pagination).
    /// No ejecuta COUNT(*) ni OFFSET; escala bien en tablas grandes.
    /// afterCreatedAt + afterId definen el punto de inicio exclusivo.
    /// Devuelve pageSize + 1 internamente para saber si hay página siguiente.
    /// </summary>
    Task<(IReadOnlyList<TaskItem> Items, string? NextCursor)> GetByBoardIdWithCursorAsync(
        Guid boardId,
        DateTime? afterCreatedAt,
        Guid? afterId,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task AddAsync(TaskItem task, CancellationToken cancellationToken = default);
    void Update(TaskItem task);
    void Delete(TaskItem task);
}
