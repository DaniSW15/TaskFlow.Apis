using TaskFlow.Domain.Entities;

namespace TaskFlow.Domain.Interfaces;

public interface ITagRepository
{
    Task<Tag?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Tag?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Tag>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Tag tag, CancellationToken cancellationToken = default);
    void Delete(Tag tag);
}
