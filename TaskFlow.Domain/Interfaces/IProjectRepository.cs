using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Domain.Interfaces;

public interface IProjectRepository
{
    Task<Project?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Project?> GetByIdWithBoardsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Project>> GetByClientIdAsync(Guid clientId, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<Project> Items, int TotalCount)> GetByAnalystIdPagedAsync(Guid analystId, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<Project> Items, int TotalCount)> GetByStatusPagedAsync(ProjectStatus status, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task AddAsync(Project project, CancellationToken cancellationToken = default);
    void Update(Project project);
    void Delete(Project project);
}
