using TaskFlow.Domain.Common;

namespace TaskFlow.Domain.Entities;

public sealed class Client : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Company { get; set; }
    public string? Notes { get; set; }

    public ICollection<Project> Projects { get; set; } = [];
}
