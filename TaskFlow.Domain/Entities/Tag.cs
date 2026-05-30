using TaskFlow.Domain.Common;

namespace TaskFlow.Domain.Entities;

public sealed class Tag : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = "#6366F1"; // color hex por defecto

    public ICollection<TaskItem> Tasks { get; set; } = [];
}
