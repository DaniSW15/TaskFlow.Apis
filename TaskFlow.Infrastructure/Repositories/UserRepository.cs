using Microsoft.EntityFrameworkCore;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Interfaces;
using TaskFlow.Infrastructure.Persistence;

namespace TaskFlow.Infrastructure.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _context.Users.FindAsync([id], cancellationToken);

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        await _context.Users.FirstOrDefaultAsync(u => u.Email == email.ToLowerInvariant(), cancellationToken);

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        await _context.Users.AnyAsync(u => u.Email == email.ToLowerInvariant(), cancellationToken);

    public async Task<(IReadOnlyList<User> Items, int TotalCount)> GetAllPagedAsync(
        int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.Users.OrderBy(u => u.LastName).ThenBy(u => u.FirstName);
        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        return (items, total);
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken = default) =>
        await _context.Users.AddAsync(user, cancellationToken);

    public void Update(User user) =>
        _context.Users.Update(user);

    public void Delete(User user) =>
        _context.Users.Remove(user); // soft-delete interceptado por AppDbContext
}
