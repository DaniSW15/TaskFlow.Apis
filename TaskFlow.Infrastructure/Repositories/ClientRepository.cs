using Microsoft.EntityFrameworkCore;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Interfaces;
using TaskFlow.Infrastructure.Persistence;

namespace TaskFlow.Infrastructure.Repositories;

public sealed class ClientRepository : IClientRepository
{
    private readonly AppDbContext _context;

    public ClientRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Client?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _context.Clients.FindAsync([id], cancellationToken);

    public async Task<Client?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        await _context.Clients
            .FirstOrDefaultAsync(c => c.Email == email, cancellationToken);

    public async Task<(IReadOnlyList<Client> Items, int TotalCount)> GetAllPagedAsync(
        int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.Clients
            .OrderBy(c => c.Name);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public async Task AddAsync(Client client, CancellationToken cancellationToken = default) =>
        await _context.Clients.AddAsync(client, cancellationToken);

    public void Update(Client client) =>
        _context.Clients.Update(client);

    public void Delete(Client client) =>
        _context.Clients.Remove(client); // el soft-delete lo intercepta AppDbContext
}
