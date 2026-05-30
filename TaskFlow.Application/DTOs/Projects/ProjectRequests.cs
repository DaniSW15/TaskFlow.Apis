using TaskFlow.Domain.Enums;

namespace TaskFlow.Application.DTOs.Projects;

public sealed record CreateProjectRequest(
    string Title,
    string? Description,
    Guid ClientId,
    Guid AnalystId,
    DateTime? StartDate,
    DateTime? EndDate);

public sealed record UpdateProjectRequest(
    string Title,
    string? Description,
    ProjectStatus Status,
    DateTime? StartDate,
    DateTime? EndDate);
