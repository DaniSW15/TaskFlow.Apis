using TaskFlow.Domain.Interfaces;
using TaskFlow.Infrastructure.Persistence;

namespace TaskFlow.Infrastructure.Repositories;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private bool _disposed;

    public IUserRepository Users { get; }
    public IBoardRepository Boards { get; }
    public ITaskRepository Tasks { get; }
    public IClientRepository Clients { get; }
    public IProjectRepository Projects { get; }
    public ICommentRepository Comments { get; }
    public ITagRepository Tags { get; }

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
        Users = new UserRepository(context);
        Boards = new BoardRepository(context);
        Tasks = new TaskRepository(context);
        Clients = new ClientRepository(context);
        Projects = new ProjectRepository(context);
        Comments = new CommentRepository(context);
        Tags = new TagRepository(context);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);

    public void Dispose()
    {
        if (!_disposed)
        {
            _context.Dispose();
            _disposed = true;
        }
    }
}
