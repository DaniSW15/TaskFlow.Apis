using TaskFlow.Domain.Enums;

namespace TaskFlow.Application.DTOs.Projects;

public sealed record ProjectDto(
    Guid Id,
    string Title,
    string? Description,
    ProjectStatus Status,
    DateTime? StartDate,
    DateTime? EndDate,
    Guid ClientId,
    string ClientName,
    Guid AnalystId,
    string AnalystFullName,
    int BoardCount,
    DateTime CreatedAt,
    DateTime UpdatedAt);
