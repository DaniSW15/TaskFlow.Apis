using TaskFlow.Domain.Entities;

namespace TaskFlow.Domain.Interfaces;

public interface IBoardRepository
{
    Task<Board?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Board?> GetByIdWithTasksAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Board>> GetByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<Board> Items, int TotalCount)> GetByOwnerIdPagedAsync(Guid ownerId, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task AddAsync(Board board, CancellationToken cancellationToken = default);
    void Update(Board board);
    void Delete(Board board);
}
