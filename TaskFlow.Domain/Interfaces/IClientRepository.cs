using TaskFlow.Domain.Entities;

namespace TaskFlow.Domain.Interfaces;

public interface IClientRepository
{
    Task<Client?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Client?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<Client> Items, int TotalCount)> GetAllPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task AddAsync(Client client, CancellationToken cancellationToken = default);
    void Update(Client client);
    void Delete(Client client);
}
