using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Persistence;

public sealed class AppDbContext : DbContext
{
    private readonly ICurrentUserService? _currentUser;

    public AppDbContext(DbContextOptions<AppDbContext> options, ICurrentUserService? currentUser = null)
        : base(options)
    {
        _currentUser = currentUser;
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Board> Boards => Set<Board>();
    public DbSet<TaskItem> Tasks => Set<TaskItem>();
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<Tag> Tags => Set<Tag>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // Global query filters — automatically exclude soft-deleted rows
        modelBuilder.Entity<User>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Board>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<TaskItem>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Client>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Project>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Comment>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Tag>().HasQueryFilter(e => !e.IsDeleted);

        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var userId = _currentUser?.IsAuthenticated == true ? _currentUser.UserId : (Guid?)null;

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = now;
                    entry.Entity.UpdatedAt = now;
                    entry.Entity.CreatedBy = userId;
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAt = now;
                    break;

                case EntityState.Deleted:
                    // Convert hard-delete into soft-delete
                    entry.State = EntityState.Modified;
                    entry.Entity.IsDeleted = true;
                    entry.Entity.DeletedAt = now;
                    entry.Entity.DeletedBy = userId;
                    entry.Entity.UpdatedAt = now;
                    break;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
