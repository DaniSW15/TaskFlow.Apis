using TaskFlow.Domain.Common;

namespace TaskFlow.Domain.Entities;

public sealed class Comment : BaseEntity
{
    public string Content { get; set; } = string.Empty;

    public Guid TaskItemId { get; set; }
    public TaskItem TaskItem { get; set; } = null!;

    public Guid AuthorId { get; set; }
    public User Author { get; set; } = null!;
}
