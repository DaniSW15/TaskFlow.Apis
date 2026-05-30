using TaskFlow.Domain.Entities;

namespace TaskFlow.Domain.Interfaces;

public interface ICommentRepository
{
    Task<Comment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<Comment> Items, int TotalCount)> GetByTaskIdPagedAsync(Guid taskItemId, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task AddAsync(Comment comment, CancellationToken cancellationToken = default);
    void Delete(Comment comment);
}
