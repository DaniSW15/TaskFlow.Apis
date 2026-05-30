namespace TaskFlow.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    IBoardRepository Boards { get; }
    ITaskRepository Tasks { get; }
    IClientRepository Clients { get; }
    IProjectRepository Projects { get; }
    ICommentRepository Comments { get; }
    ITagRepository Tags { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
