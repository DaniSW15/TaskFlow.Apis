using Microsoft.EntityFrameworkCore;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using TaskFlow.Domain.Interfaces;
using TaskFlow.Infrastructure.Persistence;

namespace TaskFlow.Infrastructure.Repositories;

public sealed class ProjectRepository : IProjectRepository
{
    private readonly AppDbContext _context;

    public ProjectRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Project?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _context.Projects
            .Include(p => p.Client)
            .Include(p => p.Analyst)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<Project?> GetByIdWithBoardsAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _context.Projects
            .Include(p => p.Client)
            .Include(p => p.Analyst)
            .Include(p => p.Boards)
                .ThenInclude(b => b.Tasks)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Project>> GetByClientIdAsync(Guid clientId, CancellationToken cancellationToken = default) =>
        await _context.Projects
            .Where(p => p.ClientId == clientId)
            .Include(p => p.Analyst)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<(IReadOnlyList<Project> Items, int TotalCount)> GetByAnalystIdPagedAsync(
        Guid analystId, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.Projects
            .Where(p => p.AnalystId == analystId)
            .Include(p => p.Client)
            .OrderByDescending(p => p.UpdatedAt);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public async Task<(IReadOnlyList<Project> Items, int TotalCount)> GetByStatusPagedAsync(
        ProjectStatus status, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.Projects
            .Where(p => p.Status == status)
            .Include(p => p.Client)
            .Include(p => p.Analyst)
            .OrderByDescending(p => p.UpdatedAt);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public async Task AddAsync(Project project, CancellationToken cancellationToken = default) =>
        await _context.Projects.AddAsync(project, cancellationToken);

    public void Update(Project project) =>
        _context.Projects.Update(project);

    public void Delete(Project project) =>
        _context.Projects.Remove(project); // el soft-delete lo intercepta AppDbContext
}
