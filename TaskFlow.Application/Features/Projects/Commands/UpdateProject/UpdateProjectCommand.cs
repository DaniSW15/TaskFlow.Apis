using MediatR;
using TaskFlow.Domain.Enums;
using TaskFlow.Shared.Common;

namespace TaskFlow.Application.Features.Projects.Commands.UpdateProject;

public sealed record UpdateProjectCommand(
    Guid Id,
    string Title,
    string? Description,
    ProjectStatus Status,
    DateTime? StartDate,
    DateTime? EndDate) : IRequest<Result>;
