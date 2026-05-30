using TaskFlow.Domain.Common;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Domain.Entities;

public sealed class Project : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ProjectStatus Status { get; set; } = ProjectStatus.Planning;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    // FK — qué cliente encargó este proyecto
    public Guid ClientId { get; set; }
    public Client Client { get; set; } = null!;

    // FK — qué analista gestiona el proyecto
    public Guid AnalystId { get; set; }
    public User Analyst { get; set; } = null!;

    public ICollection<Board> Boards { get; set; } = [];
}
