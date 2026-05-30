using Microsoft.EntityFrameworkCore;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Interfaces;
using TaskFlow.Infrastructure.Persistence;

namespace TaskFlow.Infrastructure.Repositories;

public sealed class TagRepository : ITagRepository
{
    private readonly AppDbContext _context;

    public TagRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Tag?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _context.Tags.FindAsync([id], cancellationToken);

    public async Task<Tag?> GetByNameAsync(string name, CancellationToken cancellationToken = default) =>
        await _context.Tags.FirstOrDefaultAsync(t => t.Name == name, cancellationToken);

    public async Task<IReadOnlyList<Tag>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _context.Tags.OrderBy(t => t.Name).ToListAsync(cancellationToken);

    public async Task AddAsync(Tag tag, CancellationToken cancellationToken = default) =>
        await _context.Tags.AddAsync(tag, cancellationToken);

    public void Delete(Tag tag) =>
        _context.Tags.Remove(tag);
}
