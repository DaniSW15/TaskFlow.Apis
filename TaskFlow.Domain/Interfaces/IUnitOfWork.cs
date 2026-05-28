namespace TaskFlow.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    IBoardRepository Boards { get; }
    ITaskRepository Tasks { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
