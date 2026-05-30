using MediatR;
using TaskFlow.Domain.Enums;
using TaskFlow.Shared.Common;

namespace TaskFlow.Application.Features.Projects.Commands.CreateProject;

public sealed record CreateProjectCommand(
    string Title,
    string? Description,
    Guid ClientId,
    Guid AnalystId,
    DateTime? StartDate,
    DateTime? EndDate) : IRequest<Result<Guid>>;
