using TaskFlow.Domain.Common;

namespace TaskFlow.Domain.Entities;

public sealed class Board : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid OwnerId { get; set; }

    // FK opcional — un board puede pertenecer a un proyecto
    public Guid? ProjectId { get; set; }

    public User Owner { get; set; } = null!;
    public Project? Project { get; set; }
    public ICollection<TaskItem> Tasks { get; set; } = [];
}
